using System.Globalization;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

using StarCitizen.EventsCal.Hubs;

using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace StarCitizen.EventsCal.Services;

internal class GoogleDocSourceFetcher : IHostedService
{
    static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
    static readonly string ApplicationName = "Google Docs API .NET Quickstart";
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<GoogleDocSourceFetcherOptions> _options;
    private readonly ILogger<GoogleDocSourceFetcher> _logger;
    private readonly EventCalendarStoreBacking _eventStore;
    private CancellationTokenSource _ctsStop;

    public GoogleDocSourceFetcher(IServiceProvider serviceProvider,
                                  IOptions<GoogleDocSourceFetcherOptions> options,
                                  ILogger<GoogleDocSourceFetcher> logger,
                                  EventCalendarStoreBacking eventStore)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
        _eventStore = eventStore;
        _ctsStop = new CancellationTokenSource();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting up..");

        _ = LoadDataPeriodicallyAsync(_ctsStop.Token);
        _ = LoadDataAsync();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _ctsStop?.Cancel();

        return Task.CompletedTask;
    }

    private async Task LoadDataPeriodicallyAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken)
                .ContinueWith(t => { });

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await LoadDataAsync();
        }
    }

    private async Task LoadDataAsync()
    {
#if DEBUG
        var credential = GoogleCredential.FromFile("credentials.json")
            .CreateScoped(Scopes);
#else
        // In production, this should be read from a secret/configuration value
        var credential = GoogleCredential.FromJson(_options.Value.Credentials)
            .CreateScoped(Scopes);
#endif

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        string nameRange = $"{_options.Value.Sheet}!{_options.Value.NameRange}";
        string dateRange = $"{_options.Value.Sheet}!{_options.Value.DateRange}";

        BatchGetRequest batch = service.Spreadsheets.Values.BatchGet(_options.Value.DocumentId);
        batch.Ranges = new[] { nameRange, dateRange };
        batch.DateTimeRenderOption = SpreadsheetsResource.ValuesResource.BatchGetRequest.DateTimeRenderOptionEnum.SERIALNUMBER;

        try
        {
            var batchResponse = await batch.ExecuteAsync();

            var nameValueRange = batchResponse.ValueRanges[0];
            var dateValueRange = batchResponse.ValueRanges[1];

            int nameUBound = nameValueRange.Values.Count - 1;

            using (var changeSet = _eventStore.CreateChangeSet())
            {
                for (var i = 0; i < dateValueRange.Values.Count; i++)
                {
                    string? name = i > nameUBound ? null : nameValueRange.Values[i][0] as string;
                    var fromTime = DateTime.SpecifyKind(ParseDate(dateValueRange.Values[i][0] as string), DateTimeKind.Utc);
                    var toTime = DateTime.SpecifyKind(ParseDate(dateValueRange.Values[i][1] as string), DateTimeKind.Utc);

                    changeSet.Add(new CalendarEvent()
                    {
                        EventName = name ?? "Unnamed Event",
                        StartTime = fromTime,
                        EndTime = toTime,
                    });
                }

                if (changeSet.Commit())
                {
                    using var scope = _serviceProvider.CreateScope();

                    var hub = scope.ServiceProvider.GetRequiredService<IHubContext<EventsHub, IEventsHub>>();

                    await hub.Clients.All.PushAll(_eventStore.CurrentAndUpcomingEvents());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get values");
        }
    }

    private DateTime ParseDate(string? value)
    {
        var enUS = new CultureInfo("en-US");

        if (!DateTime.TryParseExact(value, _options.Value.DateFormats, enUS, System.Globalization.DateTimeStyles.None, out DateTime timeValue))
        {
            DateTime.TryParseExact(value + $" {DateTime.Now.Year}", _options.Value.DateFormats, enUS, System.Globalization.DateTimeStyles.None, out timeValue);
        }

        if (timeValue == DateTime.MinValue)
        {
            return timeValue;
        }

        // Because it's unlikely that events will be planned > 8 months in advance;
        var diff = timeValue - DateTime.UtcNow;
        if (diff.TotalDays > 180)
        {
            timeValue = timeValue.AddYears(-1);
        }
        else if (diff.TotalDays < -180)
        {
            timeValue = timeValue.AddYears(1);
        }

        return timeValue;
    }
}

public class GoogleDocSourceFetcherOptions
{
    public string DocumentId { get; set; } = string.Empty;
    public string Sheet { get; set; } = string.Empty;
    public string DateRange { get; set; } = string.Empty;
    public string NameRange { get; set; } = string.Empty;
    public string[] DateFormats { get; set; } = Array.Empty<string>();
    public string Credentials { get; set; } = string.Empty;

}