# Interview Speech: Traffic Operations Dashboard

*A first-person talk track for approximately 15 minutes. The section timings are guides; speak naturally and pause to show the relevant screens or code.*

## 1. Introduction and engineering approach (1 minute)

I’m a senior software developer with ten years of experience building software, and one of my strengths is turning a business problem into a solution that is understandable, maintainable, and useful to its users. I care about communication as much as implementation: I clarify requirements, explain trade-offs, work closely with other disciplines, and make sure the team understands why we are building something, not only what to code.

For this project, I built a traffic operations dashboard around a realistic operator workflow. An operator signs in, reviews intersection health and detector freshness, receives a congestion event, acknowledges or resolves it according to their permissions, and can see an audit record of the change. The purpose was to demonstrate a connected full-stack workflow rather than a set of disconnected screens.

I use GitHub Copilot to increase my throughput. I use inline suggestions for focused implementation and agent mode for broader, well-scoped tasks such as exploring code paths, implementing a feature across layers, or generating a first draft. I stay accountable for the result: I define the requirements, review the changes, check security and edge cases, run tests and builds, and revise anything that does not fit the architecture. AI helps me move faster; it does not replace engineering judgment or code ownership.

## 2. Domain and architecture (2 minutes)

The application is a React 19 and TypeScript client backed by an ASP.NET Core Web API targeting .NET 10. The API uses controllers and OpenAPI support. The client uses Vite, React Router, Sass, and a feature-oriented source structure.

The domain is intelligent transportation. An intersection has an operational status and detector readings; a traffic event records something such as congestion, its severity and lifecycle status; an audit entry records an important action and its actor. One important distinction is that detector freshness and intersection health are separate. Old detector data should not be represented as though it were current, but staleness alone should not silently redefine the intersection's health.

I organized the backend into business modules for Intersections, Traffic Events, Identity, and Audit. Controllers handle HTTP concerns and delegate to application services. The services apply use-case logic and map results to response contracts; EF Core entities stay behind those response contracts instead of being returned directly from endpoints. The frontend follows a similar feature boundary: authentication, dashboard, intersections, traffic events, and audit each own their relevant pages, API functions, hooks, and components, while shared API and UI utilities live under `shared`.

I describe this as a modular monolith with Clean Architecture influences, not as a fully separated, textbook Clean Architecture solution. The modules are separated in the codebase, but they currently share one API assembly and an EF Core DbContext. That is an intentional level of structure for this project: it gives the business areas names and boundaries without introducing multiple deployables or distributed transactions before there is a need. I would strengthen dependency isolation as the system grows, but I would not split services just to follow a trend.

## 3. .NET and API implementation (4 minutes)

The API is composed in `Program.cs` using ASP.NET Core's dependency injection and middleware pipeline. Controllers are registered with the application; application services such as `IIntersectionService` and `ITrafficEventService` are registered with scoped lifetimes because they work with the request-scoped EF Core context. The SignalR publisher and password hasher have different lifetimes appropriate to their dependencies. I configure SQL Server, authentication, authorization, CORS, ProblemDetails, exception handling, health checks, and the hub as part of the host setup.

For persistence, the project uses EF Core with the SQL Server provider. The DbContext exposes intersections, traffic events, users, and audit entries, and applies entity configurations from the assembly. The intersection query is a good example of practical EF Core: it starts with an `AsNoTracking` query for read-only data, applies validated filters before pagination, orders by name and then stable ID, and projects only the fields needed by the response. That avoids returning tracked entities or loading more columns than the view needs. Page size is bounded, and the endpoint returns page metadata and summary counts. Detector freshness thresholds are configured through options and validated at startup rather than silently accepting an invalid configuration.

The traffic event service enforces the event workflow. It validates supported event types and severities, checks that the intersection exists, and checks the external detector identity. The source system and external event ID provide an idempotency key; the database also has a uniqueness constraint, which protects against two concurrent requests passing an application-level duplicate check at the same time. This is important because network retries happen, and exactly-once delivery is not a realistic assumption.

For operator actions, the domain entity controls valid state transitions: an active event can be acknowledged, an active or acknowledged event can be resolved, and invalid transitions are rejected. The service saves the event change and its audit entry within a database transaction. Only after commit does it publish the SignalR update. That ordering keeps the database as the source of truth: the client should never receive a success notification for a mutation that did not persist.

The endpoints are asynchronous and pass cancellation tokens down into EF Core operations, so work can stop when the request is cancelled. Login uses ASP.NET Core's password hasher and returns a JWT. JWT bearer validation checks signature, issuer, audience, and lifetime. Authorization policies protect traffic access and operator actions; resolving critical events requires a supervisor or administrator. The API remains the security boundary. Hiding a button in React is useful for the experience, but it cannot authorize the request.

The API also uses centralized exception handling that returns RFC 9457 ProblemDetails, including a trace ID. Request logging middleware adds a correlation ID to the response and structured logging scope, then records the method, path, status code, and elapsed time. This makes a failed request easier to investigate without exposing exception details or credentials in the client response. Swagger/OpenAPI is available in development, along with health and version endpoints.

## 4. React and TypeScript client (3 minutes)

The client is written in TypeScript, which helps make API contracts and UI state explicit. Shared traffic types describe the data crossing the client boundary, and generic API functions such as `getJson<T>` keep HTTP response handling consistent without losing useful type information. The API client adds the bearer token, parses ProblemDetails into a typed `ApiError`, and uses an `AbortSignal` for cancellable GET requests.

The app is composed from an application shell, browser routing, and an authentication provider. Protected routes keep the login flow separate from the operational views. Authentication state is exposed through React context; page-level behavior is implemented with hooks rather than one oversized component. The dashboard uses a reducer to model loading, successful data, incoming events, and failure as explicit state transitions. This makes the event update behavior easier to reason about and test.

I keep the state close to its owner. Component state is suitable for transient UI details; shared authentication belongs in the auth provider; and API-owned data is loaded through feature API functions and hooks. I would consider a server-state library such as TanStack Query if caching, invalidation, and cross-page synchronization became more complex. I would not add a global store or a dependency without a concrete need.

The dashboard loads intersections and active events concurrently with `Promise.all`, and cancels the initial request when the owning component unmounts. The UI differentiates loading, empty, and failed requests. That distinction matters in an operations product: a failed request must not look like a successful empty result. The intersections screens support filtering and pagination, and show health separately from detector freshness. Missing speed is unavailable, not zero.

For real-time updates, the client connects to the authenticated SignalR hub and exposes connecting, connected, reconnecting, and disconnected states. It registers handlers for event creation, event updates, intersection status, and audit entries. Incoming events are merged by stable ID rather than blindly appended, listeners are removed and the connection is stopped on cleanup, and the client refreshes authoritative data after reconnecting because messages may have been missed. SignalR improves freshness; it does not replace HTTP or persisted state.

The client uses semantic React components and Sass styling, and its tests use Vitest and React Testing Library. I focus tests on behavior users depend on: data rendering, authentication states, error presentation, and the update workflows, not incidental implementation details.

## 5. Quality, security, and trade-offs (3 minutes)

The project includes backend API tests for authentication, intersections, and ProblemDetails, as well as frontend tests for login, dashboard data behavior, and API errors. I use integration tests where the contract crosses an HTTP boundary, and smaller focused tests for state and component behavior. I also use TypeScript type checking, linting, production builds, and backend compilation as complementary quality gates. Tests do not prove the absence of bugs, but they make important behavior repeatable and give us confidence when changing it.

There are a few deliberate reliability choices in the implementation. Database uniqueness backs up duplicate detection. Audit and state changes are committed together. Real-time notifications follow persistence. The UI has explicit connection and request failure states. These choices fit the domain because an operator needs to know whether information is current and whether an action actually succeeded.

I would be clear about the current security and deployment scope in a production discussion. The detector submission endpoint is development-only and exists to simulate a roadway device; it is not a production ingestion design. The project is configured for local SQL Server Express development. Production deployment hardening, readiness checks that verify database connectivity, external identity integration, a SignalR backplane or managed service for multiple instances, and a production ingestion pipeline are follow-up work, not claims I would make about this implementation. For deployment, I would manage secrets outside the client bundle and source control, run database changes through a controlled migration step, and add observability and recovery procedures appropriate to the service's reliability requirements.

I also made the choice to start as a modular monolith. If traffic volume or team ownership later required independent scaling, I would first measure the bottleneck, then consider extracting high-volume ingestion or historical analytics behind stable contracts. A message broker, partitioning, bounded retries, backpressure, and dead-letter handling would be reasonable when measured load justified them. Prematurely adding those systems would increase operational complexity without improving the current workflow.

## 6. How I work with AI tools (1 minute)

I treat Copilot as part of my development workflow, not as the architect or reviewer. I give it a clear bounded task and enough local context, then inspect how the change fits existing types, conventions, security rules, and tests. Agent mode is especially useful when a feature crosses the client and API, because it can accelerate exploration and repetitive implementation. Inline suggestions are useful for focused coding. I still decide what belongs in a module, which behavior must be covered, and whether a generated solution is too broad or too clever.

My goal is not just to produce more code per hour. It is to use the speed-up to spend more time on problem framing, communication, edge cases, review, and validation. That is where experience matters most: knowing what to delegate to a tool, what to verify, and what not to build yet.

## 7. Closing (1 minute)

To summarize, this project demonstrates how I approach a full-stack operational workflow: model the domain, keep business behavior out of controllers and presentation components, persist important state with EF Core, enforce security at the API, expose consistent errors, record auditable actions, and use SignalR as a delivery mechanism on top of durable state. On the React side, I use typed feature boundaries, explicit UI states, and lifecycle-aware real-time integration.

Across my ten years as a developer, I have learned that strong engineering is a combination of technical depth, sound trade-offs, clear communication, and ownership. I use modern tools, including GitHub Copilot agent mode, to work faster, while remaining responsible for the quality and behavior of what we ship. I would bring that same approach to software that supports real transportation operations, where reliability and clarity directly affect the people using the system.

## Quick accuracy notes before the interview

- The event simulator is restricted to Development; describe it as a test/demo workflow, not a production detector integration.
- The current backend is a modular monolith with module-oriented folders, not a set of independently deployed services.
- SQL Server Express is the local development provider. Production deployment and migration automation are future hardening work.
- SignalR is implemented with reconnect handling, but scaling across multiple API instances still needs a backplane or managed SignalR service.
- Be precise about which tests and checks you personally ran most recently; the project documentation records that the later event/audit and SignalR features were compile-checked but did not have their full test suites run at that time.
- When discussing your ten years of experience, connect it to real examples from your own work. This project is a demonstration of your approach, not a substitute for those examples.