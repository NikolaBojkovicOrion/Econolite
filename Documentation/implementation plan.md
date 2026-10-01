# Econolite Traffic Operations Dashboard
# Feature-Based Implementation Plan

## 1. Goal

Transform the current React/Vite starter and ASP.NET Core starter API into a small but production-shaped Traffic Operations Dashboard.

The primary workflow is:

1. An operator signs in.
2. The operator views intersections and current health.
3. A simulated detector reports congestion.
4. The API persists the traffic event.
5. The React dashboard receives the event through SignalR.
6. The operator acknowledges the event.
7. The system records an audit entry.
8. The UI handles stale data, errors, permissions, and disconnections clearly.

This workflow demonstrates senior-level full-stack skills without requiring unnecessary microservices.

## 2. Current Starting Point

### Frontend

- React 19
- Vite
- JavaScript and Sass
- Default starter `App.jsx`
- No routing, API client, authentication, tests, or real-time updates

### Backend

- ASP.NET Core targeting .NET 10
- Controllers
- Swagger/OpenAPI
- Default weather forecast endpoint
- No database, authentication, SignalR, domain model, tests, or centralized error handling

## 2.1 Backend Architecture Direction

Use Clean Architecture with domain-driven design where it adds clear value. The API layer should depend on application contracts, application code should depend on domain behavior, and infrastructure should implement persistence and external integrations.

Keep each business domain as a separate unit. Do not place all entities in one shared `Domain` folder or allow controllers to contain business rules.

Recommended solution structure:

```text
Web API/
   Econolite API.sln
   src/
      Econolite.Api/
         Controllers/
         Hubs/
         Middleware/
         Filters/
         DependencyInjection.cs
         Program.cs

      Econolite.Modules.Intersections.Domain/
         Entities/
            Intersection.cs
         ValueObjects/
            GeographicCoordinate.cs
         Events/
         Repositories/
         Exceptions/

      Econolite.Modules.Intersections.Application/
         Commands/
         Queries/
         DTOs/
         Validators/
         Interfaces/
         Services/

      Econolite.Modules.Intersections.Infrastructure/
         Persistence/
            IntersectionsDbContext.cs
            Configurations/
            Migrations/
         Repositories/
         DependencyInjection.cs

      Econolite.Modules.TrafficEvents.Domain/
         Entities/
            TrafficEvent.cs
         ValueObjects/
            DetectorEventId.cs
         Enums/
         Events/
         Repositories/
         Exceptions/

      Econolite.Modules.TrafficEvents.Application/
         Commands/
            CreateTrafficEvent/
            AcknowledgeTrafficEvent/
            ResolveTrafficEvent/
         Queries/
         DTOs/
         Validators/
         Interfaces/

      Econolite.Modules.TrafficEvents.Infrastructure/
         Persistence/
            TrafficEventsDbContext.cs
            Configurations/
            Migrations/
         Repositories/
         SignalR/
         DetectorIngestion/
         DependencyInjection.cs

      Econolite.Modules.Identity.Domain/
         Entities/
            ApplicationUser.cs
         ValueObjects/
         Roles/

      Econolite.Modules.Identity.Application/
         Commands/
            Login/
         DTOs/
         Interfaces/
         Services/

      Econolite.Modules.Identity.Infrastructure/
         Jwt/
         Persistence/
         PasswordHashing/
         DependencyInjection.cs

      Econolite.Modules.Audit.Domain/
         Entities/
            AuditEntry.cs
         ValueObjects/
         Repositories/

      Econolite.Modules.Audit.Application/
         Queries/
         DTOs/
         Interfaces/

      Econolite.Modules.Audit.Infrastructure/
         Persistence/
         Repositories/
         DependencyInjection.cs

      Econolite.SharedKernel/
         Result.cs
         Error.cs
         DomainEvent.cs
         Entity.cs
         ValueObject.cs
         Clock/
```

The modules are separate bounded contexts:

- **Intersections** owns intersection identity, location, health, and detector freshness.
- **TrafficEvents** owns congestion events, event state transitions, idempotency, and notifications.
- **Identity** owns users, password verification, JWT creation, and roles.
- **Audit** owns immutable records of important user and system actions.

Each module should expose application interfaces and DTOs rather than leaking its EF Core entities into other modules. The shared kernel must remain small; it should contain only genuinely cross-cutting primitives, not business entities.

For a small project, these can initially be projects inside one solution rather than separately deployed services. Separate code ownership and dependency boundaries first; extract runtime services only when scale or operational needs justify it.

## 3. Proposed Feature List

Implement features in this order because each later feature depends on contracts or behavior from earlier features.

## 3.1 Mandatory Testing Rule

Testing is part of every feature and must be implemented in the same feature branch or work item. No feature is complete, and the next feature must not start, until both its unit tests and integration tests pass.

| Feature | Required unit tests | Required integration tests |
| --- | --- | --- |
| Foundation and contracts | Contract mapping, version response, route composition, and shared UI rendering | API health/version requests, CORS behavior, and frontend route navigation against the application shell |
| Database persistence | Entity invariants, DTO mapping, query filters, and client loading-state transitions | SQL Server Express migrations, seed data, foreign-key behavior, and API queries over HTTP |
| Authentication and authorization | Token claim mapping, permission checks, protected route behavior, and sign-in form states | Login endpoint, JWT validation, `401`/`403` responses, protected API routes, and authenticated hub access |
| Error handling and logging | Error mapping, error boundary, retry behavior, and structured-log field creation | Exception middleware, `ProblemDetails`, correlation IDs, and representative failure responses |
| Intersection monitoring | Freshness classification, filtering, status presentation, and empty/error components | List/detail endpoints, filtering/pagination, and dashboard loading from the API |
| Traffic events and detector workflow | Event validation, lifecycle rules, idempotency, and event components | Event creation through HTTP, SQL Server uniqueness, invalid payloads, and dashboard event display |
| Acknowledge, resolve, and audit | State transitions, mutation state, confirmation behavior, and audit presentation | Transactional state-plus-audit changes, idempotent acknowledgement, and authorization over HTTP |
| SignalR real-time updates | Message deduplication, connection-state UI, reconnect behavior, and listener cleanup | Persist-before-publish behavior, authenticated hub delivery, reconnect refresh, and failure handling |
| Deployment and production readiness | Configuration parsing, health-state presentation, and startup checks | Container startup, SQL Server readiness, migration deployment, and deployed smoke workflow |

The existing feature sections below describe the concrete tests for each row. Keep the unit and integration test files next to the feature they protect; do not create a later catch-all testing phase.

### Implementation order

1. Application Foundation and Contracts
2. Database Persistence
3. Authentication and Authorization
4. Centralized Error Handling and Structured Logging
5. Intersection Monitoring Dashboard
6. Traffic Event and Simulated Detector Workflow
7. Acknowledge, Resolve, and Audit
8. Real-Time Updates with SignalR
9. Deployment and Production Readiness

### Feature 1: Application Foundation and Contracts

Create the shared conventions and development foundation before implementing business behavior.

**Backend:**

- Establish folder structure for domain, application, infrastructure, and API layers.
- Define `Intersection`, `TrafficEvent`, `User`, and `AuditEntry` contracts.
- Define API response and error conventions.
- Replace the weather endpoint with a health-check endpoint and a temporary version endpoint.
- Configure CORS for the React development server.
- Add environment-specific configuration.

**Frontend:**

- Replace the Vite screen with an application shell.
- Create shared layout, navigation, loading, empty, and error components.
- Configure API base URL through `VITE_API_BASE_URL`.
- Add a feature-oriented source structure.
- Add routing for dashboard, intersections, events, and audit views.

**Tests:**

- Add a backend smoke test for the health/version endpoints.
- Add a frontend render test for the application shell and navigation.
- Add a build and lint check to the feature validation.

**Acceptance criteria:**

- The client builds and starts successfully.
- The API starts and Swagger loads.
- The client can call the API through the configured base URL.
- The frontend and backend have consistent error and naming conventions.

### Feature 2: Database Persistence

Create the durable operational data model.

**Backend:**

- Add EF Core and a relational provider.
- Use the locally installed Microsoft SQL Server Express instance for development.
- Configure the SQL Server connection string outside source control.
- Define entities and relationships.
- Add indexes for event status, intersection ID, and event timestamps.
- Add SQL Server migrations and seed development data.
- Keep migrations owned by the module that owns the related tables.
- Implement repositories or application services only where they add value.
- Configure production SQL Server or Azure SQL separately through deployment settings.

**Frontend:**

- Add API functions for loading intersections and traffic events.
- Implement loading, empty, error, and retry states.
- Display seeded intersections and events.

**Acceptance criteria:**

- Restarting the API does not lose persisted data.
- The API can connect to the local SQL Server Express instance using a configured connection string.
- Invalid foreign keys are rejected.
- Active events can be queried by status.
- The dashboard displays data loaded from the database rather than hard-coded arrays.

**Tests:**

- Test that migrations create the expected SQL Server schema.
- Test that seeded intersections and events can be loaded after an API restart.
- Test persistence and foreign-key failures with an integration test database.
- Test the frontend loading, empty, error, and retry states for API calls.

### Feature 3: Authentication and Authorization

Protect the application and operator actions.

**Backend:**

- Add a login endpoint that validates a user and returns a signed JWT access token.
- Store password hashes, never plaintext passwords.
- Configure JWT issuer, audience, signing key, and expiration through configuration or a secret store.
- Add JWT bearer authentication middleware.
- Add users or development identities in the Identity module.
- Define `Operator`, `Supervisor`, and `Admin` roles.
- Add policies such as `CanAcknowledgeTrafficEvent` and `CanResolveCriticalEvent`.
- Protect API endpoints with authorization attributes or policies.
- Return `401` for unauthenticated requests and `403` for insufficient permissions.

**Frontend:**

- Add sign-in and sign-out screens or development login flow.
- Add an auth provider and current-user model.
- Protect routes.
- Hide actions the user cannot perform, while relying on the API for real security.
- Handle expired sessions and redirect to sign-in.

**Tests:**

- Test successful login, invalid credentials, logout, and expired-token behavior.
- Test that protected routes redirect unauthenticated users.
- Test that permission-aware actions are hidden for users without the required permission.
- Test API responses for `401 Unauthorized` and `403 Forbidden`.

**Acceptance criteria:**

- Anonymous users cannot access protected traffic data.
- A valid JWT is required for protected API endpoints and the SignalR hub.
- Operators can acknowledge allowed events.
- Only authorized users can resolve restricted events.
- Unauthorized actions are represented correctly in the UI.

**Tests:**

- Integration-test JWT signature, issuer, audience, and expiration validation.
- Integration-test role and policy authorization for each protected endpoint.
- Test that the SignalR connection cannot be established with an invalid JWT.

### Feature 4: Centralized Error Handling and Structured Logging

Make failures diagnosable and consistent.

**Backend:**

- Add global exception handling.
- Return RFC 9457 `ProblemDetails` responses.
- Add consistent validation errors.
- Add structured logs with `TraceId`, `UserId`, `IntersectionId`, and `TrafficEventId`.
- Add request duration and outcome logging.
- Avoid logging tokens, passwords, or sensitive data.

**Frontend:**

- Parse `ProblemDetails` responses.
- Display actionable errors for `400`, `401`, `403`, `404`, `409`, and `503`.
- Add an application-level error boundary.
- Keep diagnostic details out of user-facing messages where appropriate.
- Include a trace ID in support-oriented error messages when available.

**Tests:**

- Test exception, validation, not-found, conflict, unauthorized, and unavailable responses.
- Test that the frontend maps each important status code to the correct user-facing state.
- Test that an error does not replace valid previously loaded data with an empty state.
- Test that logs contain correlation fields without secrets or tokens.

**Acceptance criteria:**

- Unexpected API exceptions have a consistent response shape.
- Validation and conflict errors are distinguishable.
- A request can be followed through logs using a correlation ID.
- The UI does not show an empty dashboard when loading failed.

**Tests:**

- Integration-test the global exception middleware and `ProblemDetails` response shape.
- Verify a request trace ID appears in both the API response and structured logs.

### Feature 5: Intersection Monitoring Dashboard

Provide the first meaningful transportation workflow.

**Backend:**

- Add `GET /api/intersections`.
- Add `GET /api/intersections/{id}`.
- Return current status, last detector update, location, and active-event summary.
- Add server-side filtering and pagination where appropriate.

**Frontend:**

- Create an intersection summary view.
- Create an intersection table with status, speed, event count, and last update.
- Add filters for health and status.
- Display freshness as fresh, delayed, or stale.
- Add an intersection detail view.

**Tests:**

- Test the intersection list renders status, speed, event count, and freshness.
- Test filtering by intersection name and health status.
- Test that an intersection-not-found response renders the correct error state.
- Test the detail view with representative API data.

**Acceptance criteria:**

- An operator can find an intersection by name or status.
- The UI distinguishes unavailable data from normal traffic.
- A detail view shows the intersection’s active events and last update time.

**Tests:**

- Test the list and detail endpoints, including filtering and pagination.
- Test that detector freshness is calculated consistently at the API boundary.
- Test that the frontend distinguishes stale data from a healthy intersection.

### Feature 6: Traffic Event and Simulated Detector Workflow

Introduce congestion and incident processing.

**Backend:**

- Add `POST /api/intersections/{id}/events`.
- Validate event type, severity, timestamp, and intersection ID.
- Add a simulated detector endpoint or development-only simulator.
- Store the external detector event ID.
- Enforce idempotency with a unique source/event ID constraint.
- Add business rules for active, acknowledged, and resolved states.

**Frontend:**

- Create an active-events panel.
- Add event severity and status indicators.
- Add a development control to simulate congestion.
- Show intersection, event type, severity, detected time, and data freshness.
- Prevent the UI from presenting stale data as current.

**Tests:**

- Unit-test event validation, status transitions, and idempotency rules.
- Integration-test creation of a congestion event in SQL Server.
- Test that submitting the same detector event twice creates one event.
- Test the simulated-detector control and event severity display.
- Test invalid payloads and missing intersection IDs in the UI.

**Acceptance criteria:**

- A simulated detector creates a persistent congestion event.
- Repeating the same detector event does not create a duplicate.
- The dashboard clearly shows active congestion.
- Invalid events return useful validation errors.

**Tests:**

- An automated test verifies duplicate detector events are rejected or treated as already processed.
- An end-to-end test verifies a simulated congestion event appears in the active-events panel.

### Feature 7: Acknowledge, Resolve, and Audit

Complete the operator workflow and make changes accountable.

**Backend:**

- Add `PATCH /api/events/{id}/acknowledge`.
- Add `PATCH /api/events/{id}/resolve`.
- Add `GET /api/audit`.
- Use transactions for state changes and audit creation.
- Enforce valid state transitions.
- Store user ID, action, entity, timestamp, and trace ID.
- Make repeated acknowledgement requests idempotent.

**Frontend:**

- Add acknowledge and resolve actions.
- Add confirmation dialogs for consequential actions.
- Disable buttons while a mutation is in progress.
- Refresh or invalidate server state after success.
- Display action errors without losing the current dashboard context.
- Add an audit history view.

**Tests:**

- Unit-test valid and invalid event state transitions.
- Integration-test that state changes and audit records commit together.
- Test repeated acknowledgement requests for idempotent behavior.
- Test successful, rejected, and failed acknowledge mutations in the client.
- Test that the audit view displays the actor, action, entity, and timestamp.

**Acceptance criteria:**

- An operator can acknowledge an active event.
- Invalid state transitions are rejected.
- Every successful action has an audit entry.
- The UI does not show success before the server confirms it.

**Tests:**

- An end-to-end test acknowledges an event and verifies the audit record.
- An authorization test verifies that a restricted resolve action returns `403` and leaves the event unchanged.

### Feature 8: Real-Time Updates with SignalR

Keep operational data current without page refreshes.

**Backend:**

- Add a traffic SignalR hub.
- Persist events before publishing notifications.
- Publish event-created, event-updated, and intersection-status messages.
- Include stable event IDs in messages.
- Configure authentication for the hub.
- Define a strategy for multiple API instances, such as a backplane or managed SignalR service.

**Frontend:**

- Add a SignalR client and connection hook.
- Start the connection after authentication.
- Merge messages by event ID to prevent duplicates.
- Show connected, reconnecting, and disconnected states.
- Refresh server state after reconnecting.
- Clean up event handlers on unmount.

**Tests:**

- Test that an incoming event is added without a page refresh.
- Test duplicate SignalR messages using the same event ID.
- Test connected, reconnecting, disconnected, and reconnect-refresh states.
- Test that listeners are removed when the component unmounts.

**Acceptance criteria:**

- A simulated congestion event appears without a full-page refresh.
- Duplicate notifications do not duplicate rows.
- A disconnected client shows a visible connection state.
- Reconnection refreshes missed data.

**Tests:**

- Integration-test event publication after successful persistence.
- Test that a failed persistence operation does not publish a false success notification.
- Add Playwright coverage for the primary real-time operator workflow.

### Feature 9: Deployment and Production Readiness

Make the system deployable and observable.

**Backend:**

- Add Docker support.
- Add liveness and readiness health endpoints.
- Configure SQL Server connection settings through environment variables or deployment secrets.
- Create a safe migration deployment step.
- Add OpenTelemetry or equivalent metrics, logs, and traces.
- Define backup and restore expectations.

**Tests:**

- Test liveness and readiness endpoints.
- Test that readiness fails when SQL Server is unavailable.
- Test the container build and startup configuration.
- Run the complete backend integration suite, frontend test suite, build, and lint checks in CI.
- Add a deployment smoke test for login, event creation, SignalR notification, and acknowledgement.

**Frontend:**

- Build the Vite static bundle.
- Serve it through Nginx, a CDN, or cloud static hosting.
- Configure environment-specific API URLs.
- Add production error tracking where approved.
- Verify HTTPS, caching, and SPA route fallback behavior.

**Acceptance criteria:**

- A clean environment can run the application using documented steps.
- Readiness fails when the database is unavailable.
- Secrets are supplied by deployment configuration and are not committed.
- The primary workflow works in the deployed environment.

## 4. FE and BE Work Split

### Backend ownership

- Domain entities and business rules within separate bounded-context modules
- Application commands, queries, DTOs, and validators per module
- SQL Server schema and migrations owned by the relevant module
- REST API contracts
- JWT authentication and policy-based authorization
- Idempotent event ingestion
- Transactions and audit records
- ProblemDetails and validation
- Structured logging and correlation IDs
- SignalR hub and server notifications
- Health checks and deployment configuration
- API, integration, and domain tests

### Frontend ownership

- Application shell and routing
- Auth provider and protected routes
- API client and request cancellation
- Dashboard and intersection views
- Event list and event detail views
- Simulated detector control
- Loading, empty, error, stale, and disconnected states
- Permission-aware controls
- SignalR connection and client deduplication
- Accessibility and responsive layout
- Component and end-to-end tests
- Vite build and static deployment configuration

### Shared responsibilities

- Agree on DTOs and error response formats before implementation.
- Agree on event lifecycle and status-transition rules.
- Define timestamps, time zones, and freshness thresholds.
- Define stable IDs and idempotency behavior.
- Review API changes together.
- Use a shared acceptance checklist for each feature.
- Keep a short decision record for important architecture choices.

## 5. Recommended Parallelization

Some work can happen in parallel after the initial contract is agreed.

```text
Foundation and contracts
          |
   Database persistence
          |
   -----------------------------
   |             |              |
Dashboard    Auth setup    Error contract
   |             |              |
Events and simulator       Logging
   |             |              |
   -------- Acknowledge and audit --------
                    |
              SignalR real-time
                    |
          Deployment hardening
```

The frontend can create components with mocked DTOs while the backend implements persistence. Once the API contract stabilizes, replace mocks with the real API client.

Do not implement SignalR before the event lifecycle and persistence behavior are stable. Real-time delivery should notify about durable state, not define the state itself.

## 6. Suggested Milestones

### Milestone 1: Vertical Slice

Deliver a thin end-to-end path:

- Seed one intersection
- Load it in React
- Simulate congestion
- Persist the event
- Display the event

### Milestone 2: Secure Operator Workflow

Add:

- JWT login and bearer authentication
- Operator role
- Acknowledge action
- Transactional audit record
- `401` and `403` behavior

### Milestone 3: Real-Time Operations

Add:

- SignalR
- Reconnection state
- Event deduplication
- Stale-data indicators

### Milestone 4: Quality and Production Shape

Add:

- Structured logging and feature-level quality gates
- ProblemDetails
- Health checks
- Docker and deployment configuration

## 7. Definition of Done for Every Feature

A feature is complete when:

- The API contract is documented.
- Backend validation and authorization are implemented.
- Frontend loading, success, empty, and error states are handled.
- Persistence behavior is tested where applicable.
- Every feature has passing unit tests for its domain/application/UI behavior.
- Every feature has passing integration tests covering its API, database, browser, or infrastructure boundary.
- Tests use Microsoft SQL Server Express or the configured SQL Server environment for database integration; do not introduce SQLite or an alternate database provider.
- Logs contain useful correlation data.
- Accessibility has been considered.
- The feature works after a page refresh.
- Duplicate requests or messages have a defined behavior.
- The feature is included in the demo workflow or explicitly excluded.
- Build, lint, and relevant tests pass.

## 8. Interview Demonstration Order

Use this order in a technical interview:

1. Explain the transportation problem and domain model.
2. Show the intersection dashboard.
3. Sign in as an operator.
4. Simulate a congestion detector event.
5. Show the event persisted in the database.
6. Show the event arriving through SignalR without refreshing.
7. Acknowledge the event.
8. Show the audit entry.
9. Demonstrate an unauthorized action returning `403`.
10. Disconnect the client and show the reconnecting or stale-data state.
11. Show structured logs and the correlation ID.
12. Explain how the design would scale and how services would be extracted only when justified.

## 9. Important Tradeoffs to Explain

### Modular monolith first

A modular monolith keeps transactions and development simple while preserving clear domain boundaries. It is the right starting point until independent scaling, deployment, or ownership needs justify service extraction.

### API as source of truth

SignalR improves freshness but does not replace persistence. The client refreshes after reconnecting and never treats a notification as proof that a mutation succeeded unless the API confirms it.

### SQL Server Express locally, SQL Server in production

SQL Server Express is already installed locally and provides a realistic relational development environment for this project. It supports the same SQL Server provider, migrations, data types, constraints, and transaction behavior used by a production SQL Server or Azure SQL deployment.

Use a named connection string such as:

```json
{
   "ConnectionStrings": {
      "DefaultConnection": "Server=.\\SQLEXPRESS;Database=EconoliteTraffic;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
   }
}
```

The exact server name depends on the local installation. Do not commit production credentials. Use User Secrets for local sensitive values and environment variables or a deployment secret store for production.

### JWT authentication

JWT is appropriate for the React client and ASP.NET Core API because the API can validate a signed bearer token without keeping a server-side session. The token should contain a subject/user ID, role or permission claims, issuer, audience, and expiration.

The API should validate the issuer, audience, signature, expiration, and signing algorithm. Keep access tokens short-lived and use a secure refresh-token strategy only if the application requires long-lived sessions. Never place the JWT signing key in the React application or source control.

The SignalR connection must also authenticate with the JWT. The client should handle expiration by refreshing or ending the session rather than repeatedly retrying an invalid token.

### Domain modules instead of a shared domain bucket

Each bounded context owns its entities, invariants, application use cases, persistence mappings, and tests. For example, `TrafficEvents` owns the rule that an acknowledged event cannot be acknowledged again; `Intersections` owns detector freshness; `Identity` owns token creation; and `Audit` owns immutable audit records.

Modules may communicate through application interfaces, integration events, or explicitly defined contracts. They should not reference another module's EF Core `DbContext` or mutate another module's entities directly.

### Idempotency over exactly-once assumptions

Network retries and at-least-once message delivery are normal. Stable external event IDs and unique constraints make repeated requests safe.

## 10. Final Outcome

The finished project should demonstrate a complete, explainable vertical slice rather than a large collection of disconnected screens. A smaller workflow with persistence, security, real-time behavior, tests, observability, and deployment is stronger evidence of senior engineering judgment than an unnecessarily broad feature list.

## 11. Implemented Features

The following features have been implemented in the current workspace.

### Feature 1: Application Foundation and Contracts

**Status:** Implemented

- Added the React application shell and feature-based frontend structure.
- Added TypeScript migration for the React client.
- Added routes for overview, intersections, events, audit, and login.
- Added API health and version endpoints.
- Added configurable CORS for the client application.
- Added backend module boundaries for Intersections, Traffic Events, Identity, and Audit.
- Added initial API contracts and environment configuration.

### Feature 2: Database Persistence

**Status:** Implemented

- Added EF Core with Microsoft SQL Server support.
- Configured SQL Server Express for local development.
- Added `Intersection`, `TrafficEvent`, `ApplicationUser`, and `AuditEntry` persistence models.
- Added SQL Server configurations, indexes, foreign keys, and unique detector-event constraints.
- Added and applied the initial SQL Server migration.
- Added seeded intersections and active traffic events.
- Added `GET /api/intersections` and `GET /api/events`.
- Connected the React dashboard to persisted API data.
- Added loading, empty, error, and retry states.

### Feature 3: Authentication and Authorization

**Status:** Implemented

- Added JWT bearer authentication.
- Added `/api/auth/login`.
- Added development operator seeding.
- Added `Operator`, `Supervisor`, and `Admin` role support.
- Added authorization policies for traffic access and event operations.
- Protected intersection and traffic-event endpoints.
- Added typed React authentication context and session storage.
- Added login, logout, protected routes, and bearer-token API requests.
- Added frontend login tests and backend JWT integration tests.

### Feature 4: Centralized Error Handling and Structured Logging

**Status:** Implemented

- Added global exception handling with RFC 9457 `ProblemDetails` responses.
- Added validation responses for malformed login requests.
- Added request logging middleware with method, path, status, duration, trace ID, and correlation ID.
- Added `X-Correlation-ID` request and response support.
- Added typed frontend `ApiError` parsing for status, detail, and trace ID.
- Updated dashboard error states to show actionable API details and trace IDs.
- Added backend integration tests for validation, unauthorized responses, and correlation IDs.
- Added frontend tests for API error parsing and dashboard failure presentation.

### Validation Completed

- Frontend unit and integration tests pass.
- Backend unit and SQL Server Express integration tests pass.
- TypeScript type checking passes.
- Frontend lint passes.
- Frontend production build passes.
- Backend build passes.
- Editor diagnostics report no errors.

### Remaining Features

- **Feature 7:** Acknowledge, Resolve, and Audit
- **Feature 8:** Real-Time Updates with SignalR
- **Feature 9:** Deployment and Production Readiness
