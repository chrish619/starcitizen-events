
namespace StarCitizen.EventsCal.Services;

public class EventCalendarStoreBacking
{
    private SortedSet<CalendarEvent> _events;

    public EventCalendarStoreBacking(ILogger<EventCalendarStoreBacking> logger)
    {
        _events = new SortedSet<CalendarEvent>(Comparer<CalendarEvent>.Create((a, b) => a.StartTime.CompareTo(b.StartTime)));
    }

    public ChangeSet CreateChangeSet()
    {
        return new ChangeSet(this);
    }

    public IEnumerable<CalendarEvent> AllEvents()
    {
        return _events
            .AsEnumerable();
    }

    public IEnumerable<CalendarEvent> LiveEvents()
    {
        var now = DateTime.UtcNow;

        return _events
            .AsEnumerable()
            .Where(e => e.StartTime < now && e.EndTime > now);
    }

    internal IEnumerable<CalendarEvent> CurrentAndUpcomingEvents()
    {
        var now = DateTime.UtcNow;

        return _events
            .AsEnumerable()
            .Where(e => e.EndTime > now);
    }

    public class ChangeSet : IDisposable
    {
        private HashSet<CalendarEvent> _mergeSet;
        private bool _disposedValue;
        private readonly EventCalendarStoreBacking _backingStore;

        public ChangeSet(EventCalendarStoreBacking backingStore)
        {
            _backingStore = backingStore;
            _mergeSet = new HashSet<CalendarEvent>();
        }

        public void Add(CalendarEvent calendarEvent)
        {
            _mergeSet.Add(calendarEvent);
        }

        /// <summary>
        /// Commits changes to the underlying backing store; Adds items not in storage, removes items no longer in the store;
        /// </summary>
        /// <returns><see langword="true"/> if there were changes made to the underlying store</returns>
        public bool Commit()
        {
            bool hasChanges = false;

            foreach (var evt in _mergeSet)
            {
                if (_backingStore._events.Add(evt))
                {
                    hasChanges = true;
                }
            }

            foreach (var evt in _backingStore._events.ToArray())
            {
                if (!_mergeSet.Contains(evt))
                {
                    _backingStore._events.Remove(evt);
                    hasChanges = true;
                }
            }

            return hasChanges;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ChangeSet()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}