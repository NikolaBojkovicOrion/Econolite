# React Client Application: Technical Guide

## Purpose

The React client should become a Traffic Operations Dashboard for Econolite-style intelligent transportation systems. It should allow an authenticated operator to monitor intersections, see current traffic health, receive congestion alerts, acknowledge incidents, and understand when data is stale or unavailable.

The important frontend responsibility is not just displaying data. It is presenting operational information accurately, handling unreliable networks, protecting user actions, and giving operators confidence about what is current.

## Current Client State

The current client is a React 19 and Vite application using JavaScript and Sass. Its main entry point renders the `App` component inside `StrictMode`. The current `App` component is the default Vite starter screen with a local counter.

Current scripts include:

```json
{
  "dev": "vite",
  "build": "vite build",
  "lint": "oxlint",
  "preview": "vite preview"
}
```

The current app does not yet include:

- API calls
- Authentication
- Routing
- Server state management
- SignalR real-time updates
- Domain components
- Error and loading states
- Tests
- Production configuration

## Recommended Frontend Structure

A feature-oriented structure keeps transportation concerns close together and prevents one very large `App.jsx` file.

```text
src/
  app/
    App.jsx
    routes.jsx
    providers.jsx
  features/
    auth/
      authApi.js
      AuthProvider.jsx
      RequireRole.jsx
    intersections/
      intersectionApi.js
      IntersectionList.jsx
      IntersectionMap.jsx
    trafficEvents/
      trafficEventApi.js
      EventList.jsx
      EventStatusBadge.jsx
      useTrafficEvents.js
    realtime/
      signalRClient.js
      useLiveTrafficUpdates.js
  shared/
    api/
      httpClient.js
    components/
      LoadingState.jsx
      ErrorState.jsx
    config/
      environment.js
  styles/
    variables.scss
```

Organize by business feature rather than only by technical type. For example, the code that loads and displays traffic events should be easy to find in one feature area.

## Component Design

Components should have one clear responsibility and receive data through props. Keep business rules out of visual components when possible.

```jsx
function EventStatusBadge({ status }) {
  const label = status === 'Active' ? 'Active' : 'Resolved'

  return (
    <span className={`status status-${status.toLowerCase()}`}>
      {label}
    </span>
  )
}
```

A dashboard page can compose smaller components:

```jsx
function TrafficDashboard() {
  return (
    <main>
      <DashboardHeader />
      <ConnectionStatus />
      <IntersectionSummary />
      <ActiveEventsPanel />
      <IntersectionTable />
    </main>
  )
}
```

Avoid putting API calls, authentication rules, filtering logic, and all markup into one component. This makes testing and future changes harder.

## State Management

Separate state into three categories:

### Local UI state

Use `useState` for state that belongs only to one component, such as a selected row, modal visibility, or a filter input.

```jsx
const [selectedIntersectionId, setSelectedIntersectionId] = useState(null)
const [showResolveDialog, setShowResolveDialog] = useState(false)
```

### Server state

Intersections and traffic events are owned by the API. They need loading, caching, retry, invalidation, and error behavior. For a larger application, use a server-state library such as TanStack Query rather than manually duplicating request state in many components.

```jsx
const { data, isLoading, isError, refetch } = useQuery({
  queryKey: ['active-events'],
  queryFn: getActiveEvents,
})
```

### Session state

The authenticated user, access token strategy, roles, and session expiration belong in an authentication provider or dedicated auth module.

Do not put all application state into one global store by default. Use the smallest state scope that is appropriate.

## API Integration

Create one HTTP client so base URLs, headers, timeouts, and common error handling are consistent.

```js
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

export async function getActiveEvents(signal) {
  const response = await fetch(`${apiBaseUrl}/api/events?status=Active`, {
    signal,
    credentials: 'include',
  })

  if (!response.ok) {
    const problem = await response.json().catch(() => null)
    throw new Error(problem?.detail ?? 'Unable to load traffic events')
  }

  return response.json()
}
```

Important API-client practices:

- Use environment variables for the API URL.
- Support request cancellation with `AbortController`.
- Handle non-2xx responses consistently.
- Do not assume every response contains valid JSON.
- Avoid sending secrets from client-side code.
- Keep DTO mapping separate from visual components.
- Treat `401`, `403`, `404`, `409`, and `503` differently where the user experience requires it.

## Loading, Error, Empty, and Stale States

An operational dashboard must distinguish these states:

- **Loading**: the initial request is still running.
- **Empty**: the request succeeded but there are no active events.
- **Error**: the request failed and the data is unavailable.
- **Stale**: the last successful data is displayed, but it is older than the freshness threshold.
- **Disconnected**: the SignalR live connection is unavailable.

Do not display an empty table when the API request failed. That can mislead an operator into believing there are no incidents.

```jsx
if (isLoading) return <LoadingState label="Loading intersections" />
if (isError) return <ErrorState onRetry={refetch} />
if (events.length === 0) return <EmptyState label="No active traffic events" />

return <EventList events={events} />
```

A stale-data message should be clear:

```text
Last updated 2 minutes ago. Live detector data is currently unavailable.
```

The UI must not confuse “no congestion detected” with “no current detector data.”

## Authentication and Authorization

The backend must enforce authorization. The React app should only reflect those rules in the user interface.

For example, the client may hide a resolve action when the user lacks permission:

```jsx
{user.permissions.includes('traffic-events.resolve') && (
  <button type="button" onClick={openResolveDialog}>
    Resolve event
  </button>
)}
```

However, hiding the button is not security. The API must still reject unauthorized requests with `403 Forbidden`.

The client should also handle session expiration:

```js
if (response.status === 401) {
  // Clear session state and redirect to sign-in.
}
```

Prefer an established identity provider and secure cookie or token strategy. Never hard-code credentials, signing keys, or production secrets in the React bundle.

## Real-Time Updates with SignalR

The initial dashboard data should come from a normal HTTP request. SignalR should then keep the view current.

Conceptually:

```js
const connection = new HubConnectionBuilder()
  .withUrl(`${apiBaseUrl}/hubs/traffic`, { accessTokenFactory })
  .withAutomaticReconnect()
  .build()

connection.on('TrafficEventCreated', (event) => {
  queryClient.setQueryData(['active-events'], (current = []) => [
    event,
    ...current,
  ])
})

await connection.start()
```

Important real-time rules:

- Persist the event before broadcasting it.
- Treat the API/database as the source of truth.
- Show reconnecting and disconnected states.
- Refresh data after reconnecting because messages may have been missed.
- Prevent duplicate events in the client using event IDs.
- Clean up listeners when a component unmounts.
- Use a SignalR backplane or managed SignalR service when multiple API instances are deployed.

SignalR improves freshness; it does not replace reliable persistence.

## Congestion and Detector Data

A detector can be radar, a camera, an inductive loop, or another roadway sensor. A simulated detector is application code that pretends to send data from one of these devices.

Congestion means traffic demand exceeds the capacity of a road or intersection. It can be identified by low average speed, long queues, high traffic density, excessive delay, or abnormal volume.

Example event displayed by the client:

```json
{
  "id": "event-123",
  "intersectionId": 101,
  "type": "Congestion",
  "severity": "High",
  "averageSpeed": 8,
  "status": "Active",
  "detectedAt": "2026-09-29T10:15:00Z"
}
```

The dashboard should show severity, intersection, current status, time detected, and data freshness. It should avoid overstating certainty when the detector is stale.

## Mutations and Optimistic UI

Acknowledging an event changes server state. The safest default is to send the request, wait for success, then update or invalidate the cached data.

```js
async function acknowledgeEvent(eventId) {
  const response = await fetch(`/api/events/${eventId}/acknowledge`, {
    method: 'PATCH',
    credentials: 'include',
  })

  if (!response.ok) {
    throw new Error('The event could not be acknowledged')
  }

  return response.json()
}
```

Optimistic updates can make the interface faster, but they require rollback when the API rejects the action. Use them only when the user experience benefits and the rollback behavior is well defined.

For operations affecting safety or audit records, waiting for confirmed server success is often preferable.

## Accessibility

An operational dashboard should be usable with keyboard and assistive technology.

Important practices:

- Use semantic headings, tables, buttons, and labels.
- Ensure every form control has an accessible label.
- Do not communicate severity only by color.
- Provide visible focus states.
- Use `aria-live` carefully for new alerts.
- Keep keyboard focus inside dialogs until they close.
- Make error messages specific and actionable.
- Preserve readable contrast and touch targets.

Example:

```jsx
<div role="status" aria-live="polite">
  {connectionState === 'reconnecting'
    ? 'Reconnecting to live traffic updates'
    : 'Live traffic updates connected'}
</div>
```

## Performance

For a dashboard with many intersections and frequent updates:

- Request only the fields needed by each view.
- Paginate or virtualize large tables.
- Debounce search and filter inputs.
- Avoid re-rendering the entire dashboard for one event.
- Normalize or merge events by ID.
- Keep stable component boundaries.
- Measure with the React Profiler before optimizing.
- Use deferred rendering for expensive filtering when appropriate.

Do not add `useMemo` or `useCallback` everywhere automatically. First identify an actual rendering or computation problem.

## Security Considerations

The frontend is not a trusted security boundary. Users can inspect and modify JavaScript in the browser.

Therefore:

- Enforce authorization in the API.
- Validate all input on the server.
- Do not put secrets in `VITE_*` variables.
- Avoid rendering untrusted HTML.
- Use HTTPS in production.
- Configure Content Security Policy where practical.
- Avoid logging tokens or personal information.
- Use secure authentication storage and expiration rules.

## Testing Strategy

Test behavior rather than implementation details.

### Component tests

Examples:

- An active event displays the correct severity.
- A stale intersection shows a freshness warning.
- A user without permission does not see the resolve action.
- An API error displays a retry action.
- A disconnected SignalR state is visible to the operator.

### Integration tests

Examples:

- Loading the dashboard requests intersections and active events.
- Acknowledge sends the correct API request.
- A successful SignalR event appears in the active-event list.
- Reconnecting causes a refresh.

### End-to-end tests

The most valuable end-to-end flow is:

1. Sign in as an operator.
2. Open the dashboard.
3. Simulate congestion.
4. Verify the new event appears without a page refresh.
5. Acknowledge the event.
6. Verify the audit information is available.
7. Verify an unauthorized user cannot resolve it.

Use the testing tools already adopted by the team. If the application grows, a typical stack would be Vitest and React Testing Library for component tests, plus Playwright for browser workflows.

## Routing and Deployment

Use routing for separate operational views such as:

```text
/dashboard
/intersections
/intersections/:intersectionId
/events
/audit
```

The Vite build produces static assets. A production deployment can serve those assets through Nginx, a CDN, or cloud static hosting. The ASP.NET Core API should be deployed separately or behind the same reverse proxy.

Configure the API URL per environment:

```env
VITE_API_BASE_URL=https://api.example.com
```

Never commit production secrets. Remember that values included in a Vite client build are visible to users.

## Senior-Level Frontend Explanation

A strong interview explanation is:

> I would treat the React application as an operational client, not just a collection of screens. The API remains the source of truth, while React manages presentation and user interaction. I would separate local UI state from server state, model loading, empty, error, stale, and disconnected states explicitly, and use SignalR only to improve freshness. Authorization would be enforced by the API, with the client reflecting the user’s permissions. I would test the important workflows, measure performance before optimizing, and deploy the Vite build with environment-specific configuration and no secrets in the bundle.

## Questions to Prepare For

### Why should server state not always be stored in a global React store?

Server state belongs to the API and has different concerns from local UI state: caching, loading, retries, refetching, invalidation, and synchronization. A server-state library such as TanStack Query handles those concerns more reliably than manually copying every API response into a global store.

I would use local React state for a selected row or open dialog, a server-state library for intersections and traffic events, and an auth provider for the current session. A global store is appropriate when multiple unrelated parts of the application genuinely need shared client-owned state, but it should not become a cache for every API response.

### What happens when the browser loses connectivity during an acknowledgement?

The UI should not mark the event as acknowledged until the API confirms success. It should show a clear failure or offline message, preserve the event as active, and allow the operator to retry.

```js
try {
  await acknowledgeEvent(eventId)
  invalidateEvents()
} catch {
  setError('The acknowledgement was not saved. Check your connection and retry.')
}
```

The API should make the operation idempotent so retrying does not create duplicate audit records. For safety-related actions, confirmed server state is preferable to optimistic UI unless rollback behavior is carefully implemented.

### How do you avoid showing stale traffic data as current?

Store and display `lastUpdatedAt` or the detector heartbeat time. The client should calculate or receive a freshness status such as `Fresh`, `Delayed`, or `Stale`.

```jsx
function FreshnessLabel({ lastUpdatedAt }) {
  const ageMs = Date.now() - new Date(lastUpdatedAt).getTime()
  const isStale = ageMs > 120_000

  return <span>{isStale ? 'Data stale' : 'Live data'}</span>
}
```

The interface must distinguish “no congestion detected” from “no current detector data.” A stale warning should remain visible even when the last known traffic status is displayed.

### How do you prevent duplicate SignalR events in the UI?

Give every event a stable server-generated ID and merge incoming events by ID rather than blindly appending them.

```js
function addEvent(currentEvents, incomingEvent) {
  const withoutDuplicate = currentEvents.filter(
    (event) => event.id !== incomingEvent.id,
  )

  return [incomingEvent, ...withoutDuplicate]
}
```

The backend should also enforce idempotency. Client-side deduplication improves the display, but it cannot replace server-side uniqueness constraints.

I would register one SignalR listener per connection and remove it when the owning component or hook unmounts. After reconnecting, I would refresh from the API because messages may have been missed.

### How do you protect an operator action?

The API must enforce authentication and authorization. The React client can hide an action when the user lacks permission, but that is only a usability improvement and is not security.

```jsx
{user.permissions.includes('traffic-events.acknowledge') && (
  <button type="button" onClick={() => acknowledgeEvent(event.id)}>
    Acknowledge
  </button>
)}
```

The API should verify the user identity, role or policy, event state, and object-level access. It should return `401` for an unauthenticated user and `403` for an authenticated user without permission. The successful action should create an audit record containing the user, event, timestamp, and action.

### How do you test a real-time update?

Test the observable behavior rather than the SignalR implementation details:

1. Render the dashboard with an initial event list.
2. Mock or provide a SignalR connection.
3. Trigger a `TrafficEventCreated` message.
4. Verify the new event appears.
5. Trigger the same event again and verify it appears only once.
6. Simulate disconnect and verify the connection warning.
7. Simulate reconnect and verify that the client refreshes data.

Component tests can use React Testing Library, while Playwright can verify the full workflow against a running API and test SignalR behavior in a browser.

### How do you handle a slow API request or a failed retry?

Show a loading state for the initial request and preserve usable previous data during a background refresh. Use request cancellation when the user changes screens or filters quickly. Retry only transient failures, such as network errors or `503`, with a bounded exponential backoff.

Do not retry validation errors or authorization failures. After retries are exhausted, show an actionable error with a retry command and keep the failure visible in logs and monitoring.

For example, a request can use an `AbortController`:

```js
const controller = new AbortController()
const response = await fetch(url, { signal: controller.signal })

// Call controller.abort() when the request is no longer needed.
```

### What data should be paginated or virtualized?

Paginate data that can grow without a practical upper bound, such as audit history, historical traffic events, and large intersection lists. Use server-side filtering and sorting so the browser does not download the entire dataset.

Virtualize a long table when many rows must remain in one view. For example, a live event list containing thousands of intersections should render only the rows visible in the viewport.

Current active alerts may not need pagination if the result is deliberately capped, but the API should still define a limit and return a clear indication when more data exists.

### What secrets can safely be placed in a Vite environment variable?

Only values intended to be public can be included in a Vite client build. For example, a public API base URL or a non-secret feature flag is acceptable:

```env
VITE_API_BASE_URL=https://api.example.com
```

Vite variables are bundled into JavaScript that users can download. Never place passwords, JWT signing keys, database connection strings, private API keys, or privileged credentials in `VITE_*` variables. Secrets belong on the server or in the deployment platform's secret store.

### How would you split a large `App.jsx` into maintainable features?

I would identify the user workflows and move each one into a feature boundary:

```text
src/
  app/
    App.jsx
    routes.jsx
  features/
    auth/
    intersections/
    trafficEvents/
    realtime/
  shared/
    api/
    components/
```

The app shell would own routing and providers. Each feature would own its API functions, hooks, components, and tests. Shared components would contain genuinely reusable UI such as loading and error states, not business-specific traffic rules.

I would extract incrementally: first separate page-level components, then move API and state logic into hooks or feature modules, and finally add tests around each workflow. This reduces risk and avoids a large rewrite.

### What happens when React state changes, and what is reconciliation?

When state or props change, React calls the affected component functions to produce a new description of the UI. React compares that result with the previous render and commits the necessary DOM changes. Rendering is not the same thing as recreating the whole page; React reconciles the component tree and updates what changed.

Keys help React identify list items across renders. I use stable domain IDs for keys when a list can be inserted into, removed from, or reordered. An array index is only appropriate for a truly static list, because index keys can associate component state with the wrong item after reordering.

### Why can a state update appear not to take effect immediately?

State setters schedule an update; they do not change the state variable inside the currently executing render. React can batch updates, so code after a setter still sees the value from that render. When the next value depends on the previous value, I use a functional update such as `setCount(current => current + 1)`. This avoids stale closure bugs when updates are queued or repeated.

I avoid copying props into state unless I have a clear synchronization requirement. Derived values should usually be calculated during render, because storing a second copy creates two values that can drift apart.

### When do you use an effect, and how do you prevent effect bugs?

I use `useEffect` to synchronize with an external system after React commits, such as a subscription, timer, browser API, or SignalR connection. I do not use it for calculations that can happen during render or for work that belongs directly in a user event handler.

The dependency list must represent the reactive values used by the effect. When an effect creates a subscription or connection, its cleanup removes listeners and releases the resource. React Strict Mode may run an extra setup-and-cleanup cycle in development to expose effects that are not resilient; I make the effect safe to repeat instead of relying on it running exactly once. In this project, the SignalR hook owns connection setup, handlers, reconnect callbacks, and cleanup.

### What is the difference between `useTransition` and `useDeferredValue`?

Both let the UI keep urgent interactions responsive while less urgent rendering work is pending, but they express different intent. `useTransition` lets me mark a state update as non-urgent and expose whether that transition is pending. `useDeferredValue` lets a value used by an expensive part of the UI lag behind the newest value while React renders the newer result in the background.

For example, typing into a search box should update the input immediately, while a large table can render from a deferred query value. Neither API is a network debounce, cancellation strategy, or guarantee that work runs on another thread. I use them when rendering is measurably expensive, not as a default around every input.

### What are React 19 Actions and form-related hooks?

React 19 supports Actions for managing asynchronous mutations in transitions and forms. `useActionState` connects an action to state and a pending flag; `useFormStatus` lets a component inside a form read its submission status; and `useOptimistic` can render a temporary optimistic value while an action is pending.

I would use these when they simplify the actual form or mutation flow. An optimistic update still needs a clear failure and rollback story. For an operator action that must be auditable, I would generally wait for the API to confirm success unless the product explicitly calls for optimistic feedback and the UI can correctly recover from rejection. These React APIs do not replace server validation, authorization, or persistence.

### How do Suspense and error boundaries differ?

Suspense displays a fallback while a descendant is waiting on a Suspense-enabled operation, commonly lazy-loaded code or data managed through a compatible framework. It does not automatically catch a fetch started inside an ordinary `useEffect`; in that pattern, the component still needs explicit loading and error state.

An error boundary catches rendering errors in its child tree and can show a fallback instead of taking down the entire application. It does not generally catch errors thrown later by event handlers or arbitrary asynchronous callbacks, so expected API failures still need to be handled in the request workflow. I place boundaries around meaningful parts of the UI and keep request errors actionable.

### How do you decide whether to use `memo`, `useMemo`, or `useCallback`?

I measure first with the React Profiler and identify the expensive render or unstable prop that causes it. `memo` can skip rendering a component when its props are unchanged; `useMemo` can cache a calculation; and `useCallback` can preserve a function reference. They add complexity and shallow comparisons, so they are not automatically performance wins.

The React Compiler can perform some memoization automatically when a project enables and supports it. I would follow the project's compiler configuration and verify its behavior instead of adding manual memoization preemptively. This client does not need to claim compiler use to benefit from sensible component boundaries and server-side pagination.

### What are Server Components, and does this Vite client use them?

Server Components render on the server and can access server-side resources without shipping that component's implementation to the browser. They cannot use client-only state, effects, or event handlers. Interactive Client Components are used for those behaviors, with framework-specific boundaries between server and client code.

This project is a Vite client-side React application backed by an ASP.NET Core API; it does not use React Server Components. I would adopt them only through a framework and deployment architecture that supports them, not treat them as a switch that can be added to any existing SPA. Regardless of rendering model, authorization and sensitive data access must remain on trusted server-side code.

### What is the difference between `useRef` and state?

State is for values that affect rendered output: updating it schedules a render. A ref stores a mutable value across renders without scheduling one. I use refs for values such as a DOM element, timer ID, or external connection handle when changing that value should not itself update the UI. If a user-visible value changes, it belongs in state rather than only in a ref.

### How would you handle a component that subscribes to an external store?

For a custom external store, `useSyncExternalStore` provides React with a subscription function and a snapshot reader. React can then observe store changes consistently, including with concurrent rendering. I would prefer the store's official React integration when one exists and avoid hand-rolling subscriptions in many components. For this project's authentication, React context is sufficient; for API-owned intersections and events, a server-state library may be more appropriate if caching and invalidation needs grow.
