#SmashTourney 

A super smash brothers ultimate themed double elimination tournament bracket tracker written in c# and react. The game is operated much like jackbox party pack games where users are expected to visit a web url and interact with the application through their smartphones while all sitting around playing smash ultimate

## Development demo login

In `Development`, the API seeds a demo identity user at startup and exposes its credentials on:

- `GET /users/demo-credentials` (development only)

Credentials:

- Username: `demoUser`
- Password: `DemoP@ssword123!`

A **Use Demo Account** action on the React home page is planned but not yet wired up; clients can fetch `/users/demo-credentials` directly to autofill sign-in fields for now.

## Development dummy users (optional, dev-only auth bypass)

Dummy identity users are available as an optional **dev-only** development seed. When enabled, all 16 dummy accounts are seeded with a **blank password** so the dev launch can be exercised without knowing any credentials — sign in with the username and an empty password field.

- Username pattern: `dummy01` .. `dummy16`
- Password: **blank** (any non-empty password will be rejected; the auth pipeline hashes `""` and matches the stored hash)

The dev launch is also available as a VS Code compound — see `SmashTourney: Full Stack (blank-password dummies)` in `.vscode/launch.json` — which starts the API in dev with the dummy seed plus the Vite client.

**This is dev-only by design.** The seeding only runs when `ASPNETCORE_ENVIRONMENT=Development` AND `DevelopmentSeed:EnableDummyUsers` is `true`. The blank password is written directly to `PasswordHash` to bypass the production password validators (`RequiredLength = 12` + mixed classes) — see `AppConstants.DummyUserPassword` and `AppSetup.cs:SeedDevelopmentUsersAsync`. The `demoUser` account keeps its real password and is not affected.

Run API with dummy users enabled (default in `Development`):

- `dotnet run --project tourneyAPI --launch-profile http-dummy-users`

Run API without dummy users:

- `dotnet run --project tourneyAPI --launch-profile http`

The setting key is `DevelopmentSeed:EnableDummyUsers` and defaults to `true` in `appsettings.Development.json`. When set to `false`, the development startup removes `dummy01` .. `dummy16` so the mode switch stays deterministic between runs.