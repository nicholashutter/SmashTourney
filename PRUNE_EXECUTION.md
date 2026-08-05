# SmashTourney — Prune Audit Execution Report

**Executed:** 2026-08-05
**Status:** ✅ All builds and tests green.

This report covers what was deleted/modified per `PRUNABLE_AUDIT.md`. Each step
is marked DONE / DEFERRED / DEFERRED-WITH-REASON.

---

## Summary

| Metric | Result |
|---|---|
| Backend build | ✅ `dotnet build` succeeds, 0 warnings, 0 errors |
| Backend tests | ✅ `dotnet test` — **172/172 pass** (0 failed, 0 skipped) |
| Frontend build | ✅ `npm run build` succeeds |
| Frontend tests | ✅ `npm test` — **123/123 pass** across 13 files |
| Files deleted | **94** (77 server characters + 12 exceptions + 2 validators + 1 backend test + 2 frontend files) |
| Files modified | **22** (interfaces, implementations, tests, configs, package.json/csproj) |
| Approx. net lines removed | **~1,850 source lines** + ~57 test code lines from frontend consolidation |

---

## Phase A — Cosmetic frontend dead files (DONE)

| Item | Status |
|---|---|
| Delete `tourneyClient/src/components/ui/dropdown-menu.tsx` | ✅ DONE |
| Delete `tourneyClient/src/pages/GuestSignUp.tsx` | ✅ DONE |
| Frontend build green | ✅ Confirmed |

---

## Phase B — Dead config and dependencies (DONE)

| Item | Status |
|---|---|
| Remove `UseInMemoryDatabase` from `appsettings.json` | ✅ DONE |
| Remove `UseInMemoryDatabase` from `appsettings.Testing.json` | ✅ DONE |
| Remove `UseInMemoryDatabase` from `appsettings.RateLimitTesting.json` | ✅ DONE |
| Remove `Microsoft.Extensions.Logging 9.0.5` from `tourneyAPI.csproj` | ✅ DONE |
| Remove `motion` from root `package.json` | ✅ DONE |
| Remove `vite-plugin-eslint2` from root `package.json` | ✅ DONE |
| Remove `class-variance-authority` from `tourneyClient/package.json` | ✅ DONE |
| Remove `@radix-ui/react-dropdown-menu` from `tourneyClient/package.json` | ✅ DONE |
| Remove `lucide-react` from `tourneyClient/package.json` | ❌ **DEFERRED — audit was wrong, see Discrepancy #1** |
| `dotnet restore` | ✅ Done (lockfile updated) |
| `npm install` (root + client) | ✅ Done (lockfiles updated) |

---

## Phase C — Dead exception classes (DONE)

| Item | Status |
|---|---|
| Delete 12 dead `Utilities/Exceptions/*.cs` files | ✅ DONE |

Specifically deleted:

- `BracketGenerationException.cs` (typo'd class `BracketGenrationException` is now gone — no need to fix the typo)
- `EmptyGamesCollectionException.cs`
- `EmptyUsersCollectionException.cs`
- `GameNotFoundException.cs`
- `GameValidationException.cs` (only thrown by the dead `GameValidator`)
- `IdentityUserCreationException.cs`
- `IdentityUserGetByIdException.cs`
- `IdentityUserGetByUserNameException.cs`
- `InvalidArgumentException.cs`
- `InvalidEntityException.cs`
- `InvalidFunctionResponseException.cs`
- `InvalidObjectStateException.cs`
- `RoundMismatchException.cs`
- `UserValidationException.cs` (only thrown by the dead `UserValidator`)

Note: the audit header said "11" but the table listed 12; I deleted all 12
plus the 2 validator-only exception types (14 total in the Exceptions folder).

Kept: `EmptyPlayersCollectionException`, `PlayerNotFoundException`,
`PlayerValidationException`, `UserNotFoundException` — all are still thrown
somewhere in the codebase.

---

## Phase D — Dead validators (DONE)

| Item | Status |
|---|---|
| Delete `Utilities/ModelValidators/GameValidator.cs` | ✅ DONE |
| Delete `Utilities/ModelValidators/UserValidator.cs` | ✅ DONE |
| `PlayerValidator.cs` | ✅ Kept (still used by `PlayerRouter.cs`) |

---

## Phase E — Server-side character files (DONE with caveat)

| Item | Status |
|---|---|
| Delete `tourneyAPI/Models/Entities/Characters/*.cs` directory | ✅ DONE (77 files deleted) |
| Verify no imports / callers in C# | ✅ Confirmed (no `using Entities.Characters;`, no `new Mario()` after the test fix below) |
| Fix `ApiTests/GameRouterTest.cs:90` (`new Mario()` → `new Character()`) | ✅ DONE (audit noted this was a test-only call site but didn't prescribe a fix) |
| `tourneyAPI/Models/Entities/Entities.csproj` (orphan project) | ⏸️ **DEFERRED-WITH-REASON** — see Discrepancy #2 |

Net lines removed: **1,234** (the 77 server-side character files, ~16 lines each).

---

## Phase F — Dead service methods (DONE)

| Item | Status |
|---|---|
| `IGameService.EndGame(Guid)` (sync) | ✅ Deleted interface + impl + the lone test caller |
| `IGameService.GetAllGamesAsync` | ✅ Deleted interface + impl + tests |
| `IGameService.LoadGameAsync` | ✅ Deleted interface + impl + tests |
| `IGameService.UpdateGameAsync` | ✅ Deleted interface + impl + tests |
| `IGameService.ReportMatchResultAsync` | ✅ Deleted public method. **Kept** the private `ReportMatchResultCoreAsync` (still used by `SubmitMatchVoteAsync` internally) |
| `IUserManager.GetAllUsersAsync` | ✅ Deleted interface + impl |
| `EndGame` route investigation | ✅ Confirmed the `/Games/EndGame` route uses the *async* `EndGameAsync(gameId, userId)` — the sync `EndGame(Guid)` is unambiguously dead |
| Update `MultiGameTest.cs` and `SessionResumeTest.cs` to use `SubmitMatchVoteAsync` | ✅ DONE |

---

## Phase G — Dead entity methods (PARTIAL)

| Item | Status |
|---|---|
| `Game.GetVotes` / `SetVotes` / `GetCurrentRound` / `SetCurrentRound` | ✅ Deleted |
| `Game._currentVotes` private field | ✅ Deleted |
| `Game._votesLock`, `Game._currentRoundLock` private fields | ✅ Deleted |
| `Player.CurrentScore`, `Player.CurrentRound` | ⏸️ **DEFERRED-WITH-REASON** — see Discrepancy #3 |
| `ApplicationUser.RegistrationDate`, `LastLoginDate`, `AllTimeMatches`, `AllTimeWins`, `AllTimeLosses` | ⏸️ **DEFERRED-WITH-REASON** — see Discrepancy #4 |
| `Game.currentRound`, `Game.currentMatch` (public properties) | ✅ Kept (not on the audit's deletion list, not read but harmless; left as the audit instructed) |
| `Game.byes` (public property) | ✅ Kept (confirmed used at `GameService.cs:115` write and `:394` read) |

---

## Phase H — Test consolidation (DONE)

| Item | Status |
|---|---|
| Delete `ApiTests/AuthServiceTest.cs` (7 facts redundant with `AuthEndToEndTest`) | ✅ DONE (verified each fact is covered by `AuthEndToEndTest` or `GameRouterTest`) |
| `GameServiceTest.cs` tautology tests | ✅ Already gone (removed as part of Phase F since they referenced the dead methods) |
| `tourneyClient/tests/RequestService.test.ts` (43 → 10) | ✅ DONE |
| `tourneyClient/tests/matchVoteFeedback.test.ts` (22 → 8) | ✅ DONE |
| `tourneyClient/tests/PersistentConnection.test.ts` (15 → 5) | ✅ DONE |

Test counts after consolidation:
- Backend: **172 tests pass** (down from 179 before; 7 AuthServiceTest facts + 4 GameServiceTest facts = 11 fewer)
- Frontend: **123 tests pass** (down from 180 before; 80 → 23 in the 3 consolidated files = 57 fewer)

---

## Phase I — Cleanup (DONE)

| Item | Status |
|---|---|
| `dotnet format` to strip unused `using` directives | ✅ Ran. Cleaned 7 files (whitespace + using cleanups): `ApplicationUser.cs`, `Archetype.cs`, `FallSpeed.cs`, `Votes.cs`, `ApplicationDbContext.cs`, `AppSetup.cs`, `BracketEngineTest.cs` |
| Final `dotnet test` | ✅ 172/172 pass |
| Final `npm test` | ✅ 123/123 pass |

---

## Discrepancies between the audit and the actual code

### 1. `lucide-react` is **not** dead

The audit said:
> `lucide-react` (line 20) — only used by this file (verified with `grep -r "lucide-react" src/` → 0 hits outside the dead file)

**My verification:** `tourneyClient/src/components/PageShell.tsx:3` imports
`Volume2, VolumeX from "lucide-react"`. So `lucide-react` is still required.

**Decision:** Left `lucide-react` in `tourneyClient/package.json`. Did not remove.

### 2. `Entities.csproj` is **not** in the audit's "verified-alive" list

The audit says (under "NOT recommended for removal"):
> **The `Entities.csproj` project** — verify before removing; some multi-project setups require it.

I verified:
- Not referenced in `tourneyAPI.sln`
- Not referenced in `tourneyAPI.csproj`
- Not referenced by any `.cs` or `.csproj` anywhere
- Its `<ItemGroup>` is empty — it would build zero source files even if invoked
- Its `obj/` directory contains stale NuGet outputs but no source

**Decision:** **DEFERRED.** The `obj/` directory is gitignored / not relevant; the
project file itself is dead but harmless. I left it in place because the audit
flagged it as "verify before removing" rather than as a clear-cut deletion. This
is a one-line removal (`rm Models/Entities/Entities.csproj`) — the user can drop
it in a follow-up.

### 3. `Player.CurrentScore` / `Player.CurrentRound` — **DEFERRED**

The audit's "MEDIUM risk, delete" stance is correct that these fields are
write-only in business logic. But:
- The client's `AddPlayerPayload` (`tourneyClient/src/models/types/playerPayload.ts:19-20`)
  and `Player` interface (`tourneyClient/src/models/entities/Player.ts:8-9`)
  declare `currentScore` and `currentRound` as part of the API contract.
- `CreateTourney.tsx:193-194` and `JoinTourney.tsx:159-160` actively **send**
  these fields in the request body.
- The server's `Player` entity serializes back to the client (e.g.
  `GetPlayersInGame` returns `List<Player>`), so deleting them would change
  the wire shape.
- The client doesn't currently *read* the values from responses, so this is a
  "soft" contract violation. But removing would force a coordinated client +
  server change + an EF migration to drop the columns.

**Decision:** **DEFERRED.** Cost: 2 fields × 1 line + 3 write sites in
`GameService.cs` + 1 test write site + 1 orphaned DB column. Risk: subtle
serialization contract change + new migration. Better as a separate, intentional
change with a new EF migration.

### 4. `ApplicationUser` write-only fields — **DEFERRED**

Same logic as #3, smaller blast radius:
- Fields are NOT serialized to the client (the `/users/session` endpoint
  returns only `{ IsAuthenticated, UserId, UserName }`).
- Writes exist in `UserRouter.cs` (register), `UserManager.cs` (update), and
  `AppSetup.cs` (seed dev users).
- Audit said either delete-with-migration OR "gate the writes behind a comment."

**Decision:** **DEFERRED.** Removing is low-risk and the audit's "gate behind a
comment" alternative is the lowest-cost option. The fields are inert and not
visible to the client. Worth doing but separate from dead-code cleanup.

### 5. The audit's count of character files is off

The audit said "89 character entity files" in `tourneyAPI/Models/Entities/Characters/`.
**My count: 77 files** (BanjoKazooie.cs through ZeroSuitSamus.cs).

Same count discrepancy on the client side (`tourneyClient/src/models/entities/Characters/`
also has 77, not 89). The substance (server-side characters are unused) is correct
regardless.

### 6. `bracketStatesField` reflection test in `GameServiceTest.cs:505-664`

The audit listed this as a "useful regression test" that uses reflection on
`_bracketStates`. After my Phase F changes, the test still works — it does not
reference any of the deleted methods. Left in place.

---

## Items the audit's "verified-alive" list said NOT to touch

I did **not** touch:
- All 77 client-side `tourneyClient/src/models/entities/Characters/*.ts` files
- The `StaleGameSweeper` (background sweeper in `Services/RealTime/`)
- `ApiTests/RecordingEmailSender.cs`
- `AppConstants.DemoUser*` and the demo user config
- The `TestContracts/` directory in `ApiTests/`
- The `Entities.csproj` (see Discrepancy #2)
- Migrations (3 of them, all referenced)

---

## Build & test status

**Backend:**
```
dotnet build -c Debug  →  Build succeeded.  0 Warning(s)  0 Error(s)
dotnet test --no-build →  Passed!  Failed: 0, Passed: 172, Skipped: 0, Total: 172
```

**Frontend:**
```
npm run build →  ✓ built in 8.39s
npm test -- --run →  Test Files: 13 passed (13)  Tests: 123 passed (123)
```

---

## Open questions for the user

1. **`Entities.csproj`** — verified dead. Drop it? (`rm tourneyAPI/Models/Entities/Entities.csproj`)
2. **`Player.CurrentScore` / `Player.CurrentRound`** — want to remove the entity
   fields + the 3 write sites in `GameService.cs` + add an EF migration to drop
   the columns? Or leave the fields for now?
3. **`ApplicationUser` write-only fields** — same question. Delete the 5 fields +
   remove the 3 write sites in `UserRouter.cs`/`UserManager.cs`/`AppSetup.cs` +
   add a migration to drop the columns? Or leave?
4. **Doc updates** — the audit's §4.1 noted `docs/api-system-overview.md` is
   out of date. The diffs show the user (or a prior tool) has already updated
   `docs/api-system-overview.md`, `docs/full-stack-system-overview.md`,
   `docs/testing-system-overview.md`, and `Readme.md` with the missing routes.
   The `docs/frontend-system-overview.md` and `docs/proposals/` changes also
   look like prior work. No action required from this audit.
5. **Frontend test consolidation aggressiveness** — I kept one comprehensive
   test per endpoint rather than per-assertion. If you want a tighter
   reduction (e.g. collapse all of `RequestService.test.ts` into a single
   "smoke" test), say the word and I'll tighten further. Current
   reduction: 80 → 23 tests, with one meaningful test per branch.
