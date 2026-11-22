using CincyEvents.Models;

namespace CincyEvents.Services
{
    public static class CategoryCatalog
    {
        public static IReadOnlyList<EventCategory> All { get; } =
            Array.AsReadOnly(
            [
                new EventCategory(
                    "music-concerts",
                    "Music & Concerts",
                    "Live music, concerts, performances, and other musical events in and around Cincinnati."),
                new EventCategory(
                    "sports",
                    "Sports",
                    "Sporting events, games, competitions, and activities for local sports fans."),
                new EventCategory(
                    "food-dining",
                    "Food & Dining",
                    "Food festivals, tastings, dining experiences, and culinary events."),
                new EventCategory(
                    "arts-culture",
                    "Arts & Culture",
                    "Museums, galleries, theater, exhibits, and other cultural experiences."),
                new EventCategory(
                    "festivals-community",
                    "Festivals & Community",
                    "Festivals, markets, neighborhood events, and community gatherings.")
            ]);

        public static EventCategory? GetById(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return All.FirstOrDefault(
                category => string.Equals(category.Id, id, StringComparison.OrdinalIgnoreCase));
        }
    }
}