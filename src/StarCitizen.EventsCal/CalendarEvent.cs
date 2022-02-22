namespace StarCitizen.EventsCal;

public struct CalendarEvent
{
    public string EventName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public CalendarEvent()
    {
        EventName = string.Empty;
        StartTime = DateTime.MinValue;
        EndTime = DateTime.MinValue;
    }

    public override bool Equals(object? obj)
    {
        return obj is CalendarEvent @event &&
               EventName == @event.EventName &&
               StartTime == @event.StartTime &&
               EndTime == @event.EndTime;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(EventName, StartTime, EndTime);
    }
}