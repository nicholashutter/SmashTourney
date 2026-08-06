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
- `ApiTests/BracketEngineTest.cs`
- `ApiTests/SessionResumeTest.cs`

Business focus:

- service workflows produce correct game, player, and user outcomes
- bracket mode behavior is initialized correctly
- vote-ledger and bracket progression state persists and reloads correctly
- bracket engine produces expected structures for power-of-two and odd player counts in both single and double elimination
- sessions can be resumed after disconnect/reload without losing bracket state
- odd-player brackets auto-resolve bye-involved matches so real-player flow does not stall

## 3) Realtime Contract Tests

Primary location:

- `ApiTests/RealTimeTest.cs`

Business focus:

- clients can connect to hub route
- connected clients receive join acknowledgments
- game-start broadcasts reach all clients in the same game group

## 4) End-to-End & Route-Coverage Tests

Primary location:

- `ApiTests/AuthEndToEndTest.cs`
- `ApiTests/AuthRateLimitTest.cs` (uses `appsettings.RateLimitTesting.json` — start the API with that profile to exercise the suite)
- `ApiTests/GameAccessControlTest.cs`
- `ApiTests/GameRouteAccessTest.cs`
- `ApiTests/GameRouterTest.cs`
- `ApiTests/MultiGameTest.cs`

Business focus:

- full register → login → create → join → vote → completion flows across both real and in-memory hosts
- route-level authorization and access enforcement across the full game router
- multiple concurrent games do not interfere with each other
- rate-limit behavior matches the configured policy under load

## 5) Frontend Flow Tests

Primary location:

- `tourneyClient/tests/RequestService.test.ts`
- `tourneyClient/tests/PersistentConnection.test.ts`
- `tourneyClient/tests/FrontendLifecycleFlow.test.ts`
- `tourneyClient/tests/matchVoteFeedback.test.ts`
- `tourneyClient/tests/ValidationService.test.ts`
- `tourneyClient/tests/GameBrowserService.test.ts`
- `tourneyClient/tests/GameRoutes.test.ts`
- `tourneyClient/tests/InMatchRouting.test.ts`
- `tourneyClient/tests/PlayerSessionService.test.ts`
- `tourneyClient/tests/ReturnPath.test.ts`
- `tourneyClient/tests/SignInFeedback.test.ts`
- `tourneyClient/tests/soundService.test.ts`
- `tourneyClient/tests/useBracketMotionConfig.test.ts`

Business focus:

- frontend request and response contracts match API routes
- realtime client behavior matches hub events
- auth + realtime + rest game flows work for both power-of-two and odd player counts
- odd-player matrices verify bye auto-resolution for single and double elimination modes
- vote-ledger status and error feedback is validated for happy-path and critical outcomes
- input validation rules consistently reject unsafe or malformed client input
- game-route and in-match routing logic moves the user to the correct screen for the current `GameState`
- sign-in feedback and return-path bookkeeping handle redirect-after-login correctly
- sound service honours mute preference and the autoplay policy (plays only from user gestures or after sufficient mount time)
- bracket motion config returns the reduced-motion timings when `prefers-reduced-motion` is set

## Alignment With System Modules

This testing structure maps directly to the modules in `docs/full-stack-system-overview.md`:

- User Access Module → auth/session tests
- Tournament Setup Module → create/join route tests
- Lobby Module → players-in-game and realtime update tests
- Bracket Flow Module → flow state, bracket, current match tests
- Match Voting Module → submit-match-vote consensus and completion tests
- Realtime + Recovery Module → hub broadcast and state polling tests
- Auth Limits & Multi-Game Isolation → rate-limit and multi-game tests

## Test Configuration Files

- `appsettings.RateLimitTesting.json` (under `tourneyAPI/`) — the rate-limit configuration profile consumed by `AuthRateLimitTest`. The API must be started with this profile (e.g. `--launch-profile http-rate-limit-testing`) for that suite to exercise real rate limiting.

## Operational Quality Gates

Recommended quality gates before merge:

- all API integration tests pass
- all frontend tests pass
- tournament matrix tests pass for both power-of-two and odd player counts in single and double elimination
- no unauthorized access regressions in auth/session routes
- rate-limit suite passes against the rate-limit testing profile

## Business Outcome

When all suites pass, stakeholders can trust that users can sign in, form tournaments, play through brackets, and complete sessions with synchronized client state across backend and frontend components.
