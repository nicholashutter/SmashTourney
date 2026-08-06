# PR 4 — Remove write-only entity fields with EF migrations

**Branch:** `pr/field-removal`
**Size:** 7 fields removed, 2 migrations created, ~20 files touched.
**Risk:** Medium. Real wire-format change for `GET /Games/GetPlayersInGame/{gameId}`
and SignalR `PlayersUpdated` events (no longer emit `currentScore` /
`currentRound`). Verified no client consumer depends on the removed fields.

## What changes

### Entity field removals (server)
- **`Player.CurrentScore`** and **`Player.CurrentRound`** — removed from
  the entity, removed from the 4 write sites (3 in `GameService.cs`,
  1 in the BYE-player seed), removed from the client `Player` type
  and `AddPlayerPayload` type, removed from the request bodies in
  `CreateTourney.tsx` and `JoinTourney.tsx`.
- **`ApplicationUser.AllTimeMatches`**, **`AllTimeWins`**,
  **`AllTimeLosses`**, **`RegistrationDate`**, **`LastLoginDate`** —
  removed from the entity and the 4 write sites (`UserRouter.cs`,
  `UserManager.cs`, `AppSetup.cs` seed). These were never serialized
  to the client (the `/users/session` endpoint returns only
  `{ IsAuthenticated, UserId, UserName }`).

### Migrations
- **`20260806101339_DropPlayerCurrentScoreAndRound`** — drops
  `CurrentScore` and `CurrentRound` columns from `Players`.
- **`20260806102008_DropApplicationUserWriteOnlyFields`** — drops 5
  columns from `AspNetUsers` (Identity table).
- `ApplicationDbContextModelSnapshot.cs` regenerated.

Both `Up()` and `Down()` reviewed before commit; `Down()` recreates
columns with the right SQLite types, nullability, and defaults.

### Client changes
- `tourneyClient/src/models/entities/Player.ts` — removed the two fields
  from the `Player` interface.
- `tourneyClient/src/models/types/playerPayload.ts` — removed the two
  fields from `AddPlayerPayload`.
- `tourneyClient/src/pages/CreateTourney.tsx` and `JoinTourney.tsx` —
  removed the two fields from the request bodies sent to the server.

### Test fixture updates
Test fixtures that constructed player payloads with the dead fields
were updated to match the new wire format. The fixtures now explicitly
assert "this is what the client sends."

- `ApiTests/GameRouterTest.cs` (3 sites via the shared
  `CreateAddPlayerPayload` helper)
- `ApiTests/GameRouteAccessTest.cs` (1 site in `JoinGameAsync` helper)
- `ApiTests/GameServiceTest.cs` (1 BYE-player seed)
- `tourneyClient/tests/FrontendLifecycleFlow.test.ts`
- `tourneyClient/tests/PersistentConnection.test.ts`
- `tourneyClient/tests/RequestService.test.ts` (one request, one
  response mock)

### Wire-format breaking change
- `GET /Games/GetPlayersInGame/{gameId}` and SignalR `PlayersUpdated`
  events no longer emit `currentScore` / `currentRound`. Verified no
  client consumer depends on these values.
- `POST /users/register` and `GET /users/session` are unchanged
  (never serialized these fields).

## Key judgment calls
1. **Updated test fixtures** to match the new wire format rather than
   keep them sending the dead fields and expecting the server to
   ignore them. Cleaner — the fixtures explicitly assert the current
   contract.
2. **`Down()` defaults** left as EF generated them (`DateTime.MinValue`
   for `RegistrationDate`, `0` for the int columns). These match the
   original column shapes; the down-migration is a recovery path, not
   a normal operation.
3. **No `dotnet format` pass** needed — 0-warning build confirms no
   new unused `using` directives.

## Verification
- `dotnet build` — 0 warnings, 0 errors
- `dotnet test` — 172/172 pass (2m 04s)
- `npm run build` — clean
- `npm test` — 123/123 pass

## Reports
- `FIELD_REMOVAL_EXECUTION.md` — per-field outcome with all consumer
  sites the audit missed.
