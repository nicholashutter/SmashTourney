# SmashTourney API Overview (Non-Technical)

This document explains what the API does, which business actions it supports, and how client screens use those actions.

## API Purpose

The API is the system of record for tournament progression.

It is responsible for:

- user identity session checks
- tournament creation and lifecycle
- player assignment to games
- bracket generation and progression
- match vote collection and result recording
- authoritative game state reporting

## Business Capabilities

## 1) Identity and Session Capability

The API validates whether a user is signed in and allowed to perform tournament actions.

Business outcomes:

- register and login support
- authenticated session status checks
- logout support

## 2) Tournament Setup Capability

The API creates and configures tournament sessions.

Business outcomes:

- create a game with default mode
- create a game with explicit mode
- retrieve game and player setup data

## 3) Lobby Participation Capability

The API adds players to an existing game before start.

Business outcomes:

- add authenticated player to game
- retrieve current players in game
- reject late joins after game start

## 4) Bracket Progression Capability

The API controls the full tournament state machine.

Business outcomes:

- start bracket progression
- return current bracket snapshot
- return active match
- return high-level game state (`LOBBY_WAITING`, `BRACKET_VIEW`, `IN_MATCH_ACTIVE`, `COMPLETE`)

## 5) Match Voting and Reporting Capability

The API collects participant votes for active matches and commits bracket progression on consensus.

Business outcomes:

- accept one authenticated participant vote for the active match
- return pending status until both participants agree
- reject duplicate, stale, invalid, or non-participant votes
- commit winner and advance bracket when consensus is reached
- retain direct match-report route for service/testing fallback usage

## API Route Catalog

## Game Lifecycle Routes

- `POST /Games/CreateGame`
  - Creates a new game with default settings.

- `POST /Games/CreateGameWithMode`
  - Creates a new game in single or double elimination.

- `POST /Games/AddPlayer/{gameId}`
  - Adds one authenticated player to a game.

- `POST /Games/StartGame/{gameId}`
  - Starts bracket progression for a game.

- `GET /Games/GetBracket/{gameId}`
  - Returns bracket snapshot data for UI rendering.

- `GET /Games/GetCurrentMatch/{gameId}`
  - Returns currently active match if one exists.

- `GET /Games/GetFlowState/{gameId}`
  - Returns high-level game state used by frontend routing.

- `POST /Games/ReportMatch/{gameId}`
  - Applies a winner report directly to the current bracket state (service/testing fallback).

- `POST /Games/SubmitMatchVote/{gameId}`
  - Accepts one authenticated participant vote and commits the match when participant votes agree.

- `GET /Games/EndGame/{gameId}`
  - Ends and removes the game.

## Supporting Game Routes

- `GET /Games/getAllGames`
  - Returns all games.

- `GET /Games/GetGameById/{gameId}`
  - Returns one game by ID.

- `POST /Games/GetPlayersInGame/{gameId}`
  - Returns players assigned to one game.

- `POST /Games/LoadGame/{gameId}`
  - Hydrates game state from persistence.

- `POST /Games/SaveGame/{gameId}`
  - Saves runtime game state.

## User Routes

- Identity routes from ASP.NET Identity are mapped for register/login/logout and session checks.
- Extended user routes are mapped under `/users`.

## Reliability Notes

- The API game state endpoint is authoritative for UI decisions.
- SignalR events improve realtime responsiveness, while polling endpoints preserve reliability.
- Match progression is event-driven from participant vote consensus (or fallback direct reports), not manual round toggles.
