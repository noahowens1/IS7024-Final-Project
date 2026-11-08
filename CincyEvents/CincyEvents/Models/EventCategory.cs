namespace CincyEvents.Models
{
    public sealed class EventCategory
    {
        public EventCategory(string id, string title, string description)
        {
            Id = id;
            Title = title;
            Description = description;
        }

        public string Id { get; }

        public string Title { get; }

        public string Description { get; }
    }
}