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

## 5) Match Voting Capability

The API collects participant votes for active matches and commits bracket progression on consensus.

Business outcomes:

- accept one authenticated participant vote for the active match
- return pending status until both participants agree
- reject duplicate, stale, invalid, or non-participant votes
- commit winner and advance bracket when consensus is reached

## Realtime Game Contract Catalog

Game lifecycle and bracket progression are handled through SignalR hub methods on `/hubs/GameServiceHub`.

Hub methods:

- `CreateGameWithMode(CreateGameOptions)`
  - Creates a new game in single or double elimination.
- `AddPlayer(gameId, player)`
  - Adds one authenticated player to a game.
- `StartGame(gameId)`
  - Starts bracket progression for a game.
- `GetPlayersInGame(gameId)`
  - Returns players assigned to one game.
- `GetBracket(gameId)`
  - Returns bracket snapshot data for UI rendering.
- `GetCurrentMatch(gameId)`
  - Returns currently active match if one exists.
- `GetFlowState(gameId)`
  - Returns high-level game state used by frontend routing.
- `SubmitMatchVote(gameId, request)`
  - Accepts one authenticated participant vote and commits the match when participant votes agree.

Realtime events:

- `PlayersUpdated`
- `GameStarted`
- `FlowStateUpdated`
- `CurrentMatchUpdated`
- `BracketUpdated`
- `VoteSubmitted`

## User Routes

- Identity routes from ASP.NET Identity are mapped for register and login.
- Extended user routes under `/users` include:
  - `POST /users/login`
  - `GET /users/demo-credentials` (development only)
  - `GET /users/session` (authenticated)
  - `POST /users/logout` (authenticated)

## Reliability Notes

- Realtime game state events are authoritative for UI decisions.
- Match progression is event-driven from participant vote consensus, not manual round toggles.
- Bye progression is auto-resolved server-side before current-match and flow-state responses are returned.
- Real-vs-bye vote paths are treated as single-participant consensus to prevent waiting-state deadlocks.
- Bye detection uses layered identity checks (runtime bye IDs, null-object user ID, and fallback labels) so progression remains stable when one metadata source is stale.
