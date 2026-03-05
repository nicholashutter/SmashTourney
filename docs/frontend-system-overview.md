# SmashTourney Frontend Overview (Non-Technical)

This document explains what the frontend does, how users move through the app, and how each major UI module supports tournament operations.

## Frontend Purpose

The frontend provides the user experience for:

- account sign-in and registration
- tournament creation and joining
- lobby coordination
- bracket viewing
- in-match winner voting with consensus handling
- automatic bye progression handling for odd-sized brackets

It consumes API responses and game-state signals to decide which screen users should see.

## User Journey

1. User signs in or registers.
2. User creates a new tournament or joins an existing one.
3. User enters lobby and waits for host to start.
4. User sees live bracket updates.
5. Active players enter in-match view and submit winner votes.
6. Game continues until completion state is reached.

## Frontend Modules

## 1) Routing and Session Guards

Core files:

- `src/main.tsx`
- `src/components/RequireAuth.tsx`
- `src/components/GameSessionGuard.tsx`
- `src/components/GameIdContext.tsx`

Business purpose:

- protect authenticated routes
- persist active game session context
- prevent accidental route escapes during active games

## 2) Tournament Setup Pages

Core files:

- `src/pages/CreateTourney.tsx`
- `src/pages/JoinTourney.tsx`

Business purpose:

- create a game with selected mode
- add host/player records to game
- validate game id and player input

## 3) Lobby Experience

Core files:

- `src/pages/Lobby.tsx`
- `src/services/PersistentConnection.ts`
- `src/services/gameFlowService.ts`

Business purpose:

- show joined players
- receive realtime player updates
- transition to bracket screen when game starts

## 4) Bracket and Match Experience

Core files:

- `src/pages/ShowBracket.tsx`
- `src/pages/InMatch.tsx`
- `src/components/brackets/*`

Business purpose:

- show current bracket state
- identify active match participants
- submit winner votes for active match
- surface vote status outcomes (`PENDING`, `COMMITTED`, conflicts, and validation errors)
- continue polling while matches are transient or non-votable so players are routed when their next match opens

## 5) API Integration Layer

Core files:

- `src/services/RequestService.ts`
- `src/models/entities/Bracket.ts`
- `src/services/playerSetupService.ts`
- `src/services/matchVoteFeedback.ts`

Business purpose:

- centralize endpoint metadata
- send typed requests and parse responses
- normalize session and character mapping payloads
- convert vote responses/errors into deterministic UI feedback actions

## UX Reliability Notes

- Realtime updates improve responsiveness in lobby and game transitions.
- Polling fallbacks keep pages consistent if events are missed.
- Frontend uses backend `GameState` as source of truth for route decisions.
- Odd-sized brackets are supported through backend bye auto-resolution, and frontend flows wait for the next real player-votable match.
