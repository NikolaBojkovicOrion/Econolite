# .NET Interview Talk: ASP.NET Core and Backend Engineering

*First-person interview script for approximately 10 minutes at a steady interview pace. The follow-up answers are optional and are not part of the timed talk. Use the headings as cues rather than reading them as a checklist.*

## 1. .NET, C#, and the application (1 minute)

When I say .NET, I mean the development platform and runtime; C# is the language I use on top of it. ASP.NET Core is the web framework in that platform. In this project, ASP.NET Core hosts a .NET 10 Web API, and C# implements the HTTP endpoints, application services, domain behavior, and infrastructure integrations. Entity Framework Core provides object-relational mapping to SQL Server, and SignalR provides real-time communication.

I like to explain the stack by responsibility. The runtime executes the application; ASP.NET Core handles HTTP, middleware, routing, and dependency injection; EF Core handles persistence; and our application code owns the traffic-management behavior. Keeping those roles clear helps avoid placing business rules in controllers or letting database details leak into the client contract.

The project uses modern C# features including nullable reference types, records for immutable request and response contracts, primary constructors for concise dependency injection, and top-level statements in the host setup. These are useful language features, but I use them to make intent clear rather than to make the code look clever.

## 2. ASP.NET Core hosting, DI, and middleware (1.5 minutes)

The API starts with the modern minimal-hosting model in `Program.cs`. I use `WebApplication.CreateBuilder` to configure services, build the host, then configure the HTTP request pipeline and map endpoints. Minimal hosting does not mean the application must use only minimal APIs; this project uses controllers for its business endpoints and maps a few infrastructure endpoints directly.

Dependency injection is central to ASP.NET Core. Controllers depend on interfaces such as `IIntersectionService`, rather than constructing concrete services themselves. The container creates and supplies the implementations. The intersection and traffic-event application services are scoped because they use the request-scoped EF Core `DbContext`. The traffic-event publisher is registered as a singleton and uses SignalR's hub context, while the password hasher is a stateless service. Choosing a lifetime means understanding the service's dependencies: a singleton must not capture a scoped `DbContext`.

Middleware forms an ordered pipeline around each request. In this application, exception handling and request logging run early; HTTPS redirection and CORS are configured before authentication and authorization; and the endpoints are mapped after those concerns. The order matters. For example, authentication establishes the user principal, and authorization evaluates that principal, so authentication has to run first.

Configuration is supplied through ASP.NET Core's configuration system instead of embedding environment-specific values in service logic. For detector freshness thresholds, the API binds configuration to an options type, validates the relationship between thresholds, and calls `ValidateOnStart`. This turns a bad configuration into a clear startup failure instead of inconsistent classifications later. The API also has development-only Swagger/OpenAPI, a health endpoint, and a version endpoint.

## 3. HTTP APIs, contracts, and asynchronous C# (1.5 minutes)

The API uses attribute-routed controllers to express resource-oriented HTTP endpoints. For example, intersection queries are separate from traffic-event operations, and an operator action is represented as a request to acknowledge or resolve a particular event. Controllers handle HTTP details, bind input, invoke application services, and translate results into status codes and response bodies.

I use request and response DTOs rather than exposing EF Core entities as public API contracts. That gives the API control over what it returns and makes persistence changes less likely to break the client. The project uses C# records for compact contracts such as `TrafficEventResponse` and `IntersectionPageQuery`. `DateTimeOffset` represents timestamps with an unambiguous offset, and nullable fields represent data that may genuinely be unavailable.

`[ApiController]` supports binding and validation behavior at the HTTP boundary, and endpoints apply constraints such as valid page sizes. Business-specific validation remains in the application path: the traffic-event service checks the allowed type, severity, intersection, timestamp, and detector identity. The API returns status codes that communicate the result, including `400` for invalid requests, `401` for unauthenticated requests, `403` for insufficient permissions, `404` for missing resources, and `409` for a conflicting event state or duplicate detector identity.

The I/O path is asynchronous. Services use `async` and `await` for EF Core calls, and accept a `CancellationToken` from the request so database work can stop when the client disconnects or the request is cancelled. This avoids blocking a request thread while waiting for I/O and improves server scalability under concurrent load. I do not wrap naturally asynchronous I/O in `Task.Run`; that would consume another thread without making the database operation more asynchronous.

## 4. EF Core, SQL Server, and data consistency (2 minutes)

EF Core maps the domain entities to relational tables through `EconoliteDbContext`. Entity configurations are applied from the assembly, keeping table relationships and constraints out of controller code. The SQL Server provider gives us relational foreign keys, indexes, unique constraints, transactions, and migrations.

For read-only queries, I use `AsNoTracking` and project the required values into response contracts. The intersections query validates filters and page bounds, applies filters before pagination, and orders by name and then ID for deterministic results. That is preferable to loading every entity and filtering in memory: the query remains composable as `IQueryable`, and EF Core translates it into SQL so the database can filter and page the result set. A bounded page size also prevents a client from requesting an unbounded response.

For detector ingestion, each event has a source system and external event ID. The service checks for an existing pair, while a database uniqueness constraint is the final protection against duplicate requests that race each other. The request is treated as idempotent input: a retry cannot silently create a second copy of the same external event. The database constraint matters because an application-level check by itself is vulnerable to two simultaneous requests both observing that no event exists.

For acknowledge and resolve operations, the service updates the event and records its audit entry in one database transaction. If persistence fails, those related changes do not partially commit. The service publishes the SignalR update only after the transaction commits. That preserves the database as the source of truth. For a much higher-throughput production system, I would assess transaction isolation and contention under realistic load and could introduce an outbox for durable notification delivery; I would not claim that the current in-process publish is a distributed transaction.

## 5. Security, errors, and observability (1.5 minutes)

The login service looks up the user, verifies the password with ASP.NET Core's `IPasswordHasher<TUser>`, and asks a JWT service to issue a token. The API's JWT bearer handler validates the signature, issuer, audience, and lifetime. It maps role and name claims so ASP.NET Core authorization can evaluate them.

Authorization is enforced at the API, not just in the React interface. Policies define which roles may view traffic and acknowledge events, while critical-event resolution requires a supervisor or administrator. That keeps the policy server-side even if a client is modified. I would also make clear that this project has a development seeder and is not a complete external identity-provider integration.

For failures, global exception handling returns an RFC 9457 ProblemDetails response and includes a trace ID, while keeping internal exception details out of the client response. Request logging middleware creates a structured logging scope with trace and correlation identifiers, HTTP method, and path, returns the correlation ID in a response header, and records status and duration. The client or operator can provide that ID when investigating an error. Structured fields are easier to search and aggregate than a single formatted message.

SignalR is authenticated as well. Browser transports can send the bearer token in the query string when establishing the hub connection, so the API reads that token only for the traffic hub path. CORS permits configured client origins and credentials. In production, I would keep signing keys in a managed secret store and restrict origins to the deployed clients.

## 6. Real-time updates, testing, and architecture (2 minutes)

The traffic hub sends event-created, event-updated, intersection-status, and audit notifications to connected clients. The important design point is that SignalR is a notification channel, not durable storage. The API persists first and broadcasts after a successful commit. Clients reconnect and refresh from the API because they may have missed messages. For multiple API instances, this implementation still needs a SignalR backplane or a managed SignalR service.

The codebase is a modular monolith. Intersections, Traffic Events, Identity, and Audit have separate module-oriented folders and application interfaces, while running in one API process and sharing one `DbContext`. I would describe it as Clean Architecture-influenced, not claim that it is a fully isolated multi-project architecture. This keeps operations and transactions straightforward for the current scope. I would extract a module into a service only when independent scaling, deployment, ownership, or reliability isolation justified the additional network and operational complexity.

The test project includes ASP.NET Core integration tests that exercise the application over HTTP. It uses `WebApplicationFactory<Program>` to host the API in-process and replaces its database configuration with a dedicated SQL Server Express test database. Tests cover successful and failed login, protected endpoints, intersection list and not-found behavior, validation errors, ProblemDetails, and correlation headers. This checks real routing, middleware, serialization, authentication, and persistence configuration together rather than only testing isolated methods.

I use tests alongside compilation and static checks, not as a substitute for design review. One scope detail I would state accurately is that the later event/audit and SignalR changes were compile-checked, but their full test suites were not run at that point. Production deployment hardening, a production ingestion pipeline, and multi-instance SignalR scaling remain follow-up work.

## 7. Closing (30 seconds)

My .NET approach is to keep each layer responsible for one thing: ASP.NET Core handles transport and request composition, application services implement use cases, domain entities protect important state transitions, and EF Core persists data with relational guarantees. I use asynchronous I/O, explicit contracts, server-side authorization, transactions where changes must stay together, and structured diagnostics so the system is understandable when something goes wrong. I choose architecture based on the problem's scale and operational needs, and I validate behavior at the boundaries where failures matter.

## Basic Features: C# Language Fundamentals

For a .NET interview, I distinguish C# language features from the .NET runtime and libraries. The following are the fundamentals I expect to explain clearly and use in everyday backend code.

### Types, variables, and nullability

- **Built-in and inferred types:** C# has types such as `int`, `decimal`, `bool`, `char`, and `string`. `var` asks the compiler to infer a static type from the initializer; it is not dynamic typing. I use it when the type is obvious from the expression and explicit types when they improve readability.
- **Value and reference types:** Structs and most built-in numeric types have value semantics; classes and arrays are reference types. Assignment copies a value type's value, but copies a reference for a reference type. Know the consequences for mutation, equality, and parameter passing rather than treating value types as automatically faster.
- **Nullable values:** Nullable value types use syntax such as `int?`; nullable reference types use annotations such as `string?` when enabled. Nullable reference types provide compiler analysis, not automatic runtime validation. At API boundaries, validate untrusted input even when DTO properties are non-nullable.
- **Conversions:** Implicit conversions are safe according to language rules; explicit casts signal that conversion may lose information or fail. Parse user-provided strings with `TryParse` when invalid input is expected, and use checked arithmetic when overflow must be detected.

### Expressions, control flow, and pattern matching

- **Operators and conditions:** Understand arithmetic, comparison, logical operators, short-circuiting (`&&`, `||`), and null-coalescing (`??`). Use braces and clear conditions for code where business rules matter.
- **Control flow:** `if`/`else`, `switch`, `for`, `foreach`, `while`, `break`, and `return` control execution. Use early returns when they make validation and failure cases easier to read.
- **Pattern matching:** `is` patterns and switch expressions can concisely branch on a type, value, or shape. I use them when they make a domain decision clearer, not when a simple conditional is more readable.
- **Enums and constants:** Enums represent a closed set of named values when that set is part of the type contract. Constants and `static readonly` values serve different initialization and mutability needs; avoid scattering magic values through business logic.

### Methods, classes, and object-oriented design

- **Methods and parameters:** Methods declare inputs and a return type. C# supports overloads, optional and named parameters, and `ref`/`out` parameters; I favor ordinary return values unless another form makes the contract clearer. Keep methods focused on one coherent responsibility.
- **Classes, objects, and access control:** Classes define reference types; constructors establish valid initial state; properties expose data; and access modifiers such as `public`, `internal`, and `private` control visibility. Encapsulation means protecting invariants rather than exposing setters for every field.
- **Interfaces and abstraction:** An interface defines a contract that multiple implementations can satisfy. Use one where it represents a meaningful boundary, such as an application service or infrastructure dependency, not automatically for every class. An abstract class can share implementation and state; an interface is usually a lighter contract and permits a type to implement multiple interfaces.
- **Inheritance and polymorphism:** Inheritance expresses an “is-a” relationship and enables substitutability, but deep hierarchies make behavior harder to follow. Prefer composition when a type needs to delegate behavior without claiming it is a specialized form of another type.
- **Records, classes, and structs:** Records provide value-oriented equality and concise data modeling, often useful for DTOs. Classes are useful for identity-bearing entities and controlled lifecycle changes. Structs are value types best suited to small value-like data; large or mutable structs can be surprising.
- **Generics:** Generics let code work with types while preserving compile-time safety, as in `List<T>`, `Task<T>`, and `IReadOnlyCollection<T>`. Constraints can state what a type parameter must support. Prefer existing generic collection and framework APIs over untyped `object` collections.

### Collections, LINQ, and exceptions

- **Collections:** Know when to use arrays, `List<T>`, `Dictionary<TKey,TValue>`, `HashSet<T>`, and read-only collection interfaces. Choose based on required operations, ordering, uniqueness, and lookup behavior; do not expose a mutable collection when callers should only read it.
- **LINQ:** LINQ expresses filtering, projection, ordering, grouping, and aggregation. With in-memory `IEnumerable<T>`, operators generally execute against objects; with EF Core `IQueryable<T>`, expressions may be translated to SQL. Deferred queries execute when enumerated or materialized, so be intentional about when the database call occurs.
- **Exceptions:** Exceptions represent exceptional failures, not ordinary branching. Catch an exception only when the layer can recover, add meaningful context, or translate it into a suitable application/API error. Preserve the original exception when rethrowing with `throw;`, and avoid swallowing failures.

### Delegates, lambdas, and events

A delegate is a type-safe reference to a method; `Func<>` and `Action<>` are common generic delegates. Lambdas provide concise inline functions and are common in LINQ and callbacks. Events provide a publisher/subscriber pattern built on delegates. Understand that captured variables in a lambda remain accessible through a closure, and avoid retaining objects longer than intended through long-lived callbacks or event subscriptions.

### Asynchronous programming and resource lifetime

`Task` and `Task<T>` represent asynchronous work. `async`/`await` make it possible to await I/O without blocking a thread; they do not automatically make CPU-bound work faster. Pass `CancellationToken` through long-running and I/O operations when cancellation is supported, and avoid sync-over-async with `.Result` or `.Wait()`.

`using` statements and declarations dispose resources that implement `IDisposable`; `await using` supports `IAsyncDisposable`. Use deterministic disposal for resources such as streams and database contexts. The `using` directive also imports a namespace and is different from the `using` statement that manages object lifetime.

### Language features used in this project

The API uses nullable reference types, records for request/response contracts, classes with controlled state for domain entities, primary constructors for dependency injection, collection expressions for concise arrays, top-level statements for host setup, LINQ for EF Core queries, and asynchronous methods with cancellation tokens. In an interview, I explain both the syntax and why it fits that responsibility.

## Short follow-up answers

**Why use dependency injection?**

It makes dependencies explicit, lets ASP.NET Core manage lifetimes, and makes implementations replaceable in tests or when infrastructure changes. I still choose lifetimes carefully; a singleton should not depend on scoped request state.

**Why use DTOs instead of returning entities?**

DTOs are the API contract. They prevent persistence details from becoming accidental public behavior and allow each endpoint to return only the data the client needs.

**How do you prevent duplicate detector events?**

I use a stable source-system and external-event identifier, check it in the application, and enforce uniqueness in the database. The database constraint covers concurrent requests that an application check alone cannot safely prevent.

**Why publish SignalR only after saving?**

A notification should describe durable state. If persistence fails, I must not tell clients the event was successfully created or changed.

**When would you split the modular monolith into services?**

When there is a measured need for independent scaling, deployment, ownership, or failure isolation. Until then, a modular monolith keeps development and transactional behavior simpler.

## Additional .NET and C# Interview Questions

### How are C#, .NET, and ASP.NET Core different?

C# is the programming language. .NET is the runtime, libraries, and tooling platform used to build and execute applications. ASP.NET Core is the web framework on .NET for HTTP APIs, middleware, routing, authentication, and related web concerns. In this project, C# implements the API and domain logic, ASP.NET Core hosts and routes requests, and .NET provides the runtime and base libraries.

### What are the DI service lifetimes, and how do you choose one?

Transient creates an instance each time it is requested, scoped creates one instance per scope (normally one HTTP request), and singleton uses one instance for the application's lifetime. I choose based on ownership and thread safety, then check the dependency graph. A singleton must not capture a scoped service such as EF Core's `DbContext`; that can cause incorrect sharing across requests. The application services that use this project's `DbContext` are scoped.

### What does `async`/`await` do in an ASP.NET Core API?

It lets the request flow await I/O, such as a database query, without blocking a worker thread for the full wait. I return `Task` or `Task<T>` from asynchronous methods, await the EF Core operation, and pass the request `CancellationToken` through. I avoid `.Result` and `.Wait()` because they block and can exhaust available threads under load. I also avoid `Task.Run` around I/O; it does not make an already asynchronous database call more efficient.

### When would you use `ValueTask` instead of `Task`?

Only when profiling shows that an operation frequently completes synchronously and avoiding a `Task` allocation materially helps. `ValueTask` has more usage constraints and can make composition more complex, so `Task` is the clearer default for most application and API methods. I would not use `ValueTask` as a blanket performance optimization.

### What is the difference between `IQueryable<T>` and `IEnumerable<T>` with EF Core?

An EF Core `IQueryable<T>` builds an expression that the provider can translate to SQL. Filtering, ordering, projection, and pagination can therefore execute in the database. `IEnumerable<T>` represents in-memory iteration; materializing too early and then filtering can pull unnecessary rows into the application. I keep query composition server-side and materialize at a deliberate boundary, while checking that each expression is translatable by the provider.

### Why use `AsNoTracking`, and what other EF Core query risks do you consider?

`AsNoTracking` is appropriate for read-only queries because EF Core does not need to retain change-tracking state for the returned entities. I also project only the fields the response needs, paginate at the database, and watch for N+1 queries, accidental eager loading, and client-side filtering of large result sets. For writes, I use tracked entities or explicit update patterns so change detection works as intended.

### How do records differ from classes in C#?

Records provide value-oriented equality and concise syntax suited to data-centric types such as request and response DTOs. A normal class uses reference identity by default and is often a natural choice for entities with identity and lifecycle. Records are not automatically deeply immutable: their properties and referenced objects still determine what can change. In this codebase, records are used for API contracts, while domain entities are classes with controlled state transitions.

### What are nullable reference types, and do they validate incoming JSON?

Nullable reference types add compile-time analysis that helps identify possibly missing values. They do not add runtime null checks by themselves and are not a substitute for validating untrusted input at the API boundary. I use nullable annotations to express intent, then use request validation and domain checks to enforce actual requirements. This distinction matters because HTTP input can be malformed regardless of compiler annotations.

### What is the difference between a value type and a reference type?

A value-type variable contains its value; assigning it copies that value. A reference-type variable holds a reference to an object, so copying the variable copies the reference and both variables can point to the same object. Structs are value types and classes are reference types. I choose structs for small value-like data where value semantics make sense, not simply to try to avoid allocations; a large or mutable struct can be surprising and expensive to copy.

### What is the difference between middleware and MVC filters?

Middleware wraps the broader HTTP request pipeline and can apply to many endpoint types, which makes it a good place for cross-cutting concerns such as exception handling, correlation IDs, and request logging. MVC filters run within the controller/action execution pipeline and have MVC-specific context, so they are useful for concerns tied specifically to MVC actions. I choose the narrowest layer that owns the behavior and pay attention to pipeline ordering.

### Why use the options pattern instead of reading configuration throughout the code?

The options pattern binds configuration to a typed object, centralizes the setting contract, and supports validation. This API uses it for detector freshness thresholds and validates the values at startup. That makes dependencies explicit and catches invalid configuration early instead of scattering string-based lookups and discovering a problem during a request.

### How do you handle concurrency when two requests update the same entity?

The right approach depends on the business invariant and database behavior. Options include optimistic concurrency tokens such as SQL Server `rowversion`, a transaction with an appropriate isolation level, and a unique constraint for duplicate identities. A pre-check alone is not sufficient when requests can race. In this project, detector-event uniqueness is enforced by the database, and event state changes use a transaction; for higher contention I would evaluate a concurrency token and return a clear conflict response.

### What is the difference between a unit test and an ASP.NET Core integration test?

A unit test isolates a behavior, usually by testing a class with controlled dependencies. An integration test exercises collaborating components across a boundary. This repository uses `WebApplicationFactory<Program>` to run the API in-process and send HTTP requests, which verifies routing, middleware, serialization, authentication, and persistence configuration together. I use the least expensive test that proves the behavior, then add integration coverage where framework wiring or cross-layer contracts could fail.
