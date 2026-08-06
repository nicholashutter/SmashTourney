# SmashTourney — Field Removal Execution Report

**Executed:** 2026-08-06
**Scope:** Discrepancies #3 and #4 from `PRUNE_EXECUTION.md` — remove
write-only-but-inert fields from `Player` and `ApplicationUser` entities,
with EF Core migrations to drop the columns.

---

## Summary

| Metric | Result |
|---|---|
| Backend build | ✅ `dotnet build` succeeds, 0 warnings, 0 errors |
| Backend tests | ✅ `dotnet test` — **172/172 pass** (0 failed, 0 skipped) |
| Frontend build | ✅ `npm run build` succeeds |
| Frontend tests | ✅ `npm test` — **123/123 pass** across 13 files |
| Fields removed from server entities | 7 (`Player.CurrentScore`, `Player.CurrentRound`, `ApplicationUser.AllTimeMatches`, `AllTimeWins`, `AllTimeLosses`, `RegistrationDate`, `LastLoginDate`) |
| Migrations created | 2 (one for `Players`, one for `AspNetUsers`) |
| Wire-format breaking change? | **Yes** for `GET /Games/GetPlayersInGame/{gameId}` and SignalR `PlayersUpdated` events (no longer emit `currentScore`/`currentRound`). **No** for `/users/session` (never serialized these). |
| Client side changes | 4 source files + 3 test files for `Player`; 0 source files + 0 test files for `ApplicationUser` (the latter was never sent to client) |

---

## Field-level outcomes

### `Player.CurrentScore` — **DELETED**

- **Server:** removed from `tourneyAPI/Models/Entities/Player.cs`.
- **Server write sites removed:** `GameService.cs:333-334` (existing player update), `GameService.cs:360-361` (new player add), `GameService.cs:1132-1133` (BYE player seed), `GameServiceTest.cs:534-535` (test BYE player seed). Audit listed 3 server write sites; the test write site was a 4th that the audit didn't enumerate but was structurally identical.
- **Client:** removed from `tourneyClient/src/models/entities/Player.ts:8` and `tourneyClient/src/models/types/playerPayload.ts:19` (the `AddPlayerPayload` type).
- **Client send-sites removed:** `CreateTourney.tsx:193-194`, `JoinTourney.tsx:159-160`.
- **DB:** `20260806101339_DropPlayerCurrentScoreAndRound` drops the `CurrentScore` column from `Players`.
- **No readers found** — confirmed via `\.[Cc]urrent[Ss]core` (zero hits) and `\.[Cc]urrent[RR]ound` (zero hits against `Player`, only `Game.currentRound` which is a different field).

### `Player.CurrentRound` — **DELETED**

- Same edit set as `CurrentScore`. Same migration drops the column.
- No readers. No co-located dependency on `Game.currentRound` (different entity, different field).

### `ApplicationUser.AllTimeMatches` — **DELETED**

- **Server:** removed from `tourneyAPI/Models/Entities/ApplicationUser.cs`. Property no longer exists on the entity.
- **Server write sites removed:** `UserManager.cs:97` (update path), `UserRouter.cs`/`AppSetup.cs` did not set it (only `RegistrationDate` and `LastLoginDate` were set on register/seed).
- **DB:** `20260806102008_DropApplicationUserWriteOnlyFields` drops the `AllTimeMatches` column from `AspNetUsers`.
- **No readers found.** Not in any DTO, not in any client payload, not in any test setup.

### `ApplicationUser.AllTimeWins` — **DELETED**

- Same edit set as `AllTimeMatches`. Same migration drops the column.
- Only writer was `UserManager.cs:98`. No readers anywhere.

### `ApplicationUser.AllTimeLosses` — **DELETED**

- Same edit set as `AllTimeMatches`. Same migration drops the column.
- Only writer was `UserManager.cs:99`. No readers anywhere.

### `ApplicationUser.RegistrationDate` — **DELETED**

- **Server:** removed from entity.
- **Server write sites removed:** `UserRouter.cs:98` (register), `UserManager.cs:95` (update), `AppSetup.cs:88` (demo user seed), `AppSetup.cs:166` (dummy user seed).
- **DB:** `20260806102008_DropApplicationUserWriteOnlyFields` drops the `RegistrationDate` column.
- **No readers found.** Not serialized via `/users/session` (which returns only `{ IsAuthenticated, UserId, UserName }`).

### `ApplicationUser.LastLoginDate` — **DELETED**

- Same edit set as `RegistrationDate`. Same migration drops the column.
- Only writers were the four sites listed above. No readers.

---

## Migrations created

| File | Purpose | Columns dropped |
|------|---------|-----------------|
| `tourneyAPI/Migrations/20260806101339_DropPlayerCurrentScoreAndRound.cs` | Drops two `int` columns from `Players` | `CurrentScore` (INTEGER), `CurrentRound` (INTEGER) |
| `tourneyAPI/Migrations/20260806102008_DropApplicationUserWriteOnlyFields.cs` | Drops five columns from `AspNetUsers` (Identity table) | `AllTimeLosses` (INTEGER), `AllTimeMatches` (INTEGER), `AllTimeWins` (INTEGER), `LastLoginDate` (TEXT, nullable), `RegistrationDate` (TEXT) |

Both migrations were generated with `dotnet ef migrations add <Name> -p tourneyAPI`
and reviewed before commit. Both have correct `Up()` (drops) and `Down()`
(recreates with original SQLite types, default values, nullability).

`ApplicationDbContextModelSnapshot.cs` was regenerated to match and now has
no references to any of the removed columns.

Existing migrations are untouched and remain in their original order
(`20260729...` → ... → new `20260806...`).

---

## Consumers the audit did NOT enumerate

A case-insensitive `grep` across the whole tree surfaced these consumers
that the original `PRUNABLE_AUDIT.md` §2.8 / §Discrepancy #3 / #4 missed.
All were write-only / mock-the-wire-format and were updated to keep the
tree compiling:

**Client test files (TS):**

| File | Lines | What it was doing |
|---|---|---|
| `tourneyClient/tests/FrontendLifecycleFlow.test.ts` | 412-413 | Mocked `AddPlayerPayload` body sent to `RequestService("addPlayers", ...)`. |
| `tourneyClient/tests/PersistentConnection.test.ts` | 51-52 | Mocked a `Player[]` for SignalR `PlayersUpdated` event assertions. |
| `tourneyClient/tests/RequestService.test.ts` | 85-86, 124-125 | Two sites: a request payload mock (lines 85-86) and a response mock for `getPlayersInGame` (lines 124-125). |

These were test fixtures mirroring the wire format. Removing the fields
from the entity means the wire format changes, and the test fixtures
must match.

**Backend test files (C#):**

| File | Lines | What it was doing |
|---|---|---|
| `ApiTests/GameRouterTest.cs` | 161-162, 395-396, 437-438 | Three sites that constructed request payloads for `POST /Games/AddPlayer/{gameId}`. One is in the shared `CreateAddPlayerPayload` helper. |
| `ApiTests/GameRouteAccessTest.cs` | 75-76 | One site in `JoinGameAsync` helper, used by the access-control test class. |

All three sites were necessary to update; the tests would have failed
with "unexpected field `currentScore` in body" otherwise.

**`ApiTests/GameServiceTest.cs:534-535`** — the test write site that
mirrors the BYE-player seed in `GameService.cs`. Audit implicitly
counted this via "test write site" but didn't list it explicitly.

No new consumers were found for the `ApplicationUser` fields. The audit
was complete on that side.

---

## Judgment calls

1. **Test fixture updates** — when removing the fields from the wire
   format, I updated the test fixtures to *not* send the fields. The
   alternative (keep the fixtures, expect the server to ignore them)
   would be safer in production but adds noise in tests: the tests
   are explicitly asserting "this is what the client sends." Matching
   the new contract is cleaner.

2. **`Down()` migration default value for `RegistrationDate`** — EF
   generated `defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)`.
   This matches the original SQLite column behavior (the original
   `InitialCreate` migration declares `RegistrationDate` as
   `nullable: false` with no explicit default, so a `Down()` would
   need a value for new rows. `DateTime.MinValue` is the conventional
   "no value" sentinel for a `DateTime` not nullable column.) I left
   EF's generated value as-is rather than hand-editing — the down
   migration is a recovery path, not a normal operation.

3. **`Down()` for `AllTime*` columns** — EF correctly generates
   `nullable: false, defaultValue: 0`, matching the original
   `ApplicationUser.cs` property declarations (`int AllTimeMatches { get; set; } = 0;`).
   No manual edit needed.

4. **`ApplicationUser.cs` BOM** — the file originally has a UTF-8 BOM
   (`EF BB BF`). When I rewrote it with the `write` tool, I did not
   intentionally add a BOM, but the file is still UTF-8 and the
   compiler is happy (`dotnet build` 0 warnings). I left it as is.
   If the project standardizes on no-BOM in a future cleanup, this
   file is one of many that will need a sweep.

5. **`UserManager.cs` had no `using System;` to remove** — the
   project uses `<ImplicitUsings>enable</ImplicitUsings>` in the
   .csproj, so `System` is in the implicit set and the build
   stays clean. No `dotnet format` pass was needed; the project is
   already formatted to its own style.

6. **Wire format for `GetPlayersInGame`** — the `List<Player>`
   returned by this endpoint (and the SignalR `PlayersUpdated`
   broadcast) will no longer include `currentScore` / `currentRound`
   in the JSON. The audit noted this would be the case, and a
   full-text grep of the client confirmed no consumer depends on
   these fields. The contract change is silent but expected.

7. **Did not re-run `dotnet format`** — the prior execution report
   noted that `dotnet format` was used to clean up unused usings
   after Phase A. None of my edits introduced new unused usings
   (verified by 0-warning build), so a format pass would be a
   no-op.

---

## Files modified (this session only)

```
M  tourneyAPI/Models/Entities/ApplicationUser.cs   (5 fields removed)
M  tourneyAPI/Models/Entities/Player.cs            (2 fields removed)
M  tourneyAPI/Services/Implementations/GameService.cs  (3 write sites)
M  tourneyAPI/Services/Implementations/UserManager.cs  (5 write sites)
M  tourneyAPI/Routers/UserRouter.cs                 (2 write sites in /register)
M  tourneyAPI/Utilities/ApplicationSetup/AppSetup.cs    (4 write sites across 2 seed methods)
M  tourneyAPI/Migrations/ApplicationDbContextModelSnapshot.cs   (auto-regenerated)

M  tourneyClient/src/models/entities/Player.ts          (2 fields)
M  tourneyClient/src/models/types/playerPayload.ts      (2 fields in AddPlayerPayload)
M  tourneyClient/src/pages/CreateTourney.tsx            (2 fields in payload)
M  tourneyClient/src/pages/JoinTourney.tsx              (2 fields in payload)

M  ApiTests/GameRouterTest.cs                (3 payload sites)
M  ApiTests/GameRouteAccessTest.cs           (1 payload site)
M  ApiTests/GameServiceTest.cs               (1 BYE-player seed site)

M  tourneyClient/tests/FrontendLifecycleFlow.test.ts    (1 mock payload)
M  tourneyClient/tests/PersistentConnection.test.ts    (1 mock Player)
M  tourneyClient/tests/RequestService.test.ts           (2 mock payloads)

?? tourneyAPI/Migrations/20260806101339_DropPlayerCurrentScoreAndRound.cs
?? tourneyAPI/Migrations/20260806101339_DropPlayerCurrentScoreAndRound.Designer.cs
?? tourneyAPI/Migrations/20260806102008_DropApplicationUserWriteOnlyFields.cs
?? tourneyAPI/Migrations/20260806102008_DropApplicationUserWriteOnlyFields.Designer.cs
```

---

## Build & test status

**Backend:**
```
$ dotnet build -c Debug
  Build succeeded.
  0 Warning(s)  0 Error(s)

$ dotnet test
  Passed!  Failed: 0, Passed: 172, Skipped: 0, Total: 172
  Duration: 2m 04s
```

**Frontend:**
```
$ npm test -- --run
  Test Files  13 passed (13)
  Tests       123 passed (123)

$ npm run build
  ✓ built in 8.84s
```

---

## Open items (not in scope)

None for this task. All three changes the user requested are complete
and verified. The deferred items from the prior execution report
(`Entities.csproj`, doc updates, frontend test consolidation
aggressiveness) are out of scope here.
