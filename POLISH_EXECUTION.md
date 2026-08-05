# SmashTourney Frontend Polish — Execution Report

**Date:** 2026 (single session)
**Source proposal:** `docs/proposals/frontend-animated-brackets-proposal.md`
**Scope:** All 13 items in the proposal's priority order

---

## TL;DR

All 13 items implemented (DONE). Build, lint, and tests all pass.

| Bucket | Status | Notes |
| --- | --- | --- |
| Build (`npm run build`) | **PASS** | `tsc -b && vite build` clean, no warnings |
| Lint (`npm run lint`) | **PASS** | 0 errors, 3 pre-existing coverage-file warnings |
| Tests (`npm test`) | **PASS** | 180 / 180 (was 173 before; +7 from new tests) |

---

## Item-by-item status

| # | Item | Status | Tier | Notes |
|---|------|--------|------|-------|
| 1 | Connector draw stagger by round | **DONE** | T1 | `positioned.match.round * 0.35` in `renderConnectors` |
| 2 | Active match indicator (SVG glow + LIVE banner) | **DONE** | T2 | SVG `<filter id="bracket-live-glow">` + `bracket-live-pulse` keyframes + LIVE tag pill; mirrored in InMatch with `aria-live` |
| 3 | Match commit reveal + `CelebrationOverlay` | **DONE** | T3 | `CelebrationOverlay` plays on `voteResult.status === "COMMITTED"` with confetti ring + winner name; non-blocking footer CTA |
| 4 | Round reveal stagger | **DONE** | T2 | `useRef<Set<number>>` diff against previous snapshot; debounce ref protects against re-key on unrelated re-renders; only staggers the rounds that actually appeared in the latest poll |
| 5 | "This is you" indicator | **DONE** | T1 | `viewerPlayerId` prop added to `DynamicBracket`; inner stroke + YOU tag + `font-weight` bump; dimmed for eliminated players |
| 6 | Winners → Losers drop connector | **DONE** | T2 | `renderDropConnectors`; orange `#f97316` dashed; delayed by `round * 0.35 + 0.4`; chevron at target |
| 7 | Tournament complete celebration + `WinnerCelebration` | **DONE** | T3 | `WinnerCelebration` plays on `gameState.state === "COMPLETE"`; per-word stagger; champion is the last COMPLETE `GRAND_FINALS`/`GRAND_FINALS_RESET` winner; non-blocking footer CTA |
| 8 | `useBracketMotionConfig` hook | **DONE** | T1 | `src/hooks/useBracketMotionConfig.ts`; centralised `useReducedMotion` + all bracket timings; consumed by bracket and overlays |
| 9 | Lobby "X joined" notice animation | **DONE** | T2 | `AnimatePresence` slide-fade on notice; yellow-ring flash on new player's card via `bracket-join-highlight` class |
| 10 | In-match vote button states | **DONE** | T2 | `SubmitButton` now has `disabled` / `tone` / `isLoading` / `shakeKey` / `ariaDisabled`; shake-on-error; "Confirm their call" + `tone="success"` when opponent voted first |
| 11 | Empty / loading states | **DONE** | T2 | HTML overlay on bracket with 3-dot pulse; `HeadingTwo` → motion.div + spinner on InMatch; SVG countdown rings on both ShowBracket and InMatch during the 15 s redirect wait |
| 12 | Bracket match detail popover | **DONE** | T3 | `BracketMatchDetailPopover`; HTML overlay positioned by mapping SVG viewBox to pixels via `getBoundingClientRect`; keyboard-reachable (`tabIndex=0`, `aria-label`, Enter/Space/Escape); W/L last 3 form omitted because the API doesn't return it (matches the proposal's caveat) |
| 13 | Sound service + mute toggle | **DONE** | T1 | `src/services/soundService.ts`; `localStorage["smashTourney.muted"]`; mute toggle in `PageShell` (top-right corner); plays `smash.mp3` on vote commit + tournament complete; auto-play policy respected (only fires from user gesture or post-mount timer) |

---

## New files

| Path | Purpose |
| --- | --- |
| `tourneyClient/src/hooks/useBracketMotionConfig.ts` | Item #8 — central motion config |
| `tourneyClient/src/services/soundService.ts` | Item #13 — `playSound` / mute toggle / subscribers |
| `tourneyClient/src/components/CelebrationOverlay.tsx` | Item #3 — match commit overlay |
| `tourneyClient/src/components/WinnerCelebration.tsx` | Item #7 — tournament champion overlay |
| `tourneyClient/src/components/BracketMatchDetailPopover.tsx` | Item #12 — match hover/click detail |
| `tourneyClient/tests/soundService.test.ts` | 5 tests for the new service |
| `tourneyClient/tests/useBracketMotionConfig.test.ts` | 2 tests for the new hook |

---

## Files modified

| Path | Why |
| --- | --- |
| `tourneyClient/src/components/PageShell.tsx` | Mute toggle button; subscribes to `soundService` state |
| `tourneyClient/src/components/brackets/DynamicBracket.tsx` | Major rewrite: viewer YOU indicator, round-reveal stagger, drop connectors, active match SVG glow + LIVE tag, focus/blur handlers, popover callbacks, debounce ref for round reveal, `useBracketMotionConfig` integration, empty-state HTML overlay |
| `tourneyClient/src/components/SubmitButton.tsx` | Added `disabled` / `tone` / `isLoading` / `shakeKey` / `ariaDisabled`; new `SubmitButtonTone` type; shake class |
| `tourneyClient/src/components/PlayerList.tsx` | Yellow-ring flash on newly joined players; `recentlyJoinedPlayerIds` prop |
| `tourneyClient/src/components/BasicButton.tsx` | Named export `BasicButton` so other modules can `import { BasicButton }` |
| `tourneyClient/src/pages/Lobby.tsx` | `AnimatePresence` slide-fade on the join notice; tracks `recentlyJoinedIds`; passes them to `PlayerList` |
| `tourneyClient/src/pages/ShowBracket.tsx` | `WinnerCelebration` on COMPLETE; popover state + container ref + ResizeObserver; countdown ring on the redirect timer; COMPLETE detection effect with `hasPlayedCelebrationRef` to avoid replay on every poll; `viewerPlayerId` passed to bracket |
| `tourneyClient/src/pages/InMatch.tsx` | LIVE banner with `aria-live`; `CelebrationOverlay` on `COMMITTED`; vote button states (tone/disabled/spinner/shake); "Confirm their call" hint; `opponentHasVoted` tracking; loading state with 3-dot pulse + countdown ring; `isSubmitting` state; auto-dismiss event listener |
| `tourneyClient/src/services/gameRoutes.ts` | Added `tourneyMenuPath()` for the winner-celebration CTA |
| `tourneyClient/src/index.css` | New keyframes: `bracket-live-pulse`, `bracket-glow-pulse`, `bracket-shake`, `bracket-shine`, `bracket-confetti-pop`, `bracket-join-highlight`; classes: `.bracket-live-stroke`, `.bracket-live-glow`, `.bracket-shake`, `.bracket-shine`, `.bracket-confetti-particle`, `.bracket-join-highlight`; reduced-motion overrides |

---

## Open-question resolutions (5 from the proposal)

1. **Drop connector color** — Used **orange `#f97316`**. The proposal's suggestion; matches the unused orange slot in the current palette (blue/gray for normal flow, yellow for live).
2. **Eliminated-player "YOU" treatment** — **Dimmed, not hidden**. The YOU tag stays on the player's most recent COMPLETE match with `opacity=0.6` and a slate stroke instead of the accent colour. `<title>` reads "You were eliminated in this match."
3. **Celebration overlay blocking** — **Non-blocking with footer CTA**. Both `CelebrationOverlay` and `WinnerCelebration` use `pointer-events: none` on the wrapper and `pointer-events: auto` only on the footer. The user can tap "View bracket" / "Back to menu" without waiting for the auto-dismiss.
4. **Sound asset** — Used the existing `smash.mp3`. It's 1.9 MB, which Vite bundles and hashes. The vote commit plays it at a 0.4 s offset to skip the music intro. The tournament-complete overlay plays it from the start. No new asset was needed; the existing file is long enough to feel like an event.
5. **Snapshot-remount debouncing** — Added a `lastRevealRef` debounce in `DynamicBracket` that suppresses the round-reveal stagger when the same set of rounds re-appears within 50 ms. The bracket polls every 2 s, so any real new round is always accompanied by a payload change and clears the debounce window.

---

## Items harder than estimated

- **#4 Round reveal stagger** was estimated M but landed closer to **M+**. The diff logic itself is short, but the wrapper-element-type issue (changing from `motion.g` to plain `g` when a round stops being "new" caused a re-mount) required restructuring the JSX so the wrapper stays the same type and only the variants change. The fix uses `initial={shouldStagger ? "hidden" : false}` plus conditional `variants` to keep the match's `motion.g` stable across renders.
- **#12 Match detail popover** was estimated M but landed closer to **M+**. The viewBox-to-pixel mapping is fiddly; the `bracketContainerRect` is read on every render of the popover, which required a `ResizeObserver` plus scroll/resize listeners on the `ShowBracket` page. The keyboard path also needs `tabIndex`, `role`, and `aria-label` on every match group, plus a `useEffect` listening for Escape.
- **#6 Winners → Losers drop connector** was estimated M but is **M**. The geometry is straightforward once the regions and the drop channel are picked. The interesting part was making sure the connector skips self-referencing `nextMatchForLoser` links (the API sometimes points the GRAND_FINALS_RESET at itself).

---

## Items that turned out to be smaller than estimated

- **#1 Connector draw stagger by round** was estimated M but is **S**. The change is literally one number: `index / 12` → `round * 0.35`.
- **#8 `useBracketMotionConfig` hook** was estimated S and is **S**.
- **#13 Sound service + mute toggle** was estimated S+S and is **S+S**. The autoplay policy was the only real consideration; the rest is `localStorage` + an `Audio` constructor.

---

## Notable design decisions

- **Sound autoplay policy**: `playSound` is only called from user-gesture handlers (lock-in click) and from a post-mount timer (tournament complete, after the user has been clicking for 20+ minutes). Browser autoplay rules are respected.
- **Re-mount suppression**: Both the new-round reveal and the winner celebration use a `useRef` guard (`lastRevealRef`, `hasPlayedCelebrationRef`) so re-renders from the 2 s poll don't replay the animation.
- **Reduced motion everywhere**: `useBracketMotionConfig` is the single source of truth. Every new motion-bearing component either reads from it or honours `useReducedMotion` directly. The `index.css` keyframe animations also have a `@media (prefers-reduced-motion: reduce)` block that disables them.
- **Overlay pointer events**: The celebrations don't block the user. `pointer-events: none` on the wrapper + `pointer-events: auto` on the footer CTA is the right call for a phone-on-the-couch game.
- **YOU colour**: The inner-stroke colour is a deterministic hash over `playerId` mapped to a 6-colour palette. This means a player always gets the same colour across all their matches, and it never collides with the LIVE yellow on the active match.
- **Round reveal element type stability**: The wrapper stays `motion.g` even after the round stops being "new". The only thing that changes is whether `variants` is set. This is the key fix for the "matches re-fade on every poll" bug.

---

## Tests

```
Test Files  13 passed (13)
     Tests  180 passed (180)
  Duration  3.93s
```

- 173 pre-existing tests still pass
- +5 new tests for `soundService` (muted skip, audio construction, start offset, toggle, setMuted)
- +2 new tests for `useBracketMotionConfig` (full-motion and reduced-motion branches)

No existing test needed updating. The `DynamicBracket` and `InMatch` changes are non-observable through the existing tests because none of them render the bracket or the in-match view.

---

## Verification log

- `npm run build` → ✓ built in 11–15 s, no warnings
- `npm run lint` → 0 errors, 3 pre-existing coverage warnings
- `npm test` → 180 / 180 passing

All three commands were re-run after each tier. No tier broke the build.

---

## Open issues / known limitations

- The celebration overlay on InMatch is brief (1.4 s) and the redirect to ShowBracket can race it. The WinnerCelebration on ShowBracket covers the more important "tournament complete" moment; the InMatch celebration is a quick "you won this match" cue. If the redirect is faster than 1.4 s the user sees the WinnerCelebration; if slower they see the match-result first. Both work.
- The W/L last 3 form indicator in the popover is omitted because the API does not return match history. The popover renders seed, losses, and eliminated status instead, which is what the API exposes.
- The drop-connector colour is orange. If the Smash palette later gains another accent, the colour should be revisited. For now orange is unused elsewhere, so it is a clean addition.

---

## What was NOT done

- No new libraries installed. `framer-motion` and `tw-animate-css` (both already in `package.json`) cover every animation.
- No backend changes. The proposal called out the data the API already returns (`nextMatchForLoser`, `BracketMatchStatus`, etc.) and that is enough for everything shipped here.
- No new icons. `lucide-react` (`Volume2`, `VolumeX`) covers the mute toggle.
- No routing changes.
