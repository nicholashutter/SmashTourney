import { useMemo, useState, type ChangeEvent } from "react";
import { motion } from "framer-motion";
import { Character } from "@/models/entities/Character";
import { resolveCharacterIcon } from "@/lib/characterIcons";

type AppProps =
    {
        characters: Character[];
        selectedCharacter: Character | null;
        onSelect: (character: Character) => void;
    };

// Background colors used for fighters with no mapped portrait.
//
// Derived from the fighter's own name so a given fighter always gets the same
// tile color. A random or index-based color would shift whenever the catalog
// order changed, which would make the grid feel unstable between visits.
const FALLBACK_COLORS = [
    "#b91c1c", "#c2410c", "#a16207", "#15803d",
    "#0f766e", "#1d4ed8", "#6d28d9", "#a21caf",
];

// Picks a stable tile color for a fighter name.
const colorForName = (name: string): string =>
{
    let hash = 0;

    for (let index = 0; index < name.length; index++)
    {
        hash = (hash * 31 + name.charCodeAt(index)) % 100000;
    }

    return FALLBACK_COLORS[hash % FALLBACK_COLORS.length];
};

// Builds the short label shown when a fighter has no portrait.
const initialsForName = (name: string): string =>
{
    const words = name.split(/[\s&.]+/).filter(Boolean);

    if (words.length === 1)
    {
        return words[0].slice(0, 2).toUpperCase();
    }

    return words.slice(0, 2).map((word) => word[0]).join("").toUpperCase();
};

// Renders the roster grid players pick their fighter from.
//
// A dropdown listing 77 names is unusable on the phone this is played from,
// which is what everyone actually joins on. A filterable grid of tiles is both
// quicker to scan and closer to what the game itself does.
const FighterSelect = ({ characters, selectedCharacter, onSelect }: AppProps) =>
{
    const [filterText, setFilterText] = useState("");

    const visibleCharacters = useMemo(() =>
    {
        const normalizedFilter = filterText.trim().toLowerCase();

        if (!normalizedFilter)
        {
            return characters;
        }

        return characters.filter((character) =>
            character.characterName.toLowerCase().includes(normalizedFilter) ||
            character.archetype.toLowerCase().includes(normalizedFilter));
    }, [characters, filterText]);

    const handleFilterChange = (event: ChangeEvent<HTMLInputElement>) =>
    {
        setFilterText(event.target.value);
    };

    return (
        <div className="flex flex-col w-full">

            <label className="text-2xl text-white font-bold tracking-wide mb-1">
                CHOOSE YOUR FIGHTER
            </label>

            <input
                type="text"
                value={filterText}
                onChange={handleFilterChange}
                placeholder="Filter by name or archetype"
                aria-label="Filter fighters"
                className="text-base text-black bg-white rounded shadow-md px-3 py-2 m-2 focus:outline-none focus:ring-2 focus:ring-green-400"
            />

            <div
                role="listbox"
                aria-label="Fighter roster"
                className="grid grid-cols-4 sm:grid-cols-6 md:grid-cols-8 gap-2 p-2 max-h-[45dvh] overflow-y-auto"
            >
                {visibleCharacters.map((character) =>
                {
                    const isSelected = selectedCharacter?.id === character.id;
                    const iconUrl = resolveCharacterIcon(character.characterName);

                    return (
                        <motion.button
                            key={character.id}
                            type="button"
                            role="option"
                            aria-selected={isSelected}
                            title={character.characterName}
                            onClick={() => onSelect(character)}
                            whileHover={{ scale: 1.06 }}
                            whileTap={{ scale: 0.95 }}
                            animate={{ scale: isSelected ? 1.06 : 1 }}
                            transition={{ type: "spring", stiffness: 320, damping: 22 }}
                            className={`relative flex flex-col items-center justify-center aspect-square rounded overflow-hidden shadow-md focus:outline-none focus:ring-2 focus:ring-green-400 ${isSelected ? "ring-4 ring-yellow-400" : "ring-1 ring-white/25"
                                }`}
                            style={iconUrl ? undefined : { backgroundColor: colorForName(character.characterName) }}
                        >
                            {iconUrl
                                ? <img src={iconUrl} alt={character.characterName} className="w-full h-full object-cover" />
                                : <span className="text-white text-lg font-bold drop-shadow">{initialsForName(character.characterName)}</span>}

                            <span className="absolute bottom-0 w-full bg-black/70 text-white text-[9px] leading-tight py-0.5 px-0.5 truncate">
                                {character.characterName}
                            </span>
                        </motion.button>
                    );
                })}
            </div>

            {visibleCharacters.length === 0 && (
                <p className="text-base text-white m-2">No fighter matches that filter.</p>
            )}

            {/* The selected fighter's traits, so a pick is informed rather than
                just a name. Reserves its height whether or not anything is
                selected, so choosing does not shift the layout under a thumb. */}
            <div className="min-h-[4.5rem] flex flex-col items-center justify-center m-2">
                {selectedCharacter
                    ? (
                        <motion.div
                            key={selectedCharacter.id}
                            initial={{ opacity: 0, y: 8 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ duration: 0.25 }}
                            className="flex flex-col items-center"
                        >
                            <span className="text-xl text-white font-bold">{selectedCharacter.characterName}</span>
                            <div className="flex flex-wrap justify-center gap-1 mt-1">
                                <span className="text-xs bg-white/20 text-white rounded px-2 py-0.5">{selectedCharacter.archetype}</span>
                                <span className="text-xs bg-white/20 text-white rounded px-2 py-0.5">{selectedCharacter.weightClass}</span>
                                <span className="text-xs bg-white/20 text-white rounded px-2 py-0.5">{selectedCharacter.fallSpeed}</span>
                                <span className="text-xs bg-white/20 text-white rounded px-2 py-0.5">Tier {selectedCharacter.tierPlacement}</span>
                            </div>
                        </motion.div>
                    )
                    : <span className="text-base text-white/70">Pick a fighter to continue.</span>}
            </div>

        </div>
    );
}

export default FighterSelect;
