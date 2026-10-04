# React Interview Talk: Core Features and Practical Use

*First-person interview script for approximately 10 minutes at a steady interview pace. The follow-up answers are optional and are not part of the timed talk. Use the headings as cues rather than reading them as a checklist.*

## 1. What React is (1 minute)

I think of React as a JavaScript library for building user interfaces from components. Its central idea is declarative rendering: I describe what the interface should look like for the current props and state, and React updates the DOM when that data changes. I focus on the state transition and the resulting UI, rather than manually finding and changing DOM elements throughout the application.

A React application is a tree of components. A page can compose feature components, and those can compose smaller reusable pieces. In this traffic dashboard, for example, the application shell composes the top navigation and routed pages; a dashboard page can then compose summary metrics, an intersection list, and active events. This structure keeps the interface understandable and lets each component have a clear responsibility.

React is not the whole application stack. React handles rendering and component behavior. In this project React Router handles client-side routing, Vite handles development and bundling, TypeScript provides static types, and SignalR provides real-time communication. Separating those responsibilities makes it easier to explain what React itself is responsible for.

## 2. Components, JSX, props, and composition (1.5 minutes)

A component is usually a function that receives inputs and returns JSX. JSX looks like HTML, but it is syntax for describing React elements and can contain JavaScript expressions in braces. I keep components small enough to understand, but not so small that every line becomes an abstraction without value.

Props are inputs from a parent. They let a reusable component display different data without owning that data. For example, a status badge can receive a status string and render the correct label and accessible visual treatment. Props are read-only: a child should not mutate the parent's data. If a child needs to request a change, it can call a callback passed by the parent.

Composition is one of my preferred design techniques. Instead of building a large component with many modes, I combine focused components and pass content through props such as `children`. That makes page structure visible and supports reuse without forcing unrelated screens into one generic component. I keep business rules and network access out of purely presentational components when that separation makes behavior easier to test.

Lists need stable keys. A key lets React match an item between renders when an item is inserted, removed, or reordered. I use a stable identifier from the domain, not an array index, if the list can change. An index can cause state or DOM identity to be associated with the wrong row after sorting or insertion.

## 3. State, events, and rendering (1.5 minutes)

State represents information that changes over time and affects what the component renders. I use `useState` for straightforward local state, such as whether a dialog is open or which filter value is selected. When a user interacts with a controlled input, the input value comes from React state and its change handler updates that state. This gives the component one clear source of truth for the field.

Calling a state setter schedules a render; it does not mutate the value in the already-running function. React may batch updates, so I use the functional updater form when the next value depends on the previous value, such as `setCount(current => current + 1)`. This avoids relying on a potentially stale value captured by a closure.

Rendering should be pure: given the same props and state, a component should return the same UI and avoid side effects. I do not perform fetches, mutate global state, or subscribe to a socket during render. Event handlers are where I respond to user actions, and effects are where I synchronize with systems outside React.

For more involved state transitions, I use `useReducer`. A reducer receives the current state and an action and returns the next state. In this project, the dashboard reducer handles loading, successful data, errors, and incoming traffic events. Keeping transitions in one reducer makes it easier to inspect behavior and test cases such as deduplicating an event or removing one that has been resolved.

## 4. Hooks, effects, and lifecycle (2 minutes)

Hooks let function components use React features such as state, context, refs, and effects. Hooks must be called at the top level of a component or custom hook, not conditionally or inside loops. That rule lets React associate hook state with the same call order on every render.

`useEffect` synchronizes a component with an external system after rendering. Typical examples include a subscription, a browser API, or a network request. The dependency list tells React when the synchronization needs to be repeated; it should reflect the reactive values used by the effect. If the effect creates a subscription or connection, its cleanup should remove the listener or stop the connection. This avoids leaks and duplicate handlers when dependencies change or the component unmounts.

In the dashboard, a SignalR hook establishes a connection after the user is authenticated, registers handlers, updates connection status, and cleans up the handlers and connection on teardown. That is a good example of an effect because SignalR is an external system. By contrast, I do not use an effect to calculate a value that can be derived directly from props or state during rendering; duplicating derived data in state creates synchronization problems.

I use `useRef` for mutable values that need to survive renders without causing a render themselves, such as a connection instance or a reference to the latest callback. I use custom hooks to package reusable behavior, for example authentication access or live traffic updates. A custom hook shares logic, not component state automatically; each call generally has its own hook state unless the state is lifted or provided through context.

## 5. Context, forms, and application state (1 minute)

Context makes a value available to a subtree without passing the same prop through every intermediate component. In this client, an authentication provider exposes the signed-in user and login/logout operations to routes and feature components. I use context for genuinely shared values with a meaningful scope, not as a default container for every piece of state. Frequently changing, very broad context can cause many consumers to render, so I consider provider boundaries and state ownership.

I separate state by what owns it. A dialog's open state is local UI state. The authenticated session is shared session state. Intersections and events are server-owned data, which brings concerns such as loading, errors, caching, retries, and refresh. For a larger client, I would evaluate a server-state library such as TanStack Query rather than hand-building cache and invalidation behavior. A client-side global store is useful only when it solves a concrete coordination problem.

For forms, I use controlled inputs when React needs to validate or react to each value change. For simpler forms, native browser behavior and form submission APIs may be enough. In either case, I keep validation understandable, label inputs accessibly, and handle pending and error states so users know whether a submission succeeded.

## 6. Async UI, errors, and React 19 (1 minute)

Asynchronous work creates more than a success path. I represent initial loading, successful empty results, populated results, and request failure as distinct UI states. A failed request should not render as though a query successfully returned no data. I also cancel requests when appropriate, such as when the component unmounts or the user changes the query, and I avoid updating the UI from obsolete results.

Error boundaries are useful for catching rendering errors in a subtree and showing a fallback UI. They do not replace handling expected request failures in application state, and they do not catch every kind of asynchronous error. I use them as a resilience boundary around rendering, while treating network and validation errors through the relevant request workflow.

React 19 adds capabilities for common async and form workflows, including Actions and hooks such as `useActionState`, `useFormStatus`, and `useOptimistic`. I choose these when their semantics fit the interaction, not simply because they are new. Optimistic UI is appropriate when immediate feedback is valuable and rollback is well-defined. For consequential operator actions, such as acknowledging an incident, I prefer to show success only after the server confirms the change unless the product has a carefully designed optimistic and rollback experience.

## 7. Performance, testing, and close (2 minutes)

I start performance work by measuring. I look for expensive rendering or computation with the React Profiler, reduce unnecessary state scope, render only the data a view needs, and paginate or virtualize genuinely large lists. `memo`, `useMemo`, and `useCallback` are tools, not defaults; they add complexity and are most useful when measurement shows repeated work or reference identity is causing a meaningful problem. I also consider deferred rendering for expensive non-urgent updates when it improves responsiveness.

I test observable behavior. Component tests verify what users see and do: a loading state, an actionable error, a permission-aware control, or an event appearing after an update. I avoid tests that merely assert a component's internal implementation. For critical workflows, integration or browser-level tests can verify that routing, API behavior, and user interaction work together.

My overall approach is to keep components focused, state ownership explicit, rendering pure, and effects limited to synchronization with external systems. I use composition to build features, test user-visible behavior, and optimize only when evidence points to a bottleneck. That combination keeps a React application easier to extend without turning it into either one giant component or an unnecessary framework of abstractions.

## Basic Features: JavaScript, TypeScript, and React

For a React interview, I prepare both the language fundamentals and the UI model. React code is JavaScript or TypeScript; JSX is syntax for describing the UI, not a separate programming language.

### JavaScript fundamentals

- **Bindings and scope:** Use `const` by default and `let` when reassignment is needed; avoid `var` in modern code because its function-scoping and hoisting behavior is easier to misuse. `const` prevents rebinding, not mutation of an object. Understand block scope, function scope, and lexical scope.
- **Types and comparisons:** Know strings, numbers, booleans, `null`, `undefined`, objects, arrays, and functions. Prefer strict equality (`===`) to avoid implicit coercion. Use optional chaining (`?.`) and nullish coalescing (`??`) when values can be missing; distinguish a missing value from valid falsy values such as `0` or an empty string.
- **Functions and closures:** Function declarations and arrow functions are both common. Arrow functions capture lexical `this`; they are convenient for callbacks but are not a drop-in replacement for every method. A closure retains access to variables from its lexical scope, which explains stale values in long-lived React callbacks and effects.
- **Objects and arrays:** Destructure values for clear access, use spread/rest syntax to copy or collect values, and use methods such as `map`, `filter`, and `find` to transform collections. In React state, create a new array or object instead of mutating the existing state value. Spread is a shallow copy, so nested updates still need to copy the changed path.
- **Modules:** Use `import` and `export` to divide code into explicit modules. Keep feature APIs, components, types, and shared utilities in clear ownership boundaries.
- **Errors and asynchronous work:** Promises represent future results. `async` functions return promises, and `await` makes promise-based control flow easier to read. Use `try`/`catch` for failures you can handle, and remember that `fetch` resolves for HTTP error statuses, so callers must check `response.ok`. Use `AbortController` when a request should be cancelled.

### TypeScript fundamentals

- **Static types:** TypeScript checks many mistakes before runtime, but its types are erased from the JavaScript bundle. A type annotation does not validate JSON received from an API; runtime data still needs validation where trust boundaries require it.
- **Object shapes:** Use `type` or `interface` to describe contracts. Both can describe object shapes; choose the local convention and use interfaces when declaration merging or extension is useful. Use `readonly` where preventing reassignment communicates intent, remembering it is a compile-time constraint rather than deep runtime immutability.
- **Unions and narrowing:** A union such as `'loading' | 'ready' | 'error'` models a finite set of states. Narrow values with checks such as `typeof`, `in`, discriminant properties, or exhaustive `switch` statements so each case is handled safely.
- **Generics:** Generics preserve relationships between input and output types. For example, an HTTP helper `getJson<T>()` can return the expected response type while sharing request behavior. A generic type is not proof that an untrusted response actually matches `T`.
- **Nullability:** Model optional data explicitly with `?`, `null`, or a discriminated state instead of using non-null assertions to silence the compiler. Handle missing API values intentionally, especially for operational data where “unknown” must not look like zero or healthy.

### React fundamentals

- **Components and JSX:** Components are functions that return UI descriptions. JSX can contain expressions in braces and should remain declarative; avoid direct DOM manipulation for normal rendering.
- **Props and state:** Props are read-only inputs from a parent. State is data that changes over time and affects rendering. State setters schedule a future render, so use functional updates when a new value depends on the previous value.
- **Events and forms:** Event handlers respond to user actions. A controlled input gets its value from React state and updates it in `onChange`; uncontrolled inputs keep their current value in the DOM and can be read through a ref or form submission. Choose the simplest model that meets validation and interaction needs.
- **Conditional rendering and lists:** Use ordinary JavaScript expressions and conditions to select UI. Render collections with stable keys derived from item identity, not array positions when the list can change.
- **Hooks and state ownership:** Call hooks at the top level of components or custom hooks. Use `useState` for simple local state, `useReducer` for related transitions, and context for values genuinely shared across a subtree. Lift state only to the nearest common owner that needs to coordinate it.
- **Purity and effects:** Keep render calculations pure. Use `useEffect` to synchronize with external systems, declare the reactive dependencies it uses, and clean up subscriptions or timers. Derive values during render rather than storing duplicate state when possible.
- **Composition:** Build pages from focused components and pass children or callbacks where appropriate. Prefer composition over a single component with many unrelated modes.

I use these fundamentals in the project: TypeScript contracts describe API data, feature components compose the operational pages, reducer transitions update dashboard state, context exposes the authenticated session, and an effect manages the SignalR connection lifecycle.

## Short follow-up answers

**When would you use `useState` versus `useReducer`?**

I use `useState` for a small number of independent values. I reach for `useReducer` when multiple actions update related state or when explicit transitions make the behavior easier to understand and test.

**What is the difference between props and state?**

Props are inputs supplied by a parent; state is data a component or its owner manages over time. Both can affect rendering, but a component should not mutate its props.

**When should you use an effect?**

When synchronizing with an external system such as a subscription, timer, or imperative browser API. I avoid effects for values that can be derived during render or for event-specific work that belongs in an event handler.

**How do you decide whether to use a global store?**

I first identify who owns the data and which parts of the UI need it. Local state, context, and server-state tools each solve different problems; I add a global store only when shared client-owned state genuinely needs coordinated updates.

**What is your approach to optimistic updates?**

I use them when the experience benefits and I can define rollback behavior. For safety- or audit-sensitive actions, waiting for server confirmation is often the clearer and more reliable choice.

## Additional React Interview Questions

### How do you reason about effect dependencies and stale closures?

An effect closes over values from the render that created it. I include every reactive value the effect reads in its dependency list, and I restructure the code when that dependency behavior is not what I intend. Omitting a dependency can leave a subscription or callback using stale state. I do not suppress the dependency lint rule just to make an effect run less often; I separate event logic from synchronization logic and use cleanup for subscriptions.

### What does concurrent rendering mean in React?

Concurrent rendering allows React to prepare, pause, resume, or abandon render work so urgent updates can remain responsive. It does not mean my component code runs concurrently on multiple JavaScript threads. I keep render pure because React may restart render work, and use transitions to mark non-urgent updates rather than performing side effects during render.

### How do `useTransition` and `useDeferredValue` differ?

`useTransition` marks a state update as non-urgent and exposes a pending state. `useDeferredValue` lets a consumer render using a previous value while an updated value is prepared. Both can keep interaction responsive when rendering is expensive, but neither is a network debounce or a substitute for request cancellation. I would use them for measured rendering bottlenecks, such as an expensive result view, not for every search field.

### How do React 19 Actions relate to forms and mutations?

React 19 provides form and action APIs such as `useActionState`, `useFormStatus`, and `useOptimistic`. `useActionState` can tie an asynchronous action to returned state and a pending indicator; `useFormStatus` lets a component inside a form read submission status; and `useOptimistic` can present a temporary state while work is pending. These APIs help structure UI feedback, but the server still validates, authorizes, and persists the operation. For an auditable traffic action, I would not show confirmed success until the API responds unless rollback semantics are explicit.

### What work is Suspense designed to coordinate?

Suspense shows a fallback when a descendant suspends using a supported mechanism, such as `lazy`-loaded code or a framework-integrated data source. An ordinary fetch inside `useEffect` does not automatically suspend; that component still needs explicit loading and error handling. I place boundaries at meaningful UI regions so a slow optional panel does not unnecessarily blank the whole application.

### What does the React Compiler change about memoization?

When enabled and supported by the project, the React Compiler can automatically memoize values and components based on its analysis. It can reduce the need for manual `memo`, `useMemo`, and `useCallback`, but it does not remove the need for pure rendering or profiling. I check whether the project has adopted and configured the compiler before relying on it. This client does not currently depend on compiler-based optimization.

### Does this project use React Server Components?

No. The client is a Vite-based React SPA that calls an ASP.NET Core API. React Server Components require a compatible server/framework and a deliberate server/client component boundary. They are not a generic optimization that can be enabled in any SPA, and they do not replace server-side authorization or secure API design.

### Why are stable list keys important beyond removing a warning?

Keys tell React which item retains its identity between renders. With stable IDs, React can preserve the correct component state when a list changes. With index keys in a sortable or mutable list, a row's local state can appear on another item after insertion or reordering. I prefer a stable key from the domain model and avoid generating a new key on every render, which would force remounts.
