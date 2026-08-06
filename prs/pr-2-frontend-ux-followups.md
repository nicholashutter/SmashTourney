# PR 2 — Frontend UX follow-ups: hide YOU for eliminated, remove celebration overlay

**Branch:** `pr/frontend-ux-followups`
**Size:** 3 files
**Risk:** Low. Visual / state-only changes; no API or behavior contract change.

## What changes

- **`tourneyClient/src/components/brackets/DynamicBracket.tsx`** — the "YOU" tag,
  colored inner stroke, and font-weight bump on the viewer's name are now hidden
  entirely when the viewer has been eliminated from that match
  (`viewerEliminated === true`). The bracket no longer draws attention to a
  player who is out.
- **`tourneyClient/src/components/CelebrationOverlay.tsx`** — the per-match
  celebration component is replaced with a documentation stub. The full
  visual was judged overly intrusive during a UX review. The file is kept
  with a comment explaining why it's empty so any stale import surfaces as a
  TypeScript error rather than silently pulling in dead code.
- **`tourneyClient/src/pages/InMatch.tsx`** — removes the import, state,
  `smash-tourney:celebration-done` event handler, renderer, and now-unused
  `BasicButton` import. The `playSound("voteCommit", ...)` call is kept —
  the sound on user-gesture vote commit is audio, not the overly-blocking
  visual the user wanted removed.

## What does NOT change

- `WinnerCelebration` (tournament-complete overlay on `ShowBracket`) is
  untouched — only the per-match celebration was overly intrusive.
- All 13 polish items from the prior `docs/proposals/frontend-animated-brackets-proposal.md`
  remain in place. This PR is a single UX adjustment on top of the polish work
  that landed in commit `4944401`.
- The sound service is unchanged; the `voteCommit` and tournament-complete
  sounds still play under the existing autoplay-policy rules.

## Verification

- `npm run build` — clean
- `npm run lint` — 0 errors (3 pre-existing coverage warnings, none from
  this PR)
- `npm test` — 123/123 passing
