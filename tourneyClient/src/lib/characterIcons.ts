import { CharacterName } from "@/models/Enums/CharacterName";

// Maps a fighter to its portrait file under assets/images/icons/characters.
//
// The 90 files there are real per-fighter roster icons, but a bulk download
// named every one of them after the first: banjo (1).png through banjo (90).png.
// The art is fine — banjo (1) is Banjo-Kazooie and banjo (45) is Mega Man — but
// nothing in the filenames says which fighter is which, and there are 90 files
// against a 77-fighter catalog.
//
// Guessing would be worse than showing nothing: a wrong portrait states
// something false about a fighter, while a missing one just falls back to a
// typographic tile. So this maps only what has been confirmed by eye, and
// FighterSelect renders the fallback for everything else. Fill entries in as
// they are identified; no other file needs to change.
const characterIconFiles: Partial<Record<CharacterName, string>> = {
    [CharacterName.BANJO_AND_KAZOOIE]: "banjo (1).png",
    [CharacterName.MEGA_MAN]: "banjo (45).png",
};

// Eagerly resolves the bundled URL for every portrait file.
const iconUrls = import.meta.glob<string>(
    "../assets/images/icons/characters/*.png",
    { eager: true, import: "default" }
);

// Returns the portrait URL for a fighter, or null when it is not yet mapped.
export const resolveCharacterIcon = (characterName: CharacterName): string | null =>
{
    const fileName = characterIconFiles[characterName];

    if (!fileName)
    {
        return null;
    }

    const matchingPath = Object.keys(iconUrls).find((path) => path.endsWith(`/${fileName}`));
    return matchingPath ? iconUrls[matchingPath] : null;
};
