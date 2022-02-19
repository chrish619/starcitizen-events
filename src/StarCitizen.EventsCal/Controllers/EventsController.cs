using Microsoft.AspNetCore.Mvc;

namespace StarCitizen.EventsCal.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController : ControllerBase
{
    private readonly ILogger<EventsController> _logger;
    private readonly EventCalendarStoreBacking _eventCalendarStoreBacking;

    public EventsController(ILogger<EventsController> logger, EventCalendarStoreBacking eventCalendarStoreBacking)
    {
        _logger = logger;
        _eventCalendarStoreBacking = eventCalendarStoreBacking;
    }

    [HttpGet()]
    public IEnumerable<CalendarEvent> Get()
    {
        return _eventCalendarStoreBacking
            .CurrentAndUpcomingEvents();
    }

    [HttpGet("live")]
    public IEnumerable<CalendarEvent> GetLiveEvents()
    {
         return _eventCalendarStoreBacking
            .LiveEvents();
    }
}
