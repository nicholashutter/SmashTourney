# PR 1 — Dev-only auth bypass: blank-password dummy users

**Branch:** `pr/blank-password-dev-dummies`
**Size:** 3 files, +33 / -20
**Risk:** Low. Gated by `ASPNETCORE_ENVIRONMENT=Development` and
`DevelopmentSeed:EnableDummyUsers`. No production code paths touched.

## What changes

- **`tourneyAPI/Utilities/AppConstants.cs`** — replaces `DummyUserPasswordPrefix = "DummyPass!"`
  with `DummyUserPassword = ""` and a comment explaining the dev-only intent.
- **`tourneyAPI/Utilities/ApplicationSetup/AppSetup.cs`** — dummy users are now seeded
  with `PasswordHash = identityUserManager.PasswordHasher.HashPassword(user, "")` and
  created via `CreateAsync(dummyUser)` (no password parameter). This bypasses the
  production password validators (`RequiredLength = 12` + mixed character classes),
  which would otherwise reject an empty string.
- **`.vscode/launch.json`** — renamed the dev-dummy config from
  `SmashTourney: API (dev + dummy users)` to
  `SmashTourney: API (dev, blank-password dummies)` and the compound from
  `SmashTourney: Full Stack (dummy users)` to
  `SmashTourney: Full Stack (blank-password dummies)`. Comments updated to spell
  out the auth-bypass use case.

## How to use

Sign in as `dummy01` .. `dummy16` with an **empty password field**. From a terminal:

```bash
dotnet run --project tourneyAPI --launch-profile http-dummy-users
```

From VS Code: pick `SmashTourney: Full Stack (blank-password dummies)` in the
Run and Debug dropdown.

## Why dev-only

`SeedDevelopmentUsersAsync` already had two gates:
1. `environment.IsDevelopment()` — must be Development
2. `configuration.GetValue<bool>(AppConstants.EnableDummyUsersConfigKey)` — must be true

This PR doesn't change either gate. The blank password only applies when both
gates are open (the dev launch + the dummy-user config key). Testing and
Production are unaffected. `demoUser` keeps its real password
(`DemoP@ssword123!`) and is unchanged.

## Verification

- `dotnet build` — 0 warnings, 0 errors
- `dotnet test` — 172/172 backend tests pass; no test relied on the old
  `DummyPass!NN` pattern.

## Follow-ups

- The Readme update for this change is included in PR 5 (doc updates) along
  with the drift fixes, so the documentation ships in one place.
