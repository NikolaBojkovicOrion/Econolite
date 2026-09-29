# Econolite Interview Task

## Goal

Build a small **Traffic Operations Dashboard** that demonstrates senior-level .NET and React development.

The workflow is:

> An authenticated traffic operator views intersections, receives live status updates, acknowledges a congestion incident, and audits who changed it.

This single workflow demonstrates database persistence, authentication, testing, structured logging, error handling, real-time updates, deployment, and a meaningful transportation use case.

## Current Starter Application

The workspace currently contains:

- A React/Vite client using React 19
- An ASP.NET Core API targeting .NET 10
- Swagger/OpenAPI support
- The default weather forecast controller

The starter app does not yet demonstrate persistence, authentication, SignalR, tests, or production observability.

## Domain Model

```text
User
  Id, Email, Role

Intersection
  Id, Name, Latitude, Longitude, Status

SignalPhase
  Id, IntersectionId, PhaseNumber, Movement, DurationSeconds

TrafficEvent
  Id, IntersectionId, Type, Severity, Message, Status, CreatedAt, ResolvedAt

AuditEntry
  Id, UserId, Action, EntityType, EntityId, CreatedAt
```

## Meaning of a Simulated Detector

A detector is a real roadway device that observes traffic. Examples include radar, cameras, inductive loops, and other vehicle-detection sensors.

A **simulated detector** is application code that pretends to be one of those devices. Instead of receiving data from physical equipment, the developer creates an API request, button, timer, or test message that reports traffic conditions.

Example:

```http
POST /api/intersections/101/events
```

```json
{
  "type": "Congestion",
  "severity": "High",
  "averageSpeed": 8,
  "detectedAt": "2026-09-29T10:15:00Z"
}
```

## What Congestion Means

Traffic congestion occurs when traffic demand exceeds the available capacity of a road or intersection. It can be identified by:

- Vehicles moving below the expected speed
- Long queues at an intersection
- High vehicle occupancy or density
- Delays exceeding a defined threshold
- A detector reporting abnormal traffic volume

The API should persist the congestion event and publish a real-time notification to the React dashboard through SignalR.

## Suggested API

```text
POST   /api/auth/login
GET    /api/intersections
GET    /api/intersections/{id}
GET    /api/events?status=Active
POST   /api/intersections/{id}/events
PATCH  /api/events/{id}/acknowledge
PATCH  /api/events/{id}/resolve
GET    /api/audit?entityId={id}
```

Use DTOs rather than exposing EF Core entities directly. Validate requests at the API boundary and keep business rules in application services.

## Recommended Implementation Order

1. Replace the weather endpoint with `Intersection` and `TrafficEvent` entities.
2. Add EF Core with SQLite locally and migrations.
3. Add service-layer business rules and DTOs.
4. Add global `ProblemDetails` error handling.
5. Add structured logs and correlation IDs.
6. Add authentication and role-based authorization.
7. Add SignalR event broadcasting.
8. Build the React dashboard.
9. Add unit and integration tests.
10. Add Docker, environment configuration, and CI.

Start with a modular monolith. Extract services only when independent scaling, ownership, deployment, or storage requirements justify the added complexity.

## Interview Questions and Answers

### Why did you choose a relational database?

Traffic events, intersections, users, permissions, and audit records have clear relationships and consistency requirements. A relational database provides foreign keys, transactions, unique constraints, indexes, and reliable queries.

I would use SQLite for local development and PostgreSQL in production. A time-series or event-streaming database could be added later for high-volume telemetry, but it would not replace the primary operational database initially.

### Which operations require a transaction?

Important transactional operations include:

- Creating a traffic event and its audit record
- Acknowledging an event and recording who acknowledged it
- Resolving an event and recording the resolution
- Updating an intersection state and its corresponding event
- Processing an incoming detector event and marking it as processed

The main principle is that related database changes must either all succeed or all fail. Transactions should remain short and should not stay open while calling external services.

### How do you prevent duplicate event processing?

Every incoming detector event should contain a unique event ID or idempotency key. The database should enforce uniqueness, for example:

```text
UNIQUE(SourceSystem, ExternalEventId)
```

If the same message arrives again, the handler treats it as already processed rather than creating another event.

At larger scale, a message broker can provide acknowledgements, retries, and dead-letter queues. The consumer must still be idempotent because most brokers provide at-least-once delivery.

### How would the system behave if SignalR is disconnected?

SignalR is the live-update mechanism, not the source of truth. The React client should:

- Show a disconnected or reconnecting state
- Retry the connection automatically
- Refresh current data after reconnecting
- Continue safe operations through normal HTTP APIs
- Avoid claiming that data is live while disconnected

The server persists the event before publishing the SignalR notification. A client that misses a message can recover by querying the API after reconnecting.

For multiple API instances, use a SignalR backplane or managed SignalR service.

### How do you secure an operator-only action?

Authenticate users using an OIDC provider or JWT, then enforce policy-based authorization in the API:

```csharp
[Authorize(Policy = "CanAcknowledgeTrafficEvent")]
```

The policy can require the `Operator` or `Supervisor` role. Hiding a button in React is not security; the API must enforce the rule.

Also validate event state, check object-level permissions, record the authenticated user in the audit log, and protect against CSRF when using cookie authentication.

### How do you correlate one request across API logs and database operations?

Use the ASP.NET Core request trace ID or W3C trace context and include it in structured logs.

Useful fields include:

```text
TraceId
RequestId
UserId
IntersectionId
TrafficEventId
Operation
DurationMs
Outcome
```

Propagate the trace ID to downstream services and message headers. OpenTelemetry can export traces, metrics, and logs so one request can be followed across the browser, API, database, broker, and external integrations.

### How do you handle stale detector data?

Store the last-seen timestamp for every detector or intersection and define a freshness policy. For example:

- Fresh: data within 30 seconds
- Delayed: data between 30 and 120 seconds old
- Stale: no data for more than 120 seconds

The UI must distinguish “no congestion detected” from “no current data available.” A background process can create a communication-health event when data becomes stale.

Missing data should not silently be treated as normal traffic.

### How would you scale ingestion if thousands of intersections reported simultaneously?

Separate ingestion traffic from interactive dashboard traffic:

```text
Intersection devices
        |
Ingestion API
        |
Message broker
        |
Partitioned consumers
        |
Operational database / time-series storage
        |
Dashboard API and SignalR
```

Partition messages by intersection ID so events from one intersection remain ordered while different intersections process in parallel.

Use bounded queues, backpressure, retry policies, dead-letter queues, batch writes where safe, database indexes, connection-pool limits, and horizontal scaling for stateless API instances.

A modular monolith with a broker may be enough initially. Measure first.

### How do you deploy database migrations safely?

Do not automatically run destructive migrations during application startup in production.

Use an expand-and-contract approach:

1. Add new nullable columns or tables.
2. Deploy code that supports both old and new structures.
3. Backfill data in controlled batches.
4. Switch reads and writes to the new structure.
5. Remove old structures only after verification.

Run migrations as a versioned deployment step using a controlled identity. Test against a production-like database backup and maintain a restore plan.

### What happens when the database is unavailable?

The API should fail clearly and safely:

- Health readiness checks report failure
- Persistence-dependent requests return `503 Service Unavailable`
- The API does not claim an event was saved
- Retryable operations use bounded retries
- Logs include trace and database failure details
- Cached data may be shown as stale, never as current

For ingestion, messages should remain in the broker until processing succeeds. Critical operations should fail closed rather than acknowledge an event that was not persisted.

### What would you monitor in production?

**Application health:**

- Request rate
- Error rate
- Latency percentiles
- HTTP 4xx and 5xx responses
- Authentication and authorization failures
- SignalR connection and reconnect rates

**Database health:**

- Connection-pool usage
- Query duration
- Slow queries
- Lock contention
- CPU, memory, and storage
- Migration and backup status

**Ingestion health:**

- Events per second
- Processing lag
- Queue depth
- Duplicate-event rate
- Retry count
- Dead-letter messages
- Stale intersections

**Business health:**

- Active traffic incidents
- Time to acknowledge
- Time to resolve
- Detector availability
- Controller communication failures

Alerts should be tied to service-level objectives instead of every individual exception.

### Which parts would you extract into services only when scale required it?

Initially keep a modular monolith with boundaries for:

- Identity and authorization
- Intersection management
- Traffic-event management
- Ingestion
- Notifications
- Audit

Possible future services include:

- High-volume telemetry ingestion
- External hardware and agency integrations
- Prediction or machine-learning workloads
- Notification delivery
- Historical analytics

Extract a service only when it needs independent scaling, deployment, ownership, reliability isolation, or a different storage model. Premature microservices add network failures, distributed tracing, deployment complexity, and consistency problems.

## Testing Strategy

At minimum, demonstrate these tests:

- An operator can acknowledge an active event.
- An operator cannot resolve a critical event without permission.
- A resolved event cannot be acknowledged again.
- An unauthenticated request returns `401 Unauthorized`.
- An authenticated user without permission receives `403 Forbidden`.
- Creating an event persists it in the database.
- Creating an event broadcasts a SignalR notification.
- Invalid intersection IDs return `404 ProblemDetails`.
- Malformed requests return `400` validation details.

Use unit tests for business rules and integration tests for API, authentication, database, and persistence behavior.

## Deployment Demonstration

A production-shaped deployment can be:

```text
React/Vite static build
        |
      Nginx
        |
ASP.NET Core API
        |
PostgreSQL
```

Use environment variables for:

```text
ConnectionStrings__Default
Jwt__Issuer
Jwt__Audience
Jwt__SigningKey
ASPNETCORE_ENVIRONMENT
```

Never commit signing keys or production connection strings. Add `/health/live` and `/health/ready`; readiness should verify database connectivity.

## Five-Minute Interview Walkthrough

1. Explain the traffic-management problem.
2. Show the dashboard.
3. Sign in as an operator.
4. Simulate congestion.
5. Show the live UI update.
6. Acknowledge the event.
7. Show the audit record.
8. Trigger an authorization failure.
9. Show the structured log and `ProblemDetails` response.
10. Explain deployment and scaling decisions.

## Senior-Level Closing Answer

> I would start with a modular ASP.NET Core application backed by PostgreSQL, with explicit domain boundaries and reliable transactional behavior. Real-time updates would be delivered through SignalR, but persistence would remain the source of truth. I would make ingestion idempotent, expose stale data clearly, use structured observability, and scale with a message broker when traffic volume justified it. I would extract services based on independent scaling or ownership needs, not simply because microservices are fashionable.
