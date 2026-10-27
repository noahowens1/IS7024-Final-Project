# IS7024-Final-Project
# Cincy Events

## Introduction

Cincy Events is a personalized event discovery and planning web application designed to help users explore activities and experiences throughout Cincinnati. The application combines information from multiple data sources to recommend local events based on users' interests, budgets, preferred dates, and locations.

Users can receive personalized event recommendations based on their saved preferences or manually search and filter events to discover new experiences.

### Key Features

- **Personalized Recommendations:** Discover events based on saved interests and preferences.
- **Event Search:** Browse local events by category, including music, sports, food, arts, and entertainment.
- **Budget Filtering:** Find activities within a selected spending limit.
- **Date and Location Filtering:** Search for events based on availability and preferred locations.
- **Event Details:** View event dates, venues, descriptions, available pricing, and ticket links.
- **Saved Events:** Bookmark events for future planning.
- **Weather Information:** View available weather forecasts when planning outdoor activities.

## Application Logo

![Cincy Events Logo](CincyEventsLogo.jpg)

## Storyboard

![Cincy Events Storyboard](Cincy_Events_Storyboard.png)

## Requirements
#### Requirement 1: Search for Events
###### Scenario
As a user who is interested in sports, I want to be able to search sporting events on any name related to the topic: sports, team names, sport names, etc.
###### Dependencies
Recent or upcoming sport events search data should be available and accessible.
###### Assumptions
Search results should be events that has not happened yet.
###### Examples
1.1
**Given** an upcoming hockey game is available
**When** I search for "Cincinnati Cyclones"
**Then** I should receive at least one result with these attributes:

Event: Cincinnati Cyclones
Date: Nov 25
Time: 7:05 PM
Location: Heritage Bank Center
Ticket: $38+

1.2 
**Given** an upcoming football game is available
**When** I search for "Cincinnati Bearcats"
**Then** I should receive at least one result with these attributes:

Event: Cincinnati Bearcats Football
Date: Nov 21
Time: 12:00 PM
Location: Nippert Stadium
Ticket: $47+

1.3
**Given** an upcoming baseball game is available
**When** I search for "Reds"
**Then** I should receive at least one result with these attributes:

Event: Cincinnati Reds
Date: Apr 13
Time: TBD
Location: Great American Ball Park
Ticket: $181+

#### Requirement 2: Filter Events by Budget
###### Scenario
As a user who is finding events to attend based on budget, I want to be able to search for different events with the filter function including options like <$50, $50-100, or >$100, etc.
###### Dependencies
Recent or upcoming events search data should be available and accessible with the respective filtered price.
###### Assumptions
Search results should be events that has not happened yet.
CUrrency should be in USD.
###### Examples
1.1
**Given** a festival that does not require ticket is available
**When** I filter events with pricing "<$50"
**Then** I should receive at least one result with these attributes:

Event: The 2026 Cincinnati SantaCon
Date: Dec 12 
Time: 12:00 PM
Location: Downtown Cincinnati, OTR, Mt.Adams, Covington, Newport, and Factory 52
Ticket: Free

1.2 
**Given** an upcoming event with the ticket price greater than $100 is available
**When** I filter events with pricing ">$100"
**Then** I should receive at least one result with these attributes:

Event: Haunted Fest
Date: Oct 31
Time: 7:00 PM
Location: MegaCorp Pavillon
Ticket: $110+

3. Requirement 3
4. Requirement 4
## Data sources

## Database

## Team Composition
- **Team Member:** Taylor Johnson, Noah Owens, Hayden Phan, Angel Paz
- **Meeting Time:** Mondays 2PM-4PM (In person/Teams)
