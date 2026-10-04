# Implementation Status

This file is the durable record of implementation progress and verification. The [implementation plan](implementation%20plan.md) defines the intended features, domain rules, and acceptance criteria. Use this tracker for current status, validation evidence, and next actions; do not duplicate full requirements here.

**Baseline recorded:** 2026-10-02, transcribed from the implementation plan and current source structure. This baseline is not a fresh test run.

## Status Definitions

- **Planned:** Work has not started.
- **In progress:** Work has started; the next action and any blocker should be recorded.
- **Implemented:** Code for the agreed slice is present, but required verification may be incomplete or not recorded.
- **Verified:** Acceptance criteria and required checks passed; record the checks and date as evidence.
- **Blocked:** Progress depends on a named decision, dependency, or environment issue; include the unblock action.

A successful compile does not by itself mean a feature is verified. A feature is not production-ready unless production-specific requirements have also been implemented and checked.

## Feature Tracker

| Feature | Status | Current evidence | Next action |
| --- | --- | --- | --- |
| 1. Application foundation and contracts | Implemented | The implementation plan records the app shell, routes, health/version endpoints, CORS, modules, and initial contracts. Its validation section records prior client and backend checks; the run date is not recorded. | Rerun relevant checks when this feature changes; record command, date, and result. |
| 2. Database persistence | Implemented | The implementation plan records EF Core/SQL Server, migration, seed data, and persisted API reads. Prior integration validation is recorded without a run date. | Rerun persistence and migration checks when changed; record exact evidence. |
| 3. Authentication and authorization | Implemented | JWT login, role policies, protected API routes, and client auth are present; prior API and frontend validation is recorded in the plan without a run date. | Rerun auth and `401`/`403` checks when changed; record exact evidence. |
| 4. Error handling and structured logging | Implemented | ProblemDetails, request logging, correlation IDs, and client error parsing are present; prior validation is recorded in the plan without a run date. | Rerun error-contract checks when changed; record exact evidence. |
| 5. Intersection monitoring | Implemented | List/detail routes, server-side filters and pagination, detector freshness, and client views are present in the codebase. A current test result is not recorded in the status notes. | Run focused API and client tests; record commands, date, and results. |
| 6. Traffic events and simulated detector | Implemented | Event APIs and a simulated-detector client workflow are present. The simulator is restricted to Development. A current test result is not recorded in the status notes. | Run focused event tests; record results. Keep simulator development-only unless its security model is deliberately changed. |
| 7. Acknowledge, resolve, and audit | Implemented | The plan records transactional event/audit updates and post-commit notifications; it says backend compile checks ran, but unit and integration tests were not run at that time. | Run focused unit and integration tests, then record results and date. |
| 8. SignalR real-time updates | Implemented | The plan records the authenticated hub and client reconnect handling; it says compile checks ran, but unit/integration tests and smoke testing were not run at that time. Multi-instance hosting needs a backplane or managed SignalR service. | Run focused real-time tests and a smoke workflow; record results. Track multi-instance infrastructure separately. |
| 9. Deployment and production readiness | Planned | The implementation plan lists deployment hardening as the remaining feature. | Define deployment target and acceptance criteria, then plan health/readiness, secrets, migration, monitoring, and deployment validation. |

## How to Keep This Current

1. **When work starts:** Move the relevant row to `In progress`. Add the developer or issue/PR reference if useful, the next concrete outcome, and any blocker. Do not mark the row in progress just because an Agent session was opened; start when implementation work begins.
2. **During Agent mode:** Track immediate steps in the chat or task panel, for example: confirm the contract, update the API service, update the client, run focused tests. Ask Copilot for a brief milestone update with completed work, evidence, and the next action. The chat checklist is temporary; this file is the durable handoff.
3. **At a meaningful milestone or pause:** Update the feature row's evidence and next action. Keep it concise and outcome-based. Do not create separate Markdown progress logs for each prompt or agent run.
4. **After validation:** Record the exact command/test scope, date, and result. For example: `2026-10-02: dotnet test ... --filter ... — passed`. If a command cannot run, record why and leave the feature `Implemented` or `Blocked`, not `Verified`.
5. **At completion:** Compare the result with the acceptance criteria in the implementation plan. Move to `Verified` only after required checks pass. Keep future enhancements and production caveats in the next-action/evidence cell or the plan, not hidden in a completion claim.
6. **At handoff:** Include the feature status, last verified checks, unresolved assumptions, and the single next action in the Agent's final update. Confirm this tracker reflects that handoff.

## Progress Update Format

Use this short format in Agent mode and when updating the tracker:

```text
Feature/status: [feature] — [In progress / Implemented / Verified / Blocked]
Completed: [observable outcome]
Validation: [command or test scope, date, result; or not run and why]
Next: [one concrete next action]
Blocker/assumption: [only when applicable]
```

Keep the status evidence factual. Copilot may draft updates, but the developer confirms that implementation and validation claims are accurate before accepting them.
