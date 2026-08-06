# SmashTourney Frontend — Animated Brackets & UI Polish Proposal

**Status:** Draft for review
**Audit date:** read-only review of `tourneyClient/` at `C:\Development\FullStack\SmashTourney\tourneyClient`
**Scope:** Two parts — animated brackets (headline) + 5 general UI polish items

---

## 0. What is already in place (so we don't reinvent)

These are facts from the read-only audit and they shape every recommendation:

- **Stack:** React 19 + TS 5.7, Vite 6, Tailwind 4, Radix dropdown, lucide-react.
- **Animation library already installed:** `framer-motion@^12.43` (used in `PageShell`, `InMatch`, `PlayerList`, `StatusBanner`, and the bracket).
- **Tailwind animation plugin:** `tw-animate-css@^1.3.8` is imported in `src/index.css` but **no `animate-*` class from it is currently used** in the source.
- **Existing bracket motion** (`src/components/brackets/DynamicBracket.tsx`):
  - Match boxes: `motion.g` fades in via `opacity: 0→1` over 0.35s.
  - Connectors: `motion.polyline` driven by `src/services/drawService.ts` variants using `pathLength: 0→1` (spring, 1.2s) with a per-connector `delay = index / 12` — **per index, not per round**, so all lines essentially start together.
  - Parent `<motion.svg initial="hidden" animate="visible">` propagates the variant.
  - Active match: yellow stroke `#facc15`, strokeWidth 2.5, fillOpacity 0.95. No motion.
  - Winner / loser colors: green `#4ade80` for the winner row, gray `#94a3b8` for the loser, gray `#64748b` for BYE. Static.
- **Existing in-match motion** (`src/pages/InMatch.tsx`):
  - Selected winner button: `scale: 1.04` spring + yellow ring.
  - "Lock in Vote" button: wrapped in `AnimatePresence` with height/opacity transition.
- **Accessibility:** `useReducedMotion` is used in `src/components/PageShell.tsx` and `src/pages/BrowseTourneys.tsx` — bracket and InMatch do not currently honour it.
- **Audio asset:** `src/assets/sound/smash.mp3` (1.9 MB) exists, plus a stub `<audio id="background-music">` in `src/pages/TourneyMenu.tsx` that is **never actually played** (no autoplay, no controls, no `.play()` call). So sound infrastructure is half-built.
- **API surface** (`src/models/entities/Bracket.ts`): `BracketMatchView` carries `status: "PENDING" | "READY" | "IN_PROGRESS" | "COMPLETE"`, `winnerId`, `nextMatchForWinner`, and `nextMatchForLoser` — so the "drop to losers" connector is renderable from the data the API already returns. No backend change needed.
- **Polling cadence:** ShowBracket polls every 2 s, InMatch every 1.5 s, so animations need to either complete in <1.5 s or be reset/replayable per snapshot to avoid stale states.

**Implication:** framer-motion is the right tool. The existing `drawService` is the foundation to extend, not replace. We don't need any new heavy library.

---

## Part 1 — Animated Brackets (the headline ask)

For each item: target file(s), technique, complexity (S/M/L), accessibility notes, and a short design description.

### 1.1 Bracket connectors drawing in (staggered by round)

- **Files:** `src/components/brackets/DynamicBracket.tsx`, `src/services/drawService.ts`
- **Technique:** framer-motion `Variants` (already there). Change the per-connector delay from `index / 12` to `round * ROUND_STAGGER_MS` where `ROUND_STAGGER_MS ≈ 0.35 s` so round 1 lines finish drawing before round 2 starts. Add an optional **"shimmer"** overlay — a second `motion.polyline` on top with `strokeDasharray` animating from `0 L → L 0 → 0 L` to give the line a "current flowing along it" effect for ~0.6 s after it finishes drawing.
- **Direction note:** the polyline is currently drawn from source match to target. The visual semantic of a bracket is "winner flows forward" — drawing FROM source TO target is already correct, but consider a brief `scaleX: 0.85 → 1` settle on the target match to make the arrival land.
- **Complexity:** **M** — the delay rewrite is trivial; the shimmer is the new bit and is ~20 lines.
- **A11y:** none of this affects keyboard / SR — the SVG is presentational. With `prefers-reduced-motion`, the parent `motion.svg initial="hidden" animate="visible"` should short-circuit to `initial={false}` or use a 0-duration transition (see §0 and §1.8).

### 1.2 Active match indicator (bracket + in-match banner)

- **Files:** `src/components/brackets/DynamicBracket.tsx`, `src/pages/InMatch.tsx`
- **Technique:**
  - **Bracket side:** wrap the active match's `<rect>` stroke in an SVG `<animate>` (SMIL is fine here — keeps the cost on the renderer, not React) **or** use a CSS `@keyframes` pulse on `stroke-opacity` and `stroke-width`. Specifically: a 1.6 s `ease-in-out` infinite alternation between `stroke-width: 2.5 → 4` and `stroke-opacity: 1 → 0.7`, plus a soft outer glow via SVG `<filter id="glow">` (`feGaussianBlur` + `feMerge`) that breathes with the same period.
  - **"LIVE" tag:** render a small `<text>` with rounded background above the active match (e.g. `LIVE` in `#facc15` text on a 70% black pill). Animate it in with a subtle `y: -4 → 0` bounce when the match becomes active.
  - **In-match side:** add a top banner on `InMatch.tsx` that mirrors the bracket's "LIVE" state so the participant feels tied to the bracket — e.g. `MATCH X OF ROUND Y` with a thin yellow progress stripe. This is a single `motion.div` with `layout` so it doesn't reflow the rest of the screen.
- **Complexity:** **M** for bracket, **S** for in-match banner.
- **A11y:** the visual pulse is decorative; the in-match banner needs `aria-live="polite"` so SR users get the "your match is live" cue when it changes.

### 1.3 Match result commit animation

- **Files:** `src/components/brackets/DynamicBracket.tsx`, `src/pages/InMatch.tsx`, new `src/components/CelebrationOverlay.tsx`
- **Technique:**
  - **Bracket side (the "reveal"):** when `match.status === "COMPLETE"` for the *first time*, key the match `<motion.g>` on `match.status` so it remounts and replays its entrance. Use a `key="completed-{match.matchId}"` with a variants object that:
    - Winner row text: `scale: 0.7 → 1.1 → 1` spring, color `opacity 0 → 1` plus a brief 360° rotation of an underlying `motion.tspan` "WINNER" tag.
    - Loser row: `opacity: 1 → 0.35` over 0.4 s (the current code already dims losers, but the transition is instant — make it visible).
    - The connector OUT of this match re-runs its `drawService` "visible" variant (because the key change re-mounts the polyline).
  - **In-match side:** on `voteFeedback.alertMessage` for `COMMITTED`, mount `<CelebrationOverlay>` (a `motion.div` `position: fixed` covering the panel) showing the winner's name, a small SVG trophy, and a 4-corner confetti burst (4 absolutely-positioned particles, each with a different `x`/`y` translate + rotation, all using `framer-motion` `animate` with `transition: { delay: i * 0.05 }`). Overlay auto-dismisses after 1.4 s via `setTimeout` clearing local state.
  - **Sound:** trigger the existing `smash.mp3` here (see §2.5).
- **Complexity:** **L** — this is the highest-impact animation. Plan ~½ day.
- **A11y:** celebration overlay should be `role="status"` (not `alert`) and dismissed on `Escape`; confetti particles should be `aria-hidden`.

### 1.4 Round reveal (new round drops in)

- **Files:** `src/components/brackets/DynamicBracket.tsx`
- **Technique:** diff the current set of `round` numbers against the previous one (`useRef<Set<number>>`). For each `round` that newly appears, wrap that round's column in a parent `motion.g` with `variants={{ hidden: { opacity: 0 }, visible: { opacity: 1, transition: { staggerChildren: 0.08, delayChildren: 0.1 } } }}` and a child `variants={{ hidden: { opacity: 0, y: -12 }, visible: { opacity: 1, y: 0 } }}`. The connector draw for the *previous* round's last match → new round's first match is what visually links the two; we don't need a separate animation.
- **Important detail:** because the API returns the full match list each poll, the diff must be against the previous snapshot, not a cumulative set. Otherwise a re-render after a refresh will look "fresh" every time. A `useEffect` writing the current rounds to the ref AFTER the render is the simplest way.
- **Complexity:** **M** — diffing logic is the only non-trivial part.
- **A11y:** the round label `<text>` (already in the file) should remain in the DOM order; the staggered children need no special handling.

### 1.5 "This is you" indicator on the bracket

- **Files:** `src/components/brackets/DynamicBracket.tsx`, `src/pages/ShowBracket.tsx`
- **Technique:** pass `playerId` (and optionally `myCharacterColor`) from `ShowBracket` into `DynamicBracket` as a new prop. In `renderMatch`, when the match contains `playerId`, draw:
  - A second inner `<rect>` with `stroke: myCharacterColor` (e.g. the fighter's color from `FighterSelect.FALLBACK_COLORS`) and `stroke-width: 1`, dashed (`stroke-dasharray="3 2"`), offset 1px inside the outer rect.
  - A small `YOU` `<text>` badge in the corner.
  - On the player-row text where the user is, animate `font-weight: 500 → 700` once via framer-motion `animate` (no infinite loop — this is a "you are here" stamp, not a "live" pulse, to avoid competing with the LIVE indicator in §1.2).
- **Edge case:** the user might have been eliminated. Show the YOU tag on their most recent COMPLETE match as well, dimmed, so the bracket still reads "you are here, you lost here."
- **Complexity:** **S** to **M** depending on how rich the eliminated state is.
- **A11y:** `<title>You are in this match</title>` on the match group for SR users.

### 1.6 Winners → Losers drop connector (double-elim visual)

- **Files:** `src/components/brackets/DynamicBracket.tsx`
- **Technique:** new function `renderDropConnectors(positioned, positionsById, regions)` that iterates every match where `match.nextMatchForLoser` is set. For each, build a 3-point polyline: out the bottom of the source match, down to a "drop channel" y-coordinate (somewhere between the winnersRegion and losersRegion), then across to the x of the target losers match, then down into the target.
  - Stroke: `stroke="#f97316"` (orange — chosen because the current palette uses blue/gray for normal flow and yellow for live; orange is unused).
  - `stroke-dasharray="6 4"` to distinguish from solid winner-flow lines.
  - Animation: a `drawService`-style variant, but with a slight `delay: round * 0.35 + 0.4` so it draws AFTER the corresponding winner-flow line, and a short `animate` of `stroke-opacity: 1 → 0.6` after draw so it reads as "historical path" rather than "current live link."
  - End-of-line: render a small downward chevron `▼` as a `<text>` at the target match so the eye lands on it.
- **Layout caveat:** the current code positions losers at `y: safeHeight * 0.55` and winners end at most `0.47`, so the drop channel lives in a ~8% band — that's tight on a 360-px phone height. If needed, compress the winners region to 0.06–0.42 and let the drop channel sit at 0.46, with losers at 0.54–0.90.
- **Complexity:** **M** — geometry is the only annoying part; the rest is more of what we already have.
- **A11y:** `<title>This loser drops to the losers bracket</title>`.

### 1.7 Tournament completion — winner celebration

- **Files:** `src/components/brackets/DynamicBracket.tsx`, new `src/components/WinnerCelebration.tsx`
- **Technique:**
  - On `ShowBracket`, when `gameState.state === "COMPLETE"`, find the last `GRAND_FINALS` (or `GRAND_FINALS_RESET`) match with `status === "COMPLETE"` and the player with the highest seed-losses count is the champion (the API doesn't yet expose a `championPlayerId` directly — but the last COMPLETE grand finals match's `winnerId` is the champion by construction).
  - **Bracket side:** render a `motion.g` "crown" above the champion's row — three triangles using `<polygon>` filled `#facc15`, animated in with a `y: -8 → 0` spring and a `rotate: -8 → 0` settle. The champion's row text gets a permanent `font-weight: 700` + a one-shot `scale: 1 → 1.15 → 1` pulse.
  - **Overlay side:** mount `<WinnerCelebration>` (a `motion.div` filling the bracket panel) showing `"TOURNAMENT CHAMPION"` in stagger: each word as a separate `motion.span` with `delay: i * 0.18`, then the champion's name in a bigger `motion.h1` with a `letter-spacing: 0.6em → 0.15em` ease. Add a CSS keyframe "shine" (a 45° linear-gradient that sweeps across the name) on a 3 s loop. Audio: play `smash.mp3` once on mount.
- **Complexity:** **L** for the full effect; ship a **M** version first (crown + name reveal) and add the audio + shine in a second pass.
- **A11y:** the overlay should be `role="status"` and dismissible; respect `prefers-reduced-motion` by skipping the shine and the staggered word reveal.

### 1.8 (Cross-cutting) `prefers-reduced-motion` coverage

- **Files:** `src/components/brackets/DynamicBracket.tsx`, `src/pages/InMatch.tsx`, `src/components/CelebrationOverlay.tsx`, `src/components/WinnerCelebration.tsx`
- **Technique:** follow the existing `PageShell` pattern — call `useReducedMotion()` and short-circuit animations: connector draw becomes 0 duration, pulses become 0 amplitude, staggered reveals collapse to a single opacity fade of 0.15 s. A `useBracketMotionConfig()` hook in `src/hooks/useBracketMotionConfig.ts` would centralize this so all the bracket components share the same settings.
- **Complexity:** **S** once the hook exists; touching every new component is the only cost.

---

## Part 2 — General UI polish (5 high-impact items)

I verified the actual code rather than guessing. Each item names the file and the technique.

### 2.1 Lobby: animate the "X joined" notice

- **File:** `src/pages/Lobby.tsx`
- **Current state:** the notice is a static `<p>` (line 255-257) that just appears and disappears. `PlayerList` already animates per-player.
- **Technique:** wrap the notice in `<AnimatePresence>` (already imported in this codebase's components), `initial={{ opacity: 0, y: -8 }}`, `animate={{ opacity: 1, y: 0 }}`, `exit={{ opacity: 0, y: -8 }}`, 0.2 s — identical to the `StatusBanner` pattern, which is the right thing to copy.
- **Bonus:** add a short 1.2 s slide-in highlight (yellow ring) to the newly joined player's `Card` in `PlayerList` — `motion.div` with `animate={{ boxShadow: ["0 0 0 0px #facc15", "0 0 0 6px transparent"] }}` once.
- **Complexity:** **S**

### 2.2 In-match vote button states (pressed, waiting, locked, error)

- **File:** `src/pages/InMatch.tsx`, `src/components/SubmitButton.tsx`
- **Current state:** the player buttons are always green and identical; the lock button is plain. The StatusBanner conveys state via text, which is fine but slow to read.
- **Technique:**
  - **`SubmitButton`:** add a `disabled` prop, a `tone` prop (`"default" | "success" | "danger"`), and a small spinner slot. Use the `tw-animate-css` plugin's `animate-pulse` for "waiting" and a CSS `@keyframes` shake (translateX ±4 px, 4 cycles, 0.4 s) for "error." This is 30 lines of CSS + 10 lines of TSX.
  - **InMatch:** the "lock" button transitions to a `tone="success"` button reading `LOCKED ✓` once `isVoteLockedForActiveMatch` is true, and becomes `disabled`. If a vote comes back with `tone: "error"` (CONFLICT, MATCH_NOT_ACTIVE), the previously-selected button shakes once and is unselected.
  - **Visual hint for "the other player has voted":** when `voteResult.status === "PENDING"` and the *other* player voted first, change the local lock button's label from "Lock in Vote" to "Confirm their call" with `tone="success"` — this is game-flow visible at a glance.
- **Complexity:** **M** (button component is reused; in-match wiring is straightforward)
- **A11y:** `disabled` buttons need `aria-disabled`; the shake animation should be skipped under `prefers-reduced-motion`.

### 2.3 Empty + loading states (bracket, in-match, lobby)

- **Files:** `src/pages/ShowBracket.tsx`, `src/pages/InMatch.tsx`, `src/pages/Lobby.tsx`
- **Current state:** ShowBracket renders an inline `<text>Waiting for the bracket...</text>` in the SVG (DynamicBracket.tsx:259-269). InMatch shows `HeadingTwo "Waiting for next match..."`. Lobby has a card. All are static.
- **Technique:**
  - Replace the SVG `<text>` placeholder in `DynamicBracket.tsx` with a centred HTML overlay (absolute-positioned inside the bracket container) showing a stylized empty-state card with a CSS-only spinner (3 dots, `tw-animate-css animate-pulse` with staggered `animation-delay`) and the Smash-themed copy. Reason: the SVG `<text>` placeholder re-renders on every snapshot and currently re-mounts the whole tree.
  - InMatch: the existing `HeadingTwo` becomes a motion.div with the same spinner + copy. Add a subtle "your next match in..." countdown ring (SVG `<circle>` with `stroke-dasharray` driven by `secondsUntilMatch / 15`) when the 15 s redirect countdown is running — this currently shows as a plain `p` with "Your match is loading in 12s..."
  - Lobby: the existing "No players yet" card stays, but add a tiny CSS-shimmering placeholder card to suggest "more rows will appear here."
- **Complexity:** **S** each — ~20 min a piece.

### 2.4 Bracket match hover/click detail (player record)

- **File:** `src/components/brackets/DynamicBracket.tsx`, new `src/components/MatchDetailPopover.tsx`
- **Current state:** matches aren't interactive; everything visible is in the match box itself.
- **Technique:** each match `<g>` gets `onMouseEnter` / `onMouseLeave` / `onClick` handlers. The popover is a `position: absolute` HTML element (not SVG) that the parent container holds — coordinates are computed by mapping the match's SVG `x,y` back to pixel space using `getBoundingClientRect()` of the SVG. Content: each player's seed (already in `BracketPlayerView.seed`), current `losses`, `eliminated` flag, and a small "form" indicator (W-L last 3 matches — only if the API returns it; otherwise omit).
- **A11y:** the popover opens on `focus` too (so keyboard users can reach it by tabbing through matches) and traps `Escape` to close. `role="dialog"` with `aria-label="Match details."` Note: the bracket view is currently not a focusable screen, so the keyboard path requires making the match group `tabIndex={0}` and giving it an `aria-label` like "Match 3 of round 1: Player A vs Player B."
- **Complexity:** **M** — the coordinate math is the only fiddly bit, ~80 lines total.
- **Caveat (defer if needed):** the API returns `BracketPlayerView` with only `playerId`, `displayName`, `seed`, `losses`, `eliminated`. There's no per-player match history in the snapshot. If the team wants a "W/L last 3" form indicator, it has to come from a new backend endpoint — out of scope for this proposal.

### 2.5 Sound design — wire up the existing `smash.mp3`

- **File:** new `src/services/soundService.ts`, used in `src/components/brackets/DynamicBracket.tsx`, `src/pages/InMatch.tsx`, `src/components/WinnerCelebration.tsx`, `src/components/PageShell.tsx` (mute toggle).
- **Current state:** the file exists at `src/assets/sound/smash.mp3`. `TourneyMenu.tsx` has a stub `<audio id="background-music">` that is never played. There is no other audio code in the project.
- **Technique:** a tiny `soundService.ts` that exposes `play(soundName, options?)` and lazily creates `new Audio("/assets/sound/smash.mp3")` (or the Vite-bundled equivalent — note: `TourneyMenu` uses the bare filename `smash.mp3`, which works because Vite serves `public/`-rooted files; `src/assets/sound/smash.mp3` needs `import smashMp3 from "@/assets/sound/smash.mp3"`). Three events worth triggering:
  1. Match commit (COMMITTED vote response) — short clip / 0.4 s start offset.
  2. Tournament complete (gameState === "COMPLETE") — full clip.
  3. Vote conflict / error — short "bonk" — but we don't have that asset, so skip unless someone records one.
- **Mute toggle:** add a `localStorage["smashTourney.muted"]` boolean and a small lucide-react `Volume2` / `VolumeX` button in `PageShell` (the chrome every screen shares). Honour `prefers-reduced-motion` adjacent: this is a setting, not a media query, so the user controls it. But: if the OS-level "reduce motion" is on, default the sound OFF.
- **Complexity:** **S** for the service + integration; **S** for the mute toggle. The autoplay policy is the real gotcha — call `.play()` only from a user-gesture handler (e.g. the lock-in button click) or inside a `setTimeout` triggered by a click. The "tournament complete" overlay can play freely because by the time it mounts, the user has been clicking for 20+ minutes.
- **A11y:** mute state visible in `PageShell`; respect `prefers-reduced-motion` as the default off.

---

## Suggested implementation order

Two to three evenings of work, in this order (each step is independently shippable):

1. **`useBracketMotionConfig` hook + `useReducedMotion` everywhere** (§1.8) — 1 hour, makes everything else a11y-correct for free.
2. **Connector draw stagger by round + shimmer** (§1.1) — 2 hours, biggest single visible win.
3. **Active match pulse + LIVE tag** (§1.2) — 1 hour, ties together with §1.1.
4. **In-match vote button states** (§2.2) — 2 hours, biggest single UX win after the bracket.
5. **Round reveal stagger** (§1.4) — 1 hour.
6. **"You" indicator** (§1.5) — 1 hour.
7. **Winners → Losers drop connector** (§1.6) — 3 hours, geometry-heavy but high payoff.
8. **Match commit reveal + CelebrationOverlay** (§1.3) — 3 hours.
9. **Empty / loading states** (§2.3) — 1 hour, scattered.
10. **Sound service + mute toggle** (§2.5) — 2 hours, includes wiring the existing file.
11. **Match detail popover** (§2.4) — 4 hours, only if a player-history endpoint is added.
12. **Tournament complete celebration** (§1.7) — 3 hours, the final boss.

---

## What I did NOT propose, and why

- **No new libraries.** `framer-motion` already covers everything. Adding `react-spring`, `gsap`, `lottie`, or `react-confetti` would bloat the bundle for negligible gain. `tw-animate-css` is already in the project and currently under-used — several items above put it to work.
- **No 3D / WebGL / canvas effects.** The bracket is SVG and the rest is HTML/CSS — keep it that way. Phone GPUs on the same wifi as a party are not a place to experiment.
- **No per-player match history indicator** (§2.4 caveat). The API doesn't return it; adding it requires a backend change that's out of scope.
- **No new icons beyond `lucide-react`.** Already in the project.
- **No changes to lobby realtime plumbing, match vote API, or routing.** All proposals are frontend-only.

---

## Open questions for the team

1. Is the orange `#f97316` drop-connector color OK, or do you want a different color that's already in the Smash palette (e.g. a purple from the existing fallbacks)?
2. Should the "YOU" indicator also be visible on a player's most recent COMPLETE match after they're eliminated, or only on in-progress matches?
3. The celebration overlay blocks interaction for ~1.4 s — is that acceptable, or should it be purely decorative (pointer-events: none) and let the user tap through?
4. Sound: do we want a separate, quieter "click" for vote commit (requires sourcing a new asset), or just the full `smash.mp3` played at 0.3 s?
5. The bracket polls every 2 s. Animations that re-mount on every snapshot (the `key={match.status}` trick in §1.3) will re-play on each poll. Two options: (a) debounce the snapshot to 4 s, (b) track the previous status in a ref and only remount on actual change. Recommendation: (b), and that's what the proposal assumes.
