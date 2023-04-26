public class EventsViewModel
{
    public IList<Event> Events { get; set; }
    public IList<RmEvent> RmEvents { get; set; }
    
    public IList<EventPrevPrice> EventPrevPrice { get; set; }
}