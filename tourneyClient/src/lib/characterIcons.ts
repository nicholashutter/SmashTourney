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
// The files turn out to be ordered alphabetically by fighter name, which made
// identifying them far quicker — the order says what to expect and the image
// only has to confirm it. Every entry below was confirmed by eye; the order was
// never trusted on its own, because it drifts: extra unlabelled slots sit at
// 4-7 and at 36, so deriving the tail from the head would have been wrong.
const characterIconFiles: Partial<Record<CharacterName, string>> = {
    [CharacterName.BANJO_AND_KAZOOIE]: "banjo (1).png",
    [CharacterName.BAYONETTA]: "banjo (2).png",
    [CharacterName.BOWSER]: "banjo (3).png",
    [CharacterName.CAPTAIN_FALCON]: "banjo (8).png",
    [CharacterName.CHROM]: "banjo (9).png",
    [CharacterName.CLOUD]: "banjo (10).png",
    [CharacterName.CORRIN]: "banjo (11).png",
    [CharacterName.DAISY]: "banjo (12).png",
    [CharacterName.DARK_PIT]: "banjo (13).png",
    [CharacterName.DARK_SAMUS]: "banjo (14).png",
    [CharacterName.DIDDY_KONG]: "banjo (15).png",
    [CharacterName.DONKEY_KONG]: "banjo (16).png",
    [CharacterName.DR_MARIO]: "banjo (17).png",
    [CharacterName.DUCK_HUNT]: "banjo (18).png",
    [CharacterName.FALCO]: "banjo (19).png",
    [CharacterName.FOX]: "banjo (20).png",
    [CharacterName.GANONDORF]: "banjo (21).png",
    [CharacterName.GRENINJA]: "banjo (22).png",
    [CharacterName.HERO]: "banjo (23).png",
    [CharacterName.ICE_CLIMBERS]: "banjo (24).png",
    [CharacterName.IKE]: "banjo (25).png",
    [CharacterName.INCINEROAR]: "banjo (26).png",
    [CharacterName.INKLING]: "banjo (27).png",
    [CharacterName.ISABELLE]: "banjo (28).png",
    [CharacterName.JIGGLYPUFF]: "banjo (29).png",
    [CharacterName.JOKER]: "banjo (30).png",
    [CharacterName.KAZUYA]: "banjo (31).png",
    [CharacterName.KEN]: "banjo (32).png",
    [CharacterName.KING_DEDEDE]: "banjo (33).png",
    [CharacterName.KIRBY]: "banjo (35).png",
    [CharacterName.LINK]: "banjo (37).png",
    [CharacterName.LUCARIO]: "banjo (39).png",
    [CharacterName.LUCAS]: "banjo (40).png",
    [CharacterName.MEGA_MAN]: "banjo (45).png",
    [CharacterName.PIKACHU]: "banjo (60).png",
    [CharacterName.ROSALINA_AND_LUMA]: "banjo (70).png",
    [CharacterName.SHEIK]: "banjo (75).png",
    [CharacterName.SORA]: "banjo (80).png",
    [CharacterName.ZERO_SUIT_SAMUS]: "banjo (90).png",
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
