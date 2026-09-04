# RaceDay API Endpoint Plan

**System:** RaceDay – South African Road Running, Walking and Cycling Event Management System  
**Part:** 1 – System Planning and Database  
**API base path:** `/api`

## Role definitions

- **Public:** No account required.
- **Participant:** Authenticated user whose role is `Participant`.
- **Organiser:** Authenticated user whose role is `Organiser`.

## Endpoint specification

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Creates a new Participant account using the supplied personal and login details. | Public | `{ firstName, lastName, email, password, phone }` | **201 Created** – account created. **400 Bad Request** – validation failure. **409 Conflict** – email already exists. |
| POST | `/api/auth/login` | Authenticates a registered user and returns an access token containing the user identity and role. | Public | `{ email, password }` | **200 OK** – login successful with token and user summary. **401 Unauthorized** – invalid credentials. |
| GET | `/api/profile/me` | Returns the authenticated user's profile details. | Participant / Organiser | None | **200 OK** – profile JSON. **401 Unauthorized** – not logged in. |
| PUT | `/api/profile/me` | Updates the authenticated user's editable profile information. | Participant / Organiser | `{ firstName, lastName, phone }` | **200 OK** – updated profile. **400 Bad Request** – invalid data. **401 Unauthorized** – not logged in. |
| GET | `/api/events` | Lists upcoming and active events, with optional filtering by event type, province or date. | Public | None | **200 OK** – array of event summaries. |
| GET | `/api/events/{id}` | Returns full details for a specific event, including organiser, route and available categories. | Public | None | **200 OK** – event details. **404 Not Found** – event does not exist. |
| POST | `/api/events` | Creates a new road running, walking or cycling event owned by the logged-in organiser. | Organiser | `{ name, description, eventDate, registrationOpen, registrationClose, venue, city, province, distanceKm, eventType, status }` | **201 Created** – event created. **400 Bad Request** – validation failure. **401 Unauthorized** – not logged in. **403 Forbidden** – participant attempted the operation. |
| PUT | `/api/events/{id}` | Updates an event owned by the logged-in organiser. | Organiser | `{ name, description, eventDate, registrationOpen, registrationClose, venue, city, province, distanceKm, eventType, status }` | **200 OK** – updated event. **404 Not Found** – event does not exist. **403 Forbidden** – organiser does not own the event. |
| DELETE | `/api/events/{id}` | Deletes an event owned by the logged-in organiser. | Organiser | None | **204 No Content** – event removed. **404 Not Found** – event does not exist. **409 Conflict** – event has dependent enrolments/results and cannot be deleted under business rules. |
| GET | `/api/events/{id}/categories` | Lists all categories available for a specific event. | Public | None | **200 OK** – category list. **404 Not Found** – event does not exist. |
| POST | `/api/events/{id}/categories` | Adds an entry category to an event. | Organiser | `{ name, distanceKm, entryFee, maxEntries, minAge, maxAge }` | **201 Created** – category created. **400 Bad Request** – invalid category. **403 Forbidden** – organiser does not own event. **404 Not Found** – event does not exist. |
| PUT | `/api/events/{id}/categories/{categoryId}` | Updates one category belonging to an organiser's event. | Organiser | `{ name, distanceKm, entryFee, maxEntries, minAge, maxAge }` | **200 OK** – updated category. **403 Forbidden** – access denied. **404 Not Found** – event/category does not exist. |
| DELETE | `/api/events/{id}/categories/{categoryId}` | Removes a category from an event when business rules allow it. | Organiser | None | **204 No Content** – category removed. **403 Forbidden** – access denied. **404 Not Found** – event/category not found. **409 Conflict** – category has enrolments. |
| GET | `/api/enrolments/me` | Returns all event enrolments belonging to the authenticated participant. | Participant | None | **200 OK** – participant's enrolment history. **401 Unauthorized** – not logged in. **403 Forbidden** – organiser attempted participant-only operation. |
| POST | `/api/events/{id}/enrolments` | Enrols the logged-in participant in one category for a specific event. | Participant | `{ categoryId }` | **201 Created** – enrolment created. **404 Not Found** – event/category does not exist. **409 Conflict** – participant is already enrolled or category is full. **401 Unauthorized** – not logged in. |
| DELETE | `/api/enrolments/{id}` | Cancels an enrolment belonging to the authenticated participant. | Participant | None | **204 No Content** – enrolment cancelled. **403 Forbidden** – enrolment belongs to another participant. **404 Not Found** – enrolment not found. |
| GET | `/api/events/{id}/enrolments` | Allows an organiser to view all participants enrolled in one of their events. | Organiser | None | **200 OK** – enrolment list. **403 Forbidden** – organiser does not own event. **404 Not Found** – event not found. |
| GET | `/api/results/me` | Returns the authenticated participant's personal race results and performance history. | Participant | None | **200 OK** – result history. **401 Unauthorized** – not logged in. |
| GET | `/api/events/{id}/results` | Returns results captured for all participants in an organiser's event. | Organiser | None | **200 OK** – result list. **403 Forbidden** – organiser does not own event. **404 Not Found** – event not found. |
| POST | `/api/events/{id}/results` | Captures a participant's result for an event enrolment. | Organiser | `{ enrolmentId, finishTime, overallPosition, categoryPosition, averagePace, finishStatus }` | **201 Created** – result recorded. **400 Bad Request** – invalid result data. **404 Not Found** – enrolment/event not found. **409 Conflict** – result already exists. |
| PUT | `/api/events/{id}/results/{resultId}` | Corrects an existing participant result recorded by the organiser. | Organiser | `{ finishTime, overallPosition, categoryPosition, averagePace, finishStatus }` | **200 OK** – result updated. **403 Forbidden** – access denied. **404 Not Found** – result not found. |
| GET | `/api/events/{id}/route` | Returns route and course information stored for an event. | Public | None | **200 OK** – route information. **404 Not Found** – route/event not found. |
| GET | `/api/events/{id}/weather` | Returns current/fresh weather data for the event location through the server-side weather integration. | Public | None | **200 OK** – weather data. **404 Not Found** – event not found. **502 Bad Gateway** – external weather service unavailable. |

## Design notes

1. Authentication endpoints issue a token that the Part 2 API uses for role-based authorisation.
2. Organiser operations are restricted to events owned by the authenticated organiser, not merely to any user with the organiser role.
3. Participants can only view or cancel their own enrolments and personal results.
4. Public event browsing does not require authentication so visitors can discover upcoming events before registering.
5. Route information is stored in the database, while live weather is treated as an external-service integration rather than static event data.
6. Part 2 should keep route names and request/response contracts as close to this plan as possible. Any deviations should be documented in the README.
