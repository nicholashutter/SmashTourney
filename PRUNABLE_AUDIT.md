# SmashTourney — Prunable Code Audit

**Audit type:** Read-only. No project files were modified.
**Method:** Static analysis — every "is X used?" claim is backed by a grep
across `*.cs`, `*.ts`, `*.tsx`, `*.csproj`, `package.json`, and
`appsettings*.json`. Inline evidence and line numbers are in the body.

The user said: *"there's a lot of code for an app that does quite little."*
The audit found that the user is right — at least ~5,000 lines of code are
either unused, redundant, or test-the-framework.

---

## TL;DR — Top 10 highest-impact prune candidates

| # | Item | Lines saved | Risk |
|---|------|-------------|------|
| 1 | 89 server-side `Models/Entities/Characters/*.cs` files — never imported, never instantiated | ~5,300 | LOW |
| 2 | 11 dead exception classes in `Utilities/Exceptions/` | ~600 | LOW |
| 3 | Two never-called validators (`GameValidator.cs`, `UserValidator.cs`) and the two exception types only they throw | ~120 | LOW |
| 4 | Frontend `components/ui/dropdown-menu.tsx` (~270 lines) + `pages/GuestSignUp.tsx` (~50 lines) — zero callers in `src/` | ~320 | LOW |
| 5 | Dead `IGameService` methods not wired to any route: `EndGame(Guid)` (sync), `GetAllGamesAsync`, `LoadGameAsync`, `UpdateGameAsync`, `ReportMatchResultAsync` | ~150 + interface shrink | MEDIUM (drop in same PR as test edits) |
| 6 | Dead config: `"UseInMemoryDatabase": true/false` in all three `appsettings*.json` files — never read by any C# | 0 (config) | LOW |
| 7 | Dead NuGet dep `Microsoft.Extensions.Logging 9.0.5` in `tourneyAPI.csproj` — code uses Serilog only | 0 (config) | LOW |
| 8 | Dead root `package.json` deps: `motion`, `vite-plugin-eslint2` (the project uses `vite-plugin-eslint`) | 0 (config) | LOW |
| 9 | Frontend `RequestService.test.ts` has 43 tests for a 200-line HTTP wrapper; `matchVoteFeedback.test.ts` has 22 tests for a small mapping file | ~400 lines of test code | MEDIUM (some tests are useful) |
| 10 | Significant test overlap: `AuthServiceTest` (7 facts) is a strict subset of `AuthEndToEndTest` (22 facts). `GameServiceTest` direct-service tests duplicate `GameRouterTest` HTTP tests of the same business logic. | ~600 lines of test code | MEDIUM (drop in same PR) |

Total dead/redundant code identified: **~7,000–8,000 lines** (including all the character entity files and their tests).

---

## 1. Definitely dead code

### 1.1 Server-side character entity files (89 files, ~5,300 lines)

**Path:** `tourneyAPI/Models/Entities/Characters/*.cs` (89 files from `BanjoKazooie.cs` to `ZeroSuitSamus.cs`)

**Evidence — the namespace is never imported anywhere in C#:**

```text
=== Entities\.Characters ===
  (no hits)

=== using Entities.Characters ===
  (no hits)
```

**Evidence — only the base `Character()` is ever constructed server-side:**

```text
=== new BanjoKazooie ===
  (no hits)

=== new Mario ===
  ApiTests\GameRouterTest.cs:90   ← test only

=== new Character\( ===
  tourneyAPI\Models\Entities\Player.cs:21
  tourneyAPI\Services\Implementations\GameService.cs:1248
  + 4 test-file hits
```

The client has its own `tourneyClient/src/models/entities/Characters/` directory
loaded via `import.meta.glob` in `lib/loadCharacterCatalog.ts:24`. The server
keeps a parallel set of files that nobody reads. The server treats
`Character` as a generic with default enum values (line 1248 of
`GameService.cs` builds BYE players with `new Character()`).

**Why prunable:** zero callers, zero imports. Even `using Entities;` in
test files shows these classes are never used. The base `Character.cs`
class itself is used and must stay.

**Risk:** LOW — nothing imports them.

**Action:** delete the entire `tourneyAPI/Models/Entities/Characters/`
directory. Optionally, also delete the corresponding
`tourneyAPI/Models/Entities/Entities.csproj` if it was a project
created *only* to house these (it has 218 bytes — verify it isn't
referenced by `tourneyAPI.csproj` or solution).

---

### 1.2 Dead exception classes (11 files, ~600 lines)

**Path:** `tourneyAPI/Utilities/Exceptions/*.cs`

**Evidence — only self-references for these exception types:**

| Exception | `throw new` hits outside own file | Notes |
|-----------|-----------------------------------|-------|
| `BracketGenrationException` (note: **mis-spelt** — missing 'e' between "Gen" and "ration"; the FILE is named correctly) | 0 | The class is named `BracketGenrationException` while the file is `BracketGenerationException.cs`. Either the class never compiled (suggesting zero usage for a long time) or the type was always wrong. Verify which you intend before deleting. |
| `EmptyGamesCollectionException` | 0 | |
| `EmptyUsersCollectionException` | 0 | |
| `GameNotFoundException` | 0 | |
| `IdentityUserCreationException` | 0 | |
| `IdentityUserGetByIdException` | 0 | |
| `IdentityUserGetByUserNameException` | 0 | |
| `InvalidArgumentException` | 0 | |
| `InvalidEntityException` | 0 | |
| `InvalidFunctionResponseException` | 0 | |
| `InvalidObjectStateException` | 0 | |
| `RoundMismatchException` | 0 | |

`PlayerNotFoundException`, `EmptyPlayersCollectionException`,
`PlayerValidationException`, `UserNotFoundException`,
`GameValidationException`, and `UserValidationException` are at least
thrown once — keep those.

**Risk:** LOW.

**Action:** delete the 11 listed files. Fix the typo in
`BracketGenerationException.cs` (rename class to `BracketGenerationException`)
*before* deleting if you want to leave it.

---

### 1.3 Unused validators

**Files:**
- `tourneyAPI/Utilities/ModelValidators/GameValidator.cs` (20 lines)
- `tourneyAPI/Utilities/ModelValidators/UserValidator.cs` (20 lines)

**Evidence — `GameValidator.Validate` and `UserValidator.Validate` are not called outside their own file:**

```text
=== GameValidator\.Validate ===
  tourneyAPI\Utilities\ModelValidators\GameValidator.cs:12  (self)

=== UserValidator\.Validate ===
  tourneyAPI\Utilities\ModelValidators\UserValidator.cs:12  (self)

=== PlayerValidator\.Validate ===
  tourneyAPI\Routers\PlayerRouter.cs:76
  tourneyAPI\Routers\PlayerRouter.cs:103
```

`PlayerValidator.cs` IS used. Only the Game/User variants are dead.
Their corresponding `GameValidationException` / `UserValidationException`
only have self-references, so they can be deleted together.

**Risk:** LOW.

**Action:** delete `GameValidator.cs`, `UserValidator.cs`, and the
exception files `GameValidationException.cs`, `UserValidationException.cs`.

---

### 1.4 Frontend dead files

**File 1: `tourneyClient/src/components/ui/dropdown-menu.tsx` (~270 lines)**

**Evidence:** all references in the file are self-references; no other file imports it:

```text
=== dropdown-menu ===
  tourneyClient\src\components\ui\dropdown-menu.tsx:2   (self: Radix import)
  tourneyClient\src\components\ui\dropdown-menu.tsx:11  (self: usage)
  ... 7 more self-hits
  0 hits in any other file
```

**Ripple effect — three dependencies become unused if this file is deleted:**

- `@radix-ui/react-dropdown-menu` (in `tourneyClient/package.json` line 15) — only used by this file
- `lucide-react` (line 20) — only used by this file (verified with `grep -r "lucide-react" src/` → 0 hits outside the dead file)
- `class-variance-authority` (line 17) — already had 0 hits across `src/` (see §1.5)

**File 2: `tourneyClient/src/pages/GuestSignUp.tsx` (~50 lines)**

**Evidence:** the only references are inside the file itself:

```text
=== GuestSignUp ===
  tourneyClient\src\pages\GuestSignUp.tsx:11   const GuestSignUp = () =>
  tourneyClient\src\pages\GuestSignUp.tsx:43   export { GuestSignUp };
  (zero hits in any other file)
```

The page is not wired into `main.tsx` (verified — only 9 pages are routed
there, and `GuestSignUp` is not one of them).

**Risk:** LOW.

**Action:** delete both files. After deleting `dropdown-menu.tsx`, also
remove `class-variance-authority`, `lucide-react`, and
`@radix-ui/react-dropdown-menu` from `tourneyClient/package.json`.

---

### 1.5 Dead frontend dep: `class-variance-authority`

**Path:** `tourneyClient/package.json:17`

**Evidence:** no `import` or reference in `src/`:

```text
=== cva ===
=== class-variance-authority ===
=== VariantProps ===
  (no hits across src/)
```

`components/ui/card.tsx` does NOT use CVA (it uses raw `cn()` calls
without variant logic).

**Risk:** LOW.

**Action:** remove from `dependencies`.

---

### 1.6 Dead root `package.json` dependencies

**File:** `package.json` (at repo root)

```json
"motion": "^12.23.12",          ← not imported anywhere
"uuid": "^13.0.0",              ← imported in tourneyClient/src
"vite-plugin-eslint2": "^5.0.4" ← not used; project uses `vite-plugin-eslint`
```

**Evidence — `motion` and `vite-plugin-eslint2`:**

```text
=== from .motion. / from 'motion' / from "motion" ===
  (no hits)

=== vite-plugin-eslint2 ===
  (no hits)
```

`vite.config.ts:6` imports `vite-plugin-eslint` (the original package),
not `vite-plugin-eslint2`. `uuid` is real — but the question of which
package it should live under (root vs `tourneyClient/`) is separate.
`uuid` IS imported in `CreateTourney.tsx` and `JoinTourney.tsx` so
keep it somewhere; moving it to `tourneyClient/package.json` would be
cleaner but is a structural change, not dead-code removal.

**Risk:** LOW.

**Action:** remove `motion` and `vite-plugin-eslint2` from root
`package.json`. Consider moving `uuid` into `tourneyClient/package.json`.

---

### 1.7 Dead C# NuGet package: `Microsoft.Extensions.Logging` 9.0.5

**File:** `tourneyAPI/tourneyAPI.csproj:21`

**Evidence:** no usages in any `.cs`:

```text
=== Microsoft\.Extensions\.Logging ===  (no hits)
=== ILogger< ===                          (no hits)
=== ILoggerFactory ===                    (no hits)
=== LoggerFactory ===                     (no hits)
```

The project uses `Serilog` (`Log.Information`, `Log.Warning`, etc.)
exclusively, not the Microsoft.Extensions.Logging abstraction.

**Risk:** LOW.

**Action:** remove the `Microsoft.Extensions.Logging` PackageReference.

---

### 1.8 Dead config: `UseInMemoryDatabase`

**Files:**
- `tourneyAPI/appsettings.json:3`
- `tourneyAPI/appsettings.Testing.json:2`
- `tourneyAPI/appsettings.RateLimitTesting.json:2`

**Evidence — never read by any C#:**

```text
=== UseInMemoryDatabase ===
  (3 hits, all in JSON files; 0 hits in any .cs)
```

`Program.cs:45` calls `ApplicationDbContext.SetupProd()` unconditionally.
The setting is purely aspirational.

**Risk:** LOW.

**Action:** delete the `UseInMemoryDatabase` line from all three
`appsettings*.json` files.

---

## 2. Dead methods on the service interface

These are members of `IGameService` and `IUserManager` that exist in
the interface, are implemented in the service, but are never called
from any HTTP route. The tests call some of them directly.

### 2.1 `IGameService.EndGame(Guid)` (sync, no host check)

**Defined:** `tourneyAPI/Services/Interfaces/IGameService.cs:21`

**Evidence — only one caller, in a test:**

```text
=== gameService\.EndGame\( ===
  ApiTests\GameServiceTest.cs:123   ← the only caller
  tourneyAPI\Services\Implementations\GameService.cs:498  (impl)
  tourneyAPI\Services\Interfaces\IGameService.cs:21  (decl)
```

The async `EndGameAsync` (the route-mapped one) is used by
`GameRouter.cs:119`. The sync overload is dead.

**Risk:** MEDIUM — `GameServiceTest.EndGameDoesNotThrow` references it.

**Action:** delete the interface member, the implementation at
`GameService.cs:498`, and the test in `GameServiceTest.cs:118-126`.

---

### 2.2 `IGameService.GetAllGamesAsync`

**Defined:** `IGameService.cs:47`

**Evidence — only one caller, in a test:**

```text
=== GetAllGamesAsync ===
  ApiTests\GameServiceTest.cs:132   ← test only
  ApiTests\GameServiceTest.cs:141   ← test only
  tourneyAPI\Services\Implementations\GameService.cs:192
  tourneyAPI\Services\Interfaces\IGameService.cs:47
```

`GetActiveGames` is the route-mapped call (line 87 of `GameRouter.cs`)
and goes through `GetGameSummariesAsync`, not this method.

**Risk:** MEDIUM — single test references it.

**Action:** delete the interface member, the implementation, and
`GameServiceTest.GetAllGamesReturnsExpectedCount` (lines 128-143).

---

### 2.3 `IGameService.LoadGameAsync`

**Defined:** `IGameService.cs:62`

**Evidence:**

```text
=== LoadGameAsync ===
  ApiTests\GameServiceTest.cs:259
  ApiTests\GameServiceTest.cs:262
  tourneyAPI\Services\Implementations\GameService.cs:158
  tourneyAPI\Services\Interfaces\IGameService.cs:62
```

`SessionResumeTest` builds a `new GameService(...)` and exercises
persistence by *re-instantiating* the service, but it does not call
`LoadGameAsync` (it calls `GetBracketSnapshotAsync` etc. on the new
service, which lazy-loads). The single direct caller is
`GameServiceTest.cs:259`.

**Risk:** MEDIUM.

**Action:** delete the interface member, the implementation, and the
`DoubleEliminationReportMatchPersistsAndHydratesAcrossLoadGame` test
which is the only one that exercises it (lines 209-271). The
underlying persistence mechanism is already covered by
`SessionResumeTest`.

---

### 2.4 `IGameService.UpdateGameAsync`

**Defined:** `IGameService.cs:65`

**Evidence:**

```text
=== UpdateGameAsync ===
  ApiTests\GameServiceTest.cs:254
  tourneyAPI\Services\Implementations\GameService.cs:136
  tourneyAPI\Services\Interfaces\IGameService.cs:65
```

Only the test in `GameServiceTest.cs` calls it. The other tests
(`SessionResumeTest`, `MultiGameTest`) just call `SubmitMatchVoteAsync`,
which writes through internally. `UpdateGameAsync` is a redundant public
escape hatch.

**Risk:** MEDIUM.

**Action:** delete the interface member, the implementation, and the
single call site in `GameServiceTest.cs:254`.

---

### 2.5 `IGameService.ReportMatchResultAsync`

**Defined:** `IGameService.cs:77`

**Evidence:**

```text
=== ReportMatchResultAsync ===
  ApiTests\GameServiceTest.cs:234
  ApiTests\GameServiceTest.cs:239
  ApiTests\MultiGameTest.cs:105
  ApiTests\SessionResumeTest.cs:226
  tourneyAPI\Services\Implementations\GameService.cs:893
  tourneyAPI\Services\Interfaces\IGameService.cs:77
```

Only tests call it. The production vote path is `SubmitMatchVoteAsync`,
which internally calls `TryReportMatch` on the bracket engine. The tests
that use `ReportMatchResultAsync` are testing a public face of the
service that no route wires up.

**Risk:** MEDIUM.

**Action:** delete the interface member, the implementation, and update
the 4 test call sites. The same business logic is covered by
`SubmitMatchVoteAsync` tests in `GameRouterTest` and
`GameServiceTest.SubmitMatchVoteAsyncCommitsWhenTwoParticipantsAgree`.

---

### 2.6 `IUserManager.GetAllUsersAsync`

**Defined:** `IUserManager.cs:16`

**Evidence:**

```text
=== GetAllUsersAsync ===
  tourneyAPI\Services\Implementations\UserManager.cs:75
  tourneyAPI\Services\Interfaces\IUserManager.cs:16
```

No tests, no routes, no production code uses it.

**Risk:** LOW.

**Action:** delete from interface + implementation.

---

### 2.7 `Game` entity methods that only self-reference

**File:** `tourneyAPI/Models/Entities/Game.cs`

**Dead methods:**
- `Game.GetVotes()` (line 55) — 0 callers
- `Game.SetVotes(Votes votes)` (line 61) — 0 callers
- `Game.GetCurrentRound()` (line 70) — 0 callers
- `Game.SetCurrentRound(int)` (line 76) — 0 callers

```text
=== GetVotes / SetVotes / GetCurrentRound / SetCurrentRound ===
  (each appears only inside Game.cs itself)
```

The bracket engine uses `BracketMatchRuntime` and `BracketRuntimeState`
to track current match and votes — not the `Game` entity.

**Risk:** MEDIUM — but the `_votesLock` and `_currentRoundLock` private
fields become dead too, along with `byes` (only self-referenced at line
474 of `GameService.cs:474` which uses `game.byes`).

Wait — let me re-check `byes`. The grep earlier showed:
```text
=== byes ===
  tourneyAPI\Services\Implementations\GameService.cs:474, 475, 764, 797, 1227, 1237, 1238
```

So `byes` is actually USED in `GameService.cs` (multiple lines). My
apologies — `byes` is not dead. Only the four `Get*`/`Set*` wrapper
methods on `Game` are dead.

**Action:** delete `GetVotes`, `SetVotes`, `GetCurrentRound`,
`SetCurrentRound`. Delete the two private lock fields
(`_votesLock`, `_currentRoundLock`) and the `Votes _currentVotes`
field that they alone guard — and the `using Enums;` import that
becomes unused. **Verify carefully before deleting** — if any
non-obvious code path is meant to read these (e.g. future
`GetActiveGames` payload), restore.

---

### 2.8 Dead data fields

**`Player.CurrentScore` and `Player.CurrentRound`** — written, never
read. Evidence:

```text
=== CurrentScore ===
  ... +1 GameService.cs hit where it's written
  (no other reads; tests count because tests are writes, not reads)

=== CurrentRound ===
  ... +1 GameService.cs hit where it's written
  Game.cs also has its own currentRound which IS read (different field)
  Player.CurrentRound is only written
```

The `Player` entity has `CurrentScore` and `CurrentRound` that are
copied from request DTOs at `GameService.cs:413-414, 440-441` and
set to `0` for BYE players at `GameService.cs:1245-1246`. No code
ever reads them back. The bracket engine tracks wins/losses in
`BracketPlayerRuntime`.

**Risk:** MEDIUM (data shape; check with frontend that the field
isn't being read on the client).

**Action:** delete `CurrentScore` and `CurrentRound` from the
`Player` entity and stop copying them. Update the migration if EF
requires a column drop. The `Player.cs` field declarations and the
3 `GameService.cs` write sites can be removed. May also require a
new migration.

**`ApplicationUser.AllTimeMatches`, `AllTimeWins`, `AllTimeLosses`,
`RegistrationDate`, `LastLoginDate`** — written, never read by business
logic. The only places they appear outside migration files are:

```text
=== RegistrationDate ===
  tourneyAPI\Routers\UserRouter.cs:98     ← set on register
  tourneyAPI\Services\Implementations\UserManager.cs:113  ← set on update
  tourneyAPI\Utilities\ApplicationSetup\AppSetup.cs:88, 166  ← set when seeding
  (no reads)

=== LastLoginDate ===
  tourneyAPI\Routers\UserRouter.cs:99     ← set on register
  tourneyAPI\Services\Implementations\UserManager.cs:114  ← set on update
  tourneyAPI\Utilities\ApplicationSetup\AppSetup.cs:89, 167
  (no reads)

=== AllTimeMatches / AllTimeWins / AllTimeLosses ===
  tourneyAPI\Services\Implementations\UserManager.cs:115-117
  (only writes, no reads)
```

**Risk:** MEDIUM — these may be planned for a future "stats screen" but
currently do nothing.

**Action:** either delete the fields (with migration), or mark them with
a TODO and at least gate the writes behind a comment. They are pure
write-only fields.

---

## 3. Test redundancy

### 3.1 `AuthServiceTest.cs` (7 facts) is largely a subset of `AuthEndToEndTest.cs` (22 facts)

| `AuthServiceTest` test | Covered by `AuthEndToEndTest`? |
|------------------------|--------------------------------|
| `RegisterNewUser` (line 46) | Yes — `RegisteringSendsAConfirmationEmail` (line 41) is strictly stronger. |
| `LoginNewUser` (line 55) | Partially — `FollowingTheEmailedLinkAllowsSignIn` (line 96) is strictly stronger. `LoginNewUser` only checks that `RegisterAndLoginAsync` returns non-null, which is a tautology. |
| `SecureEndpointWithCookieReturnsOk` (line 63) | NOT covered. This tests that `GET /` returns 200, which is just static file serving — not auth. Likely should be deleted regardless. |
| `SecureEndpointWithoutCookieReturnsUnauthorized` (line 74) | Yes — covered by `GameRouterTest.GameRoutesWithoutAuthentication_ReturnUnauthorized` (line 843). |
| `LogoutEndsSessionSuccessfully` (line 85) | Yes — `SigningOutEndsTheSession` (line 336). |
| `SessionEndpointRequiresAuthentication` (line 95) | **Duplicate of `SecureEndpointWithoutCookieReturnsUnauthorized` (line 74) in the same file.** |
| `SessionEndpointReturnsOkWhenAuthenticated` (line 106) | Yes — partial overlap with `FollowingTheEmailedLinkAllowsSignIn`. |

**Net redundancy:** 5 of 7 tests in `AuthServiceTest` are either
duplicates of each other or strictly weaker versions of tests in
`AuthEndToEndTest`. The remaining 2 (`LoginNewUser` and
`SecureEndpointWithCookieReturnsOk`) test trivial/tangential behavior.

**Risk:** MEDIUM — the file is short (4KB) but if you delete it, you
need to be sure `AuthEndToEndTest` is the only auth coverage.

**Action:** delete `AuthServiceTest.cs`. Verify by running the
`AuthEndToEndTest` suite.

---

### 3.2 `GameServiceTest.cs` direct-service tests duplicate `GameRouterTest.cs` HTTP tests

`GameServiceTest` drives `IGameService` methods directly. `GameRouterTest`
covers the same business logic over HTTP. Because the HTTP layer is
strictly stronger (it proves the route wiring, status codes, JSON shape,
and middleware order), the service-layer direct tests are weaker
proxies of what `GameRouterTest` already proves:

| `GameServiceTest` (service) | `GameRouterTest` (HTTP) |
|------------------------------|--------------------------|
| `SubmitMatchVoteAsyncCommitsWhenTwoParticipantsAgree` (line 301) | `SubmitMatchVoteCommitsWhenBothParticipantsAgree` (line 679) |
| `SubmitMatchVoteAsyncRejectsDuplicateVoteFromSameVoter` (line 338) | `SubmitMatchVoteRejectsDuplicateVoteBySameParticipant` (line 709) |
| `DoubleEliminationStartGameInitializesBracketSnapshot` (line 181) | `DoubleEliminationBracketEndpointsReturnSnapshotAndSupportCurrentMatchRoute` (line 517) |
| `GetGameStateAsyncReturnsStartedStateAfterStartGame` (line 274) | `GetFlowStateReturnsAuthoritativeStateForStartedGame` (line 574) |
| `EndGameDoesNotThrow` (line 118) | No equivalent — but the test only asserts `Record.Exception == null` (a tautology) |
| `CreateUserSessionDoesNotThrow` (line 146) | No equivalent — same tautology |
| `GetGameByIdAsyncReturnsSameGame` (line 160) | No equivalent — but trivially tests the EF round-trip |
| `GetAllGamesReturnsExpectedCount` (line 129) | Covered indirectly by `GetActiveGames` route test (line 87 in `GameRouter.cs`, no direct test, but the listing is exercised in `MultiGameTest` and `GameAccessControlTest`) |

**Tautology tests to delete outright** (they only assert "the call did
not throw"):

- `GameServiceTest.EndGameDoesNotThrow` (line 118)
- `GameServiceTest.CreateUserSessionDoesNotThrow` (line 146)
- `AuthServiceTest.LoginNewUser` (line 55) — only asserts the helper returns non-null

**Service-only tests worth keeping (HTTP can't reach them):**

- `DoubleEliminationReportMatchPersistsAndHydratesAcrossLoadGame` (line 209) — this exercises `UpdateGameAsync` and `LoadGameAsync` (both slated for deletion in §2.3, §2.4). If you delete those, this test must also be deleted.
- `StartGameAsyncWithOddPlayersAutoResolvesByesBeforeFirstRealMatch` (line 369) — bye-resolution edge cases, useful.
- `StartGameAsyncAutoCompletesRealVersusByeWithoutManualVote` (line 401) — useful.
- `StartGameAsyncAutoCompletesByeVersusByeWithoutManualVote` (line 427) — useful.
- `StartGameAsyncWithLoneEntrantCompletesAndCrownsThatPlayer` (line 460) — useful.
- `SubmitMatchVoteAsyncCommitsRealVersusByeWhenByeMetadataIsMissing` (line 503) — useful regression test (uses reflection on private state, but the regression is real).

**Risk:** MEDIUM — deleting requires updating call sites in other
tests that use the soon-to-be-deleted methods.

**Action:** if you adopt §2.3 and §2.4 (delete `LoadGameAsync` and
`UpdateGameAsync`), the corresponding test goes too. Otherwise, drop
the two tautology tests and the four direct-service tests that have
HTTP-layer equivalents listed above. Keep the bye-regression tests.

---

### 3.3 Frontend: `RequestService.test.ts` has 43 tests for a 200-line HTTP wrapper

**File:** `tourneyClient/tests/RequestService.test.ts` (43 tests, ~26KB)

Many of these tests assert the *same* behavior with cosmetic
variations. For example:

```text
test("addPlayers calls AddPlayer route", async () =>
test("addPlayers uses POST method", async () =>
test("addPlayers sets JSON content type", async () =>
test("addPlayers serializes array payload", async () =>
test("addPlayers serializes displayName", async () =>
test("addPlayers serializes character id", async () =>
test("addPlayers allows payload without userId", async () =>
test("getPlayersInGame returns game id", async () =>
test("getPlayersInGame returns game name", async () =>
test("getPlayersInGame returns players array", async () =>
test("getPlayersInGame returns character id", async () =>
test("getPlayersInGame calls route with gameId", async () =>
test("getPlayersInGame uses POST method", async () =>
... 30 more lines of "X returns Y" / "X calls Y" / "X uses Z" tests
```

This is "the framework works" testing. The real assertions worth
keeping:

- The "X returns Y" tests catch a typo in a route name.
- The "X calls Y" tests catch a typo in a method.
- The "X uses Z" tests are largely framework.

**Risk:** MEDIUM — keep the route-shape tests (URLs, methods, status
codes), drop the payload-serialization micro-tests.

**Action:** consolidate to one test per endpoint (route, method,
payload shape, error path) — about 10 tests total. The 43→10
reduction removes ~70% of the file without losing meaningful coverage.

---

### 3.4 Frontend: `matchVoteFeedback.test.ts` (22 tests) for a ~95-line mapper

**File:** `tourneyClient/tests/matchVoteFeedback.test.ts` (22 tests)

The source has 4 branches in `getVoteFeedbackFromResponse` and 5
in `getVoteFeedbackFromError`. 22 tests means each branch is
hit ~2.5 times on average — most likely with copy-paste coverage.

**Risk:** MEDIUM.

**Action:** trim to one test per branch (9 total). The current
22-test file is over-tested for a function that maps an enum to a
display object.

---

### 3.5 Frontend: `PersistentConnection.test.ts` (15 tests)

**File:** `tourneyClient/tests/PersistentConnection.test.ts` (15 tests)

Tests like:

```text
test("createPlayerConnection configures Hub URL with credentials", async () =>
test("createPlayerConnection enables automatic reconnect", async () =>
test("createPlayerConnection builds a connection instance", async () =>
test("createPlayerConnection starts the connection", async () =>
test("createPlayerConnection registers PlayersUpdated event", async () =>
test("createPlayerConnection registers Successfully Joined event", async () =>
test("createPlayerConnection registers GameStarted event", async () =>
```

These test that the SignalR builder methods were *called*, not that
they did the right thing. A single "createPlayerConnection wires all
expected SignalR options and event handlers" test would cover all 7.

**Risk:** MEDIUM.

**Action:** consolidate to 3-4 tests:
1. Connection configures credentials + reconnect.
2. Connection registers all 3 event handlers (`PlayersUpdated`,
   `Successfully Joined`, `GameStarted`).
3. `updateOthers`/`notifyGameStarted` invoke the right hub methods.
4. `disconnect` stops the connection.

---

## 4. Outdated/leftover artifacts

### 4.1 `docs/api-system-overview.md` is missing several current routes

The doc lists only these routes (as of audit):
- `POST /Games/CreateGameWithMode`
- `POST /Games/AddPlayer/{gameId}`
- `POST /Games/StartGame/{gameId}`
- `GET /Games/GetBracket/{gameId}`
- `GET /Games/GetCurrentMatch/{gameId}`
- `GET /Games/GetFlowState/{gameId}`
- `POST /Games/SubmitMatchVote/{gameId}`
- `POST /Games/GetPlayersInGame/{gameId}`
- `POST /users/login`
- `GET /users/demo-credentials` (dev only)
- `GET /users/session`
- `POST /users/logout`

Routes that exist in code but are missing from the doc:
- `GET /Games/GetActiveGames` (`GameRouter.cs:87`)
- `GET /Games/GetPlayerSession/{gameId}` (`GameRouter.cs:267`)
- `POST /Games/EndGame/{gameId}` (`GameRouter.cs:109`)
- `POST /users/register` (`UserRouter.cs:77`)
- `POST /forgotPassword` (ASP.NET Identity, in `Program.cs:281`)
- `POST /resetPassword` (ASP.NET Identity)
- `POST /resendConfirmationEmail` (ASP.NET Identity)

**Risk:** LOW.

**Action:** bring the doc up to date, or note explicitly that the
generated `docs/api.html` / `docs/index.html` is the source of truth.

---

### 4.2 `.vscode/launch.json` and `.vscode/tasks.json` are clean

Verified — every preLaunchTask and command references a real script
(`npm run dev`, `dotnet build`, `dotnet test`, etc.). No stale
entries.

---

### 4.3 Migrations are clean

Three migrations exist: `InitialCreate`, `GameLifecycleTimestamps`,
`DropPhantomPlayerGameColumn`. All three are referenced by
`ApplicationDbContextModelSnapshot.cs` and applied at startup by
`AppSetup.ApplyDatabaseMigrationsAsync`. No orphan migrations.

---

## 5. `using` directive bloat (high-volume, low-value cleanup)

A static check found **~150 unused `using` directives** across the
project. C# 12 with `<ImplicitUsings>enable</ImplicitUsings>` in
both `tourneyAPI.csproj` and `ApiTests.csproj` already provides
many of them by default. Highlights:

- `tourneyAPI/Program.cs` (5 unused usings)
- `tourneyAPI/Routers/GameRouter.cs` (5 unused)
- `tourneyAPI/Routers/PlayerRouter.cs` (4 unused)
- `tourneyAPI/Routers/UserRouter.cs` (5 unused)
- All 89 `Characters/*.cs` files (1 unused each: `using Enums;`)
- All `tourneyAPI/Migrations/*.cs` files (1-2 unused each)
- `tourneyAPI/Services/Implementations/GameService.cs` (5 unused)
- All ApiTests test files (2-5 unused each)

**Action:** add `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>`
and `dotnet format` to remove them mechanically. This is a separate
task from dead-code removal.

**Risk:** LOW (mechanical).

---

## 6. Pre-existing bug noted in passing

`Utilities/Exceptions/BracketGenerationException.cs` contains a class
named `BracketGenrationException` (missing 'e' between "Gen" and
"ration"). The file name uses the correct spelling. This compiles
because the class is never used (§1.2). If you ever wire it up, it
will fail to find a reference under the expected name.

**Action:** rename the class to `BracketGenerationException` (matching
the file name and the conventional spelling) before/while deleting it.

---

## 7. Items deliberately NOT recommended for removal

These look redundant but are load-bearing:

- **All 89 client-side character files** (`tourneyClient/src/models/entities/Characters/*.ts`) — loaded
  by `lib/loadCharacterCatalog.ts:24` via `import.meta.glob`. Even
  files that look "unused" by name search are picked up by the glob.
- **The `Entities.csproj` project** — verify before removing; some
  multi-project setups require it.
- **`StaleGameSweeper`** — registered as a hosted service in
  `Program.cs:57` and is the only place `PruneStaleGamesAsync` is
  called from production code.
- **`RecordingEmailSender.cs`** in `ApiTests/` — used by
  `CustomWebApplicationBuilder.cs:27` to capture outbound email for
  the E2E auth tests. Removing it breaks the E2E suite.
- **`AppConstants.DemoUser*` and `EnableDummyUsers` config key** —
  consumed by `UserRouter` (`demo-credentials` endpoint) and
  `AppSetup.SeedDevelopmentUsersAsync` (demo + dummy user seed).
- **`using System;` in the EF migration files** — the IDE may flag
  this, but removing it can change migration signature behavior in
  subtle ways. Leave alone.
- **`Mii_BRAWLER`, `MII_SWORDFIGHTER`, `MII_GUNNER` in `CharacterName`**
  enum — listed but not used by any character file. The
  `Character.cs` base class has these defaulted to 0, so they're
  inert. If you trim the enum, do it as a separate, larger
  effort — there's no test that uses the Mii names today.
- **The `TestContracts/` directory** in `ApiTests/` — each file is a
  small DTO used by a specific test class. They are not dead.

---

## Recommended order of work (low-risk → higher-risk)

1. Delete `tourneyClient/src/components/ui/dropdown-menu.tsx` and
   `tourneyClient/src/pages/GuestSignUp.tsx`. Run `npm run build` to
   confirm.
2. Delete the 11 unused exception classes and the 2 unused
   validators. Run `dotnet build` to confirm.
3. Delete the 89 `tourneyAPI/Models/Entities/Characters/*.cs` files
   plus the `Entities.csproj` if nothing else uses it. Run
   `dotnet build` and the test suite to confirm.
4. Remove dead `using` directives mechanically (`dotnet format`).
5. Delete `UseInMemoryDatabase` from the three `appsettings*.json`
   files.
6. Remove `Microsoft.Extensions.Logging` from `tourneyAPI.csproj`.
7. Remove `motion`, `vite-plugin-eslint2` from root `package.json`,
   and `class-variance-authority`, `lucide-react`,
   `@radix-ui/react-dropdown-menu` from `tourneyClient/package.json`
   (the last three become unused after step 1).
8. Delete `AuthServiceTest.cs`. Run the test suite to confirm
   `AuthEndToEndTest` covers everything.
9. Delete the 4 dead `Game` entity methods and the 2 dead locks
   (`_votesLock`, `_currentRoundLock`) plus the `Votes` field. Run
   tests.
10. Delete the 5 dead `IGameService` methods (§2.1-2.5) and the 1
    dead `IUserManager.GetAllUsersAsync` (§2.6). Update the 4
    affected tests in `GameServiceTest`, `MultiGameTest`,
    `SessionResumeTest`. Run the test suite.
11. Update `docs/api-system-overview.md` to include the missing
    routes.
12. (Optional, last) Consolidate the verbose frontend test files
    (`RequestService.test.ts`, `matchVoteFeedback.test.ts`,
    `PersistentConnection.test.ts`).

Steps 1-7 are mostly cosmetic. Step 8 is mechanical with a known
test target. Steps 9-10 require careful test reruns (the user noted
this is MEDIUM risk). Step 11 is documentation. Step 12 is taste.
