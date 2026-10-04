# GitHub Copilot Instructions

## Goal

Help developers deliver correct, maintainable software quickly. Optimize for the smallest complete change that satisfies the domain need, not for maximum code generation. Treat Copilot as an accelerator and collaborator; the developer remains responsible for requirements, design decisions, security, and the code that is merged.

The latest explicit developer or user request takes precedence over these defaults. Do not silently expand scope or turn a suggestion into an implementation requirement.

## Workflow: Domain Requirement to Verified Change

For each task, use this sequence. Keep investigation proportional to the change.

1. **Understand the need.** Identify the user, the domain problem, the workflow, and the desired outcome. Read the supplied requirements fully. Find the owning code path, nearby conventions, relevant tests, and repository instructions before proposing a design.
2. **Make requirements testable.** Identify expected behavior, acceptance criteria, failure and boundary cases, permissions, data ownership, and out-of-scope work. Note assumptions explicitly. Ask a concise clarifying question when an unresolved domain decision would materially change the behavior; otherwise proceed with the narrowest reasonable assumption and state it.
3. **Choose a small design.** Trace the behavior to the layer that owns it. Reuse existing modules, APIs, components, types, and test helpers. For a cross-layer or risky change, summarize a short implementation and validation plan before editing. Avoid broad repository surveys when one local read can resolve the question.
4. **Implement a vertical slice.** Make the smallest coherent change that delivers the requested behavior. Keep API contracts, backend behavior, client presentation, and tests aligned. Do not leave placeholder implementations, silently omit acceptance criteria, or mix unrelated cleanup into the change.
5. **Validate promptly.** After the first substantive edit, run the narrowest relevant test, type check, build, or lint check before more exploration or edits. When it fails, fix the same slice and rerun that check. Then run additional focused gates required by the change.
6. **Review as an engineer.** Inspect the final diff and consider correctness, regressions, authorization, validation, data consistency, cancellation, accessibility, error and loading states, performance, and secret exposure. Remove generated code that is redundant, speculative, or inconsistent with local patterns.
7. **Report clearly.** Summarize behavior changed, important decisions or assumptions, validation actually run and its result, and any remaining work or known limitations. Never claim a test, build, migration, or deployment succeeded unless it was run and observed to succeed.

## Working From Domain Requirements

Before coding, translate domain language into concrete behavior:

- Who performs the action, and what are they trying to accomplish?
- What is the valid lifecycle or state transition? Which layer owns that rule?
- What data is authoritative, optional, stale, or unavailable?
- What happens on duplicate requests, retries, concurrency, invalid input, missing resources, and insufficient permissions?
- Which changes must persist atomically? What must be auditable?
- What should the user see while loading, after success, for an empty result, and after failure or disconnection?
- Which examples or acceptance criteria demonstrate completion?

Do not infer domain rules from a label, mockup, or frontend behavior if the API or domain contract is authoritative. Preserve distinctions such as “no results” versus “request failed,” “unknown” versus zero, and “stale data” versus a healthy status. If a rule is ambiguous and consequential, ask before implementing it.

## Using Copilot Effectively

- Use **Ask** or chat to clarify unfamiliar code and compare options before changing architecture.
- Use **Agent mode** for a bounded, multi-file task with clear acceptance criteria. Ask it to inspect the owning path and tests first, implement the vertical slice, run named checks, and report changed files and limitations. Review its plan and edits as they happen.
- Use **inline suggestions** for local, well-understood code. Accept only suggestions whose behavior and edge cases you can explain.
- Break broad requirements into independently verifiable slices. Do not ask an agent to “build the whole product” in one instruction; that encourages guessed requirements, large diffs, and weak validation.
- Give the tool the relevant domain rules, constraints, existing patterns, desired outcome, and exact checks. A precise prompt reduces rework more than a longer prompt full of generic coding advice.
- Parallelize only independent investigations or tasks. Do not have multiple agents edit overlapping files or implement against unresolved contracts.
- Treat generated tests as a starting point. Check that they assert externally meaningful behavior and include failure, boundary, and authorization cases where relevant.
- Never paste credentials, signing keys, customer data, or other secrets into prompts, source files, or client-side environment variables.
- Do not run destructive commands, production migrations, or operations that can affect external systems without explicit authorization.

### Agent Mode: Implementation and Progress

Use Agent mode when the task requires the assistant to inspect code, make coordinated edits, and run checks. Keep the task bounded by a user outcome and acceptance criteria. For a small, local change, skip formal planning and work directly.

1. **Frame the task.** Provide the domain outcome, current behavior if known, acceptance criteria, constraints, and what is explicitly out of scope. Ask the agent to inspect the owning implementation and nearby tests before editing.
2. **Plan non-trivial work.** For multi-file, cross-layer, or risky changes, ask for a concise implementation and validation plan first. Review the affected layers, likely files, dependencies between steps, and test strategy. Correct misunderstandings before implementation. If the chat offers a Plan mode, use it; otherwise request the plan in Agent mode before asking it to proceed.
3. **Track milestones.** Ask the agent to keep a short checklist in the chat or task panel when available. Break work into observable steps such as “confirm API contract,” “implement service behavior,” “update client state,” and “run focused tests.” Mark a step complete only after its outcome is verified. Keep one step in progress at a time; revise the checklist if investigation changes the scope.
4. **Request useful updates.** Ask for a brief update after repository investigation, after each significant milestone, and when a blocker or scope change appears. A useful update says what was learned or changed, what is verified, and the next action. Avoid asking for repeated narration after every file edit.
5. **Validate as progress.** After the first substantive edit, run the narrowest relevant check before continuing to adjacent work. If it fails, fix that slice and rerun the same check. Do not mark implementation complete merely because code was generated or files were changed.
6. **Close the loop.** Ask the agent to review the final diff against each acceptance criterion and report completed steps, checks actually run and their results, assumptions, and remaining work. The developer reviews the diff and owns the final decision to accept it.

For long tasks, track progress by outcomes rather than elapsed time or number of files. A test passing is evidence for the behavior it covers, not proof that unrelated requirements are done. If work is paused or interrupted, preserve the checklist with completed items, the current blocker or next action, and any checks still required.

### Reusable task prompt

```text
Domain outcome:
[Who needs what, and why?]

Current behavior / relevant context:
[Known behavior, code path, contract, or links to files. Inspect nearby code and tests before editing.]

Acceptance criteria:
- [Observable success behavior]
- [Validation, error, permission, and edge behavior]

Constraints and out of scope:
[Compatibility, architecture, security, performance, and work not requested]

Please:
1. Identify the owning code path and state any material assumption.
2. For multi-step work, show a concise checklist and validation plan; keep it updated as outcomes are verified.
3. Implement the smallest complete vertical slice using existing patterns.
4. Add or update focused tests for the acceptance criteria.
5. After the first substantive edit, run the narrowest relevant validation before continuing.
6. Review the diff and report completed criteria, checks and results, assumptions, and remaining limitations.
```

## Repository Conventions

### Backend: `Web API/`

- Target .NET 10 and follow the existing ASP.NET Core controller and dependency-injection patterns.
- Keep controllers focused on HTTP binding, status codes, and response mapping. Put use-case behavior in the relevant application service and domain invariants in the owning domain entity or module.
- Keep boundaries between Intersections, Traffic Events, Identity, and Audit understandable. This repository is a modular monolith; do not claim modules are independently deployed services or introduce microservices without an explicit requirement.
- Use request/response contracts rather than exposing EF Core entities. Preserve nullable values and existing response semantics.
- Use EF Core with the SQL Server provider. For read-only queries, follow existing `AsNoTracking`, server-side filtering, bounded pagination, deterministic ordering, and projection patterns.
- Use asynchronous I/O and pass `CancellationToken` through controller, service, and EF Core calls. Avoid blocking waits and wrapping I/O in `Task.Run`.
- Enforce authorization at the API, validate inputs at the boundary, and keep secrets out of source control and client bundles.
- Preserve database constraints as protection for uniqueness and relationships. Keep related writes transactional. Publish SignalR notifications only after durable state changes succeed; treat the database/API as the source of truth.
- Use the existing ProblemDetails and correlation/logging behavior. Do not expose internal exception details or log passwords, tokens, or sensitive data.
- Treat the detector simulator as Development-only unless requirements explicitly change its security and operational model. Production ingestion, migration deployment, and multi-instance SignalR infrastructure require deliberate design.

### Client: `Client app/`

- Use the existing React 19, TypeScript, Vite, React Router, and Sass setup. Organize implementation by feature and reuse shared API and component utilities.
- Keep components focused. Use props for inputs, local state for local UI, context for appropriately scoped shared session state, and reducers when related state transitions benefit from explicit actions.
- Keep rendering pure. Use effects to synchronize with external systems, declare accurate dependencies, and clean up subscriptions, timers, and connections. Do not use an effect to calculate values that can be derived during render.
- Keep API contracts typed, use the shared HTTP client and `ApiError`, and handle cancellation where applicable. Never treat a failed request as a successful empty result.
- Preserve distinct loading, success, empty, error, stale, and disconnected states where relevant. The API owns server data and authorization; client-side permission-aware UI is not a security boundary.
- For SignalR, deduplicate by stable IDs, clean up handlers, show connection state, and refresh authoritative data after reconnecting.
- Prefer accessible semantic HTML, labeled controls, keyboard support, visible focus, and status meaning that does not rely on color alone.
- Do not add state libraries, dependencies, memoization, or abstractions by default. Follow local patterns and demonstrate a concrete need first.

## Validation and Completion

Use the narrowest check that exercises the edited behavior, then run the relevant broader gates when practical. Typical client checks are `npm run test`, `npm run typecheck`, `npm run lint`, and `npm run build` from `Client app/`. Typical backend checks are `dotnet build "Web API/Econolite API.csproj"` and `dotnet test "Web API.Tests/Econolite API.Tests.csproj"` from the workspace root. Check project scripts and test prerequisites before assuming these commands are available; API integration tests may require local SQL Server Express.

A task is complete when its acceptance criteria are met, focused tests pass or any blocker is reported accurately, no relevant diagnostics remain, and the final diff contains only the intended work. Do not change unrelated user work or claim production readiness based only on a successful local build.
