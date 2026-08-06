# PR 5 — Doc updates: drift fixes, blank-password dev mode, polish notes

**Branch:** `pr/doc-updates`
**Size:** 6 files, doc-only
**Risk:** Low. No code changes.

## What changes

- **`Readme.md`** — fixed two drifts:
  1. The "Development dummy users" section is rewritten to reflect the
     new blank-password auth-bypass behaviour introduced in PR 1. The
     dev launch is now a "dev-only auth bypass": all 16 dummy accounts
     are seeded with an empty password and you sign in with a blank
     password field. The rationale (why the production password
     validators are bypassed for the dev seed) is documented inline.
  2. The "Use Demo Account" claim is softened to honestly describe the
     endpoint and note that the React home page action is planned but
     not yet wired up.

- **`docs/api-system-overview.md`** — added routes the prior audit
  flagged as missing from the catalog (`POST /Games/EndGame/{gameId}`,
  `GET /Games/GetActiveGames`, `GET /Games/GetPlayerSession/{gameId}`,
  `POST /users/register`), the `MapIdentityApi` group, the `/Players`
  CRUD group, and the SignalR hub URL `/hubs/GameServiceHub`. Annotated
  `GetPlayersInGame` as `POST` (the previous "polling-safe read"
  framing was ambiguous). Added a one-line note that the per-player
  response shape follows the client `Player` type contract.

- **`docs/frontend-system-overview.md`** — added the new bracket-system
  files to the "Bracket and Match Experience" section
  (`WinnerCelebration`, `BracketMatchDetailPopover`,
  `useBracketMotionConfig`, `soundService`, `drawService`). Documented
  the YOU tag behaviour (hidden entirely for eliminated players) and
  the `CelebrationOverlay` removal. Forward-pointing proposal note
  updated from "not yet implemented" to "implemented; remains as
  design record". Added reliability notes for `prefers-reduced-motion`
  coverage and the sound service's autoplay-policy handling.

- **`docs/full-stack-system-overview.md`** — added the missing game
  routes (kept in sync with the api doc), annotated `GetPlayersInGame`
  as `POST`, added a line about the SignalR hub, and added a new
  "Data Model Maintenance" section pointing at `tourneyAPI/Migrations/`
  as the schema home and `dotnet ef migrations add` as the workflow.

- **`docs/testing-system-overview.md`** — added the 8 missing API test
  files (`AuthEndToEndTest`, `AuthRateLimitTest`, `GameAccessControlTest`,
  `GameRouteAccessTest`, `GameRouterTest`, `MultiGameTest`,
  `BracketEngineTest`, `SessionResumeTest`) and the 6 missing frontend
  test files (`GameBrowserService`, `GameRoutes`, `InMatchRouting`,
  `PlayerSessionService`, `ReturnPath`, `SignInFeedback`). Renumbered
  the section list to add a new "End-to-End & Route-Coverage Tests"
  section. Added a "Test Configuration Files" section for
  `appsettings.RateLimitTesting.json`. Added the 2 new test files
  shipped with the polish work (`soundService.test.ts`,
  `useBracketMotionConfig.test.ts`).

- **`docs/proposals/frontend-animated-brackets-proposal.md`** —
  committed as a design record for the polish work (now in PR 1's
  branch already; this PR is just adding the doc to the tree).

## Why this is its own PR
- The drift fixes and the polish/blank-password doc updates are
  independent of the code changes. Reviewing them as their own PR
  means doc-only diffs are easy to scrutinise.
- Future contributors can rebase or cherry-pick doc fixes from this
  PR without dragging in the prune and field-removal churn.

## Verification
- No code touched; no build or test run needed.
- The proposal doc was already linked from the frontend doc in a
  previous turn; this PR makes the link resolvable.
