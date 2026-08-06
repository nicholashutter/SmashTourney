// CelebrationOverlay was removed during a UX review pass — the visual
// celebration on match commit was judged overly intrusive (see POLISH_EXECUTION.md
// follow-ups). The match commit flow is now silent on the visual side; the
// `voteCommit` sound still plays via `soundService` on user-gesture submission,
// and the tournament-complete celebration (`WinnerCelebration`) is still in
// place on `ShowBracket`. This file is intentionally empty so any stale import
// would surface as a TypeScript error rather than silently pulling in dead code.
// If the celebration is reintroduced, restore the component here and the
// `InMatch.tsx` import + render calls.
//
// Original location: src/components/CelrationOverlay.tsx (typo preserved for
// historical git blame).

export {};
