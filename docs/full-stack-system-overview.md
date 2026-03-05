# SmashTourney System Overview (Non-Technical)

This document explains what the system does, how the major parts fit together, and what each public API route is responsible for.

For detailed subsystem documentation, see:

- `docs/api-system-overview.md`
- `docs/frontend-system-overview.md`
- `docs/testing-system-overview.md`

## Product Purpose

SmashTourney helps players run and participate in tournament sessions. It supports both:

- Single Elimination
- Double Elimination

The system handles:

- Account/session entry
- Lobby management
- Bracket progression
- Match voting and consensus
- Tournament completion

## Main Modules

## 1) User Access Module

Responsible for creating and validating signed-in sessions.

User outcomes:

- Sign in
- Register
- Session validation
- Sign out

## 2) Tournament Setup Module

Responsible for creating tournament sessions and selecting rules.

User outcomes:

- Create a new tournament
- Choose elimination mode
- Join existing tournament
- Add participants to a session

## 3) Lobby Module

Responsible for pre-game player coordination.

User outcomes:

- See players in the session
- Receive join updates
- Host starts the tournament

## 4) Bracket Flow Module

Responsible for tournament progression from first match to completion.

System outcomes:

- Generate bracket state
- Determine current match
- Advance bracket on participant vote consensus
- Expose flow status for clients

## 5) Match Voting Module

Responsible for collecting participant winner votes and moving tournament forward.

System outcomes:

- Accept participant winner votes for active matches
- Commit results when participant votes agree
- Update bracket state
- Calculate next active match
- Auto-resolve bye-involved matches so odd-sized brackets continue without user stall

## 6) Realtime + Recovery Module

Responsible for making client screens stay in sync.

System outcomes:

- Broadcast key events (players updated, game started)
- Provide polling-safe endpoints so clients can recover from missed realtime updates

## Frontend Experience Flow

1. User signs in.
2. User creates or joins a tournament.
3. User enters lobby and waits for start.
4. Bracket view appears.
5. Active match participants move to in-match vote screen.
6. Non-active participants remain in bracket/waiting view.
7. Tournament advances until complete.

## Architecture Summary

- Backend API is the source of truth for `GameState` and bracket progression.
- Frontend routes are driven by backend `GameState` and current-match responses.
- SignalR broadcasts speed up updates, while polling endpoints protect against missed events.

## API Route Map (Business Function)

## Tournament Lifecycle Routes

- `POST /Games/CreateGameWithMode`
  - Creates a session with selected bracket type.

- `POST /Games/AddPlayer/{gameId}`
  - Adds one authenticated player to a session.

- `POST /Games/StartGame/{gameId}`
  - Starts the tournament and initializes bracket progression.

- `GET /Games/GetBracket/{gameId}`
  - Returns current bracket view data.

- `GET /Games/GetCurrentMatch/{gameId}`
  - Returns the active match when one is available.

- `GET /Games/GetFlowState/{gameId}`
  - Returns authoritative overall `GameState` (`LOBBY_WAITING`, `BRACKET_VIEW`, `IN_MATCH_ACTIVE`, `COMPLETE`).

- `POST /Games/SubmitMatchVote/{gameId}`
  - Records one authenticated participant vote and commits progression only after participant consensus.

## Supporting Routes

- `POST /Games/GetPlayersInGame/{gameId}`
  - Returns players currently assigned to a session.

## User/Session Routes

- User identity endpoints are exposed through identity and user routers (register/login/session/profile/logout).

## Operational Notes

- Frontend and backend are designed around one authoritative game state endpoint for route decisions.
- Realtime updates improve responsiveness, while polling endpoints provide reliability and recovery.
- Tournament progression is driven by participant-vote consensus and bracket state transitions, not by manual round toggles.
- Odd-sized tournaments progress through backend bye auto-resolution until the next real player-votable match.
