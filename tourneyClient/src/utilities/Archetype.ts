
export const Archetype =
    {
        ALL_ROUNDER: "ALL_ROUNDER",
        RUSHDOWN: "RUSHDOWN",
        PRESSURER: "PRESSURER",
        HIT_AND_RUN: "HIT_AND_RUN",
        MIX_UP: "MIX_UP",
        ZONE_BREAKER: "ZONE_BREAKER",
        DOMINATING: "DOMINATING",
        POWERHOUSE: "POWERHOUSE",
        FOOTSIES: "FOOTSIES",
        GLASS_CANNON: "GLASS_CANNON",
        TAG_TEAM: "TAG_TEAM",
        FIGHTING_GAME_CHARACTER: "FIGHTING_GAME_CHARACTER",
        ZONER: "ZONER",
        BAIT_AND_PUNISH: "BAIT_AND_PUNISH",
        TRAPPER: "TRAPPER",
        TURTLE: "TURTLE",
        KEEP_AWAY: "KEEP_AWAY",
        STAGE_CONTROL: "STAGE_CONTROL",
        NINJA: "NINJA",
        GRAPPLER: "GRAPPLER",
        HALF_GRAPPLER: "HALF_GRAPPLER",
        HALF_HEALER: "HALF_HEALER",
        DYNAMIC: "DYNAMIC",
        TANK: "TANK",
        AURA: "AURA",
        FIRST_STRIKE: "FIRST_STRIKE",
        PRECISION: "PRECISION",
    } as const;

export type ArchetypeType = typeof Archetype[keyof typeof Archetype];