import { CharacterName } from "@/models/Enums/CharacterName";

// Maps a fighter to its portrait file under assets/images/icons/characters.
//
// The 90 files there are real per-fighter roster icons, but a bulk download
// named every one of them after the first: banjo (1).png through banjo (90).png.
// Nothing in the filenames says which fighter is which.
//
// They turned out to be ordered alphabetically by fighter name, which made
// identifying them tractable — the order says what to expect and the image only
// has to confirm it. Every entry below was confirmed by looking at the image.
// The order was never trusted on its own, and it should not be: it breaks in
// three places, and deriving the tail from the head would have produced wrong
// portraits.
//
//   - Bowser Jr. sits at 36, between Kirby and Link, not beside Bowser.
//   - Steve sorts at 52, right after Min Min, because the set files him as
//     "Minecraft Steve".
//   - Olimar sorts at 61, after Pikachu, because the set files him as
//     "Pikmin & Olimar".
//
// Four files are deliberately unused. 5, 6 and 7 are alternate hair colours for
// Byleth, and 54 is Mythra drawn separately — the catalog carries Pyra and
// Mythra as a single fighter, so only Pyra's icon at 65 is referenced.
const characterIconFiles: Partial<Record<CharacterName, string>> = {
    [CharacterName.BANJO_AND_KAZOOIE]: "banjo (1).png",
    [CharacterName.BAYONETTA]: "banjo (2).png",
    [CharacterName.BOWSER]: "banjo (3).png",
    [CharacterName.BYLETH]: "banjo (4).png",
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
    [CharacterName.KING_K_ROOL]: "banjo (34).png",
    [CharacterName.KIRBY]: "banjo (35).png",
    [CharacterName.BOWSER_JR]: "banjo (36).png",
    [CharacterName.LINK]: "banjo (37).png",
    [CharacterName.LITTLE_MAC]: "banjo (38).png",
    [CharacterName.LUCARIO]: "banjo (39).png",
    [CharacterName.LUCAS]: "banjo (40).png",
    [CharacterName.LUCINA]: "banjo (41).png",
    [CharacterName.LUIGI]: "banjo (42).png",
    [CharacterName.MARIO]: "banjo (43).png",
    [CharacterName.MARTH]: "banjo (44).png",
    [CharacterName.MEGA_MAN]: "banjo (45).png",
    [CharacterName.META_KNIGHT]: "banjo (46).png",
    [CharacterName.MEWTWO]: "banjo (47).png",
    [CharacterName.MII_BRAWLER]: "banjo (48).png",
    [CharacterName.MII_GUNNER]: "banjo (49).png",
    [CharacterName.MII_SWORDFIGHTER]: "banjo (50).png",
    [CharacterName.MIN_MIN]: "banjo (51).png",
    [CharacterName.STEVE]: "banjo (52).png",
    [CharacterName.MR_GAME_AND_WATCH]: "banjo (53).png",
    [CharacterName.NESS]: "banjo (55).png",
    [CharacterName.PAC_MAN]: "banjo (56).png",
    [CharacterName.PALUTENA]: "banjo (57).png",
    [CharacterName.PEACH]: "banjo (58).png",
    [CharacterName.PICHU]: "banjo (59).png",
    [CharacterName.PIKACHU]: "banjo (60).png",
    [CharacterName.OLIMAR]: "banjo (61).png",
    [CharacterName.PIRANHA_PLANT]: "banjo (62).png",
    [CharacterName.PIT]: "banjo (63).png",
    [CharacterName.POKEMON_TRAINER]: "banjo (64).png",
    [CharacterName.PYRA_AND_MYTHRA]: "banjo (65).png",
    [CharacterName.ROB]: "banjo (66).png",
    [CharacterName.RICHTER]: "banjo (67).png",
    [CharacterName.RIDLEY]: "banjo (68).png",
    [CharacterName.ROBIN]: "banjo (69).png",
    [CharacterName.ROSALINA_AND_LUMA]: "banjo (70).png",
    [CharacterName.ROY]: "banjo (71).png",
    [CharacterName.RYU]: "banjo (72).png",
    [CharacterName.SAMUS]: "banjo (73).png",
    [CharacterName.SEPHIROTH]: "banjo (74).png",
    [CharacterName.SHEIK]: "banjo (75).png",
    [CharacterName.SHULK]: "banjo (76).png",
    [CharacterName.SIMON]: "banjo (77).png",
    [CharacterName.SNAKE]: "banjo (78).png",
    [CharacterName.SONIC]: "banjo (79).png",
    [CharacterName.SORA]: "banjo (80).png",
    [CharacterName.TERRY]: "banjo (81).png",
    [CharacterName.TOON_LINK]: "banjo (82).png",
    [CharacterName.VILLAGER]: "banjo (83).png",
    [CharacterName.WARIO]: "banjo (84).png",
    [CharacterName.WII_FIT_TRAINER]: "banjo (85).png",
    [CharacterName.WOLF]: "banjo (86).png",
    [CharacterName.YOSHI]: "banjo (87).png",
    [CharacterName.YOUNG_LINK]: "banjo (88).png",
    [CharacterName.ZELDA]: "banjo (89).png",
    [CharacterName.ZERO_SUIT_SAMUS]: "banjo (90).png",
};

// Eagerly resolves the bundled URL for every portrait file.
const iconUrls = import.meta.glob<string>(
    "../assets/images/icons/characters/*.png",
    { eager: true, import: "default" }
);

// Returns the portrait URL for a fighter, or null when it is not mapped.
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
