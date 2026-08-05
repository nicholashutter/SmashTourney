# PR 3 — Prune: dead code, exceptions, validators, configs, deps

**Branch:** `pr/prune`
**Size:** 94 files deleted, ~22 modified, 2 reports added. ~1,850 source lines removed.
**Risk:** Medium. Mostly deletions, but the entity method removals touched
`IGameService`/`IUserManager` interfaces and the matching implementations and
test call sites. The frontend test consolidation drops test counts but not
coverage.

## What changes

### Deleted — server-side character data
- `tourneyAPI/Models/Entities/Characters/*.cs` — 77 files. The server-side
  character data was unused; the client loads its own parallel set via
  `import.meta.glob`. Test references (`new Mario()` etc.) replaced with
  `new Character()` in `ApiTests/GameRouterTest.cs`.
- `tourneyAPI/Utilities/Exceptions/*` — 14 files. All were unreferenced.
  Includes the mis-spelt `BracketGenerationException` (originally
  `BracketGenrationException`, no need to fix the typo since the class
  is dead).
- `tourneyAPI/Utilities/ModelValidators/GameValidator.cs` and
  `UserValidator.cs` — unreferenced.
- `ApiTests/AuthServiceTest.cs` — strict subset of `AuthEndToEndTest.cs`,
  every fact covered by the E2E suite.
- `tourneyClient/src/components/ui/dropdown-menu.tsx` and
  `tourneyClient/src/pages/GuestSignUp.tsx` — zero references.

### Modified — interfaces, services, configs
- `tourneyAPI/Models/Entities/Game.cs` — removed `GetVotes`, `SetVotes`,
  `GetCurrentRound`, `SetCurrentRound` and the `_currentVotes` /
  `_votesLock` / `_currentRoundLock` private fields. The bracket engine
  uses `BracketRuntimeState`, not these.
- `tourneyAPI/Services/Interfaces/IGameService.cs` and
  `IGameService.cs`/`IUserManager.cs` — removed `EndGame(Guid)` sync,
  `GetAllGamesAsync`, `LoadGameAsync`, `UpdateGameAsync`,
  `ReportMatchResultAsync` (kept the private `ReportMatchResultCoreAsync`
  that `SubmitMatchVoteAsync` uses internally), and
  `IUserManager.GetAllUsersAsync`. Verified zero route/test/background
  callers for each before deleting.
- `tourneyAPI/Models/Entities/Entities.csproj` — replaced with a
  deprecation stub. The project had no source files and was not
  referenced by the .sln or any other .csproj. Safe to physically
  `rm` from disk.
- `tourneyAPI/appsettings*.json` — removed `UseInMemoryDatabase` from all
  three files (never read by any C#).
- `tourneyAPI/tourneyAPI.csproj` — removed `Microsoft.Extensions.Logging
  9.0.5` (the project uses Serilog only).
- `package.json` and `package-lock.json` (root) — removed `motion` and
  `vite-plugin-eslint2` (the project uses `vite-plugin-eslint`).
- `tourneyClient/package.json` and `tourneyClient/package-lock.json` —
  removed `class-variance-authority`, `@radix-ui/react-dropdown-menu`
  (only used by the dead `dropdown-menu.tsx`).
- `tourneyClient/src/components/CelebrationOverlay.tsx` — stub (the
  visual celebration was removed in PR 2's UX follow-up).

### Test consolidation
- `ApiTests/AuthServiceTest.cs` — deleted (covered by E2E).
- `tourneyClient/tests/RequestService.test.ts` — 43 → 10 tests.
- `tourneyClient/tests/matchVoteFeedback.test.ts` — 22 → 8 tests.
- `tourneyClient/tests/PersistentConnection.test.ts` — 15 → 5 tests.
  7 of the originals just asserted the SignalR builder was *called*
  rather than that it worked; those were dropped.
- `ApiTests/GameServiceTest.cs` — tautology tests removed.
- `dotnet format` cleaned unused `using` directives and whitespace
  across 7 files.

### Notable audit discrepancies (verified, not all audit claims held)
- `lucide-react` is **not** dead — `PageShell.tsx` uses `Volume2` /
  `VolumeX` for the mute toggle. **Kept in `package.json`.**
- Character file count was 77, not 89 (the audit's count was off).
- `bracketStatesField` reflection test in `GameServiceTest.cs` is still
  valid after the service method removals. Kept.

### node_modules ripple
The dependency removals (`motion`, `vite-plugin-eslint2`, `class-variance-authority`,
`@radix-ui/react-dropdown-menu`) cause `npm install` to remove many
transitive packages. This is a large surface but mechanical; everything
that remains is a real dependency.

## Deferred to separate PRs (out of scope here)
- `Player.CurrentScore` / `CurrentRound` and the 5
  `ApplicationUser` write-only fields → PR 4 (coordinated with EF
  migrations).
- `tourneyAPI/Models/Entities/Entities.csproj` physical deletion
  (host doesn't allow `rm`; stub kept).

## Verification
- `dotnet build` — 0 warnings, 0 errors
- `dotnet test` — 172/172 backend tests pass
- `npm run build` — clean
- `npm test` — 123/123 frontend tests pass

## Reports included
- `PRUNABLE_AUDIT.md` — the original audit that drove this PR
- `PRUNE_EXECUTION.md` — the per-phase execution report
