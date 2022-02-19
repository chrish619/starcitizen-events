public struct CalendarEvent
{
    public string EventName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

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