# RaceDay API Endpoint Plan

These endpoints handle users registration , login, and profile management for both Organisers and Participants.

## Authentication Endpoints

|HTTP Method|Route|Description|Role Required|Request Body|Expected Response|
|-|-|-|-|-|-|
|POST|/api/auth/register|Register a new user account|None (Public)|{ "email", "password", "firstName", "lastName", "userType" }|201 Created - user details with token|
|POST|/api/auth/login|Authenticate user and return JWT token|None (Public)|{ "email", "password" }|200 OK - { token, user }|

## User Profile Endpoints

|HTTP Method|Route|Description|Role Required|Request Body|Expected Response|
|-|-|-|-|-|-|
|GET|/api/users/profile|Get current user's profile|Any (logged in)|None|200 OK - user profile|
|PUT|/api/users/profile|Update current user's profile|Any (logged in)|{ "firstName", "lastName", "email" }|200 OK - updated user|
|GET|/api/users/{id}/enrolments|Get all enrolments for a participant|Participant or Organiser|None|200 OK - list of enrolments|

## Event Endpoints

The endpoints allow Organisers to create , update , and delete events while Participants can view upcomimg races.

|HTTP Method|Route|Description|Role Required|Request Body|Expected Response|
|-|-|-|-|-|-|
|GET|/api/events|Get all events (with optional filters)|None (Public)|None (query params: city, date, category)|200 OK - list of events|
|GET|/api/events/{id}|Get specific event details|None (Public)|None|200 OK - event details|
|POST|/api/events|Create a new event|Organiser|{ "eventName", "description", "eventDate", "startTime", "endTime", "venue", "city", "province", "maxParticipants" }|201 Created - event|
|PUT|/api/events/{id}|Update an existing event|Organiser (creator only)|Same as POST|200 OK - updated event|
|DELETE|/api/events/{id}|Delete an event|Organiser (creator only)|None|204 No Content|

## Category Endpoints

These endpoints manage race categories (e.g., 5km, 10km, Marathon ) Linked to specific events.

|HTTP Method|Route|Description|Role Required|Request Body|Expected Response|
|-|-|-|-|-|-|
|GET|/api/events/{eventId}/categories|Get all categories for an event|None (Public)|None|200 OK - list of categories|
|POST|/api/events/{eventId}/categories|Add a new category to an event|Organiser|{ "categoryName", "distance", "entryFee", "maxParticipants" }|201 Created - category|
|PUT|/api/categories/{id}|Update a category|Organiser|Same as POST|200 OK - updated category|
|DELETE|/api/categories/{id}|Delete a category|Organiser|None|204 No Content|

## Enrolment Endpoints

These endpoints handle participant registrations (enrolments) for specific event categories.

|HTTP Method|Route|Description|Role Required|Request Body|Expected Response|
|-|-|-|-|-|-|
|GET|/api/events/{eventId}/enrolments|Get all enrolments for an event|Organiser|None|200 OK - list of enrolments|
|POST|/api/events/{eventId}/enrol|Enrol a participant in an event|Participant|{ "categoryId" }|201 Created - enrolment|
|GET|/api/enrolments/{id}|Get specific enrolment details|Participant or Organiser|None|200 OK - enrolment details|
|PUT|/api/enrolments/{id}/status|Update enrolment status|Organiser or Participant|{ "status" }|200 OK - updated enrolment|
|DELETE|/api/enrolments/{id}|Cancel an enrolment|Participant (own) or Organiser|None|204 No Content|

## Results Endpoints

These endpoints allow Organisers to capture race results and Participants to view their performance history.

|HTTP Method|Route|Description|Role Required|Request Body|Expected Response|
|-|-|-|-|-|-|
|GET|/api/events/{eventId}/results|Get all results for an event|None (Public)|None|200 OK - list of results|
|GET|/api/enrolments/{id}/result|Get result for a specific enrolment|Participant or Organiser|None|200 OK - result|
|POST|/api/enrolments/{id}/result|Capture a participant's result|Organiser|{ "finishTime", "overallPosition", "categoryPosition", "pace", "status" }|201 Created - result|
|PUT|/api/results/{id}|Update a participant's result|Organiser|Same as POST|200 OK - updated result|



