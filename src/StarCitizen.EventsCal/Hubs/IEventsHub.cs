namespace StarCitizen.EventsCal.Hubs
{
    public interface IEventsHub
    {
        Task PushAll(IEnumerable<CalendarEvent> calendarEvents);
    }
}