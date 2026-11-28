
namespace CincyEvents.Models
{
    public class ExternalEvent
    {
        public string ExternalId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset StartsAt { get; set; }
        public DateTimeOffset EndsAt { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public decimal? MaxPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string EventUrl { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}
