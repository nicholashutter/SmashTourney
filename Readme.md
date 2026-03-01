#SmashTourney 

A super smash brothers ultimate themed double elimination tournament bracket tracker written in c# and react. The game is operated much like jackbox party pack games where users are expected to visit a web url and interact with the application through their smartphones while all sitting around playing smash ultimate

## Development demo login

In `Development`, the API seeds a demo identity user at startup and exposes it to the home page:

- Username: `demoUser`
- Password: `DemoP@ssword123!`

The React home page fetches these credentials from `/users/demo-credentials` and provides a **Use Demo Account** action to autofill sign-in fields.

## Development dummy users (optional)

Dummy identity users are available as an optional development seed and can be switched on per run profile.

- Username pattern: `dummy01` .. `dummy16`
- Password pattern: `DummyPass!01` .. `DummyPass!16`

Run API without dummy users (default):

- `dotnet run --project tourneyAPI`

Run API with dummy users enabled:

- `dotnet run --project tourneyAPI --launch-profile http-dummy-users`

The setting key is `DevelopmentSeed:EnableDummyUsers` and defaults to `false` in `appsettings.Development.json`.
When disabled, the development startup removes `dummy01` .. `dummy16` so the mode switch stays deterministic between runs.