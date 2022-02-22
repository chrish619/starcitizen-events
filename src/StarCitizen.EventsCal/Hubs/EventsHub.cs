
using StarCitizen.EventsCal.Services;

namespace StarCitizen.EventsCal.Hubs;

public class EventsHub : Microsoft.AspNetCore.SignalR.Hub<IEventsHub>
{
    private readonly ILogger<EventsHub> _logger;
    private readonly EventCalendarStoreBacking _store;

    public EventsHub(ILogger<EventsHub> logger, EventCalendarStoreBacking store)
    {
        _logger = logger;
        _store = store;
    }

    public override Task OnConnectedAsync()
    {
        this.Clients.Caller.PushAll(_store.CurrentAndUpcomingEvents());

        return base.OnConnectedAsync();
    }
}