import { useEffect, useRef } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { Card, CardContent } from "@/components/ui/card";
import { Player } from "@/models/entities/Player";
import { CharacterName } from "@/models/Enums/CharacterName";
import { resolveCharacterIcon } from "@/lib/characterIcons";

type AppProps =
  {
    players: Player[];
    // Ids of players that just joined. These cards get a one-shot yellow
    // ring flash so the rest of the room sees who arrived.
    recentlyJoinedPlayerIds?: ReadonlySet<string>;
  }

// Resolves a fighter portrait from whatever spelling of the name arrived.
//
// The join payload sends enum keys ("MARIO") while the icon map is keyed by
// display values ("Mario"), and which one comes back depends on the route. This
// tries the value first and falls back to translating a key. A miss is
// harmless — the card simply shows no portrait.
const resolvePortrait = (characterName: string | undefined): string | null =>
{
  if (!characterName)
  {
    return null;
  }

  const direct = resolveCharacterIcon(characterName as CharacterName);
  if (direct)
  {
    return direct;
  }

  const asKey = (CharacterName as Record<string, string>)[characterName];
  return asKey ? resolveCharacterIcon(asKey as CharacterName) : null;
};

const PlayerList = ({ players, recentlyJoinedPlayerIds }: AppProps) =>
{
  const resolvePlayerId = (player: Player): string =>
  {
    return player.Id ?? player.id ?? "";
  };

  // Tracks ids that have already been highlighted so the ring animation does
  // not replay on every unrelated re-render. The set is read at mount and
  // every snapshot, and ids are removed once the highlight window closes.
  const highlightedRef = useRef<Set<string>>(new Set());
  useEffect(() =>
  {
    if (!recentlyJoinedPlayerIds || recentlyJoinedPlayerIds.size === 0)
    {
      return;
    }

    const timeoutIds: number[] = [];
    recentlyJoinedPlayerIds.forEach((playerId) =>
    {
      if (highlightedRef.current.has(playerId))
      {
        return;
      }
      highlightedRef.current.add(playerId);
      const timeoutId = window.setTimeout(() =>
      {
        highlightedRef.current.delete(playerId);
      }, 1500);
      timeoutIds.push(timeoutId);
    });

    return () =>
    {
      timeoutIds.forEach((timeoutId) => window.clearTimeout(timeoutId));
    };
  }, [recentlyJoinedPlayerIds]);

  if (players.length === 0)
  {
    return (
      <Card className="max-w-sm overflow-hidden p-2 m-1">
        <CardContent>
          <p className="text-sm">No players yet. Share the session code and wait for joins.</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="flex flex-col gap-1">
      {/* Joins are animated because the lobby is the one screen everyone stares
          at while waiting. A card sliding in is how the room sees that someone
          made it, without anyone having to read a list to check. */}
      <AnimatePresence initial={false}>
        {players.map((player, index) =>
        {
          const portraitUrl = resolvePortrait(player.currentCharacter?.characterName);
          const playerId = resolvePlayerId(player) || `${player.displayName}-${index}`;
          const isHighlighted = highlightedRef.current.has(playerId);

          return (
            <motion.div
              key={playerId}
              layout
              initial={{ opacity: 0, x: -24 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: 24 }}
              transition={{ type: "spring", stiffness: 380, damping: 30 }}
            >
              <div className={isHighlighted ? "bracket-join-highlight rounded" : ""}>
                <Card className="max-w-sm overflow-hidden p-1 m-1">
                  <CardContent className="flex items-center gap-2">
                    {portraitUrl && (
                      <img
                        src={portraitUrl}
                        alt=""
                        className="w-8 h-8 rounded object-cover shrink-0"
                      />
                    )}
                    <div className="min-w-0 text-left">
                      <p className="text-sm truncate font-bold ">{player.displayName}</p>
                      <p className="text-sm truncate">{player.currentCharacter?.characterName}</p>
                    </div>
                  </CardContent>
                </Card>
              </div>
            </motion.div>
          );
        })}
      </AnimatePresence>
    </div>
  );
};

export default PlayerList
