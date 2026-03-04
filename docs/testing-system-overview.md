# SmashTourney Testing Overview (Non-Technical)

This document explains how testing validates business behavior across the full stack and how test suites map to product modules.

## Testing Purpose

The testing strategy verifies that real user flows work end to end, not only that individual methods compile.

The test layers cover:

- authentication and session access
- tournament setup and player join flows
- bracket progression and completion
- realtime synchronization events
- persistence and recovery behavior

## Test Layers

## 1) API Integration Tests

Primary location:

- `ApiTests/`

Business focus:

- user can register, login, and logout
- authenticated users can create and join tournaments
- games progress from lobby to completion using authoritative state routes
- bracket snapshots and current matches remain consistent

## 2) Service-Level Behavior Tests

Primary location:

- `ApiTests/GameServiceTest.cs`
- `ApiTests/PlayerRepositoryTests.cs`
- `ApiTests/UserServiceTest.cs`

Business focus:

- service workflows produce correct game, player, and user outcomes
- bracket mode behavior is initialized correctly
- reported match state persists and reloads correctly

## 3) Realtime Contract Tests

Primary location:

- `ApiTests/RealTimeTest.cs`

Business focus:

- clients can connect to hub route
- connected clients receive join acknowledgments
- game-start broadcasts reach all clients in the same game group

## 4) Frontend Flow Tests

Primary location:

- `tourneyClient/tests/RequestService.test.ts`
- `tourneyClient/tests/PersistentConnection.test.ts`
- `tourneyClient/tests/FrontendLifecycleFlow.test.ts`
- `tourneyClient/tests/matchVoteFeedback.test.ts`

Business focus:

- frontend request and response contracts match API routes
- realtime client behavior matches hub events
- auth + realtime + rest game flows work for power-of-two tournament sizes
- vote-ledger status and error feedback is validated for happy-path and critical outcomes

## Alignment With System Modules

This testing structure maps directly to the modules in `docs/full-stack-system-overview.md`:

- User Access Module → auth/session tests
- Tournament Setup Module → create/join route tests
- Lobby Module → players-in-game and realtime update tests
- Bracket Flow Module → flow state, bracket, current match tests
- Match Reporting Module → submit-match-vote consensus and completion tests
- Realtime + Recovery Module → hub broadcast and state polling tests

## Operational Quality Gates

Recommended quality gates before merge:

- all API integration tests pass
- all frontend tests pass
- power-of-two tournament matrix tests pass for single and double elimination
- no unauthorized access regressions in auth/session routes

## Business Outcome

When all suites pass, stakeholders can trust that users can sign in, form tournaments, play through brackets, and complete sessions with synchronized client state across backend and frontend components.
