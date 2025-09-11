
export const TierPlacement =
    {
        S: "S",
        A: "A",
        B: "B",
        C: "C",
        D: "D",
        E: "E",
    } as const;

export type TierPlacement = typeof TierPlacement[keyof typeof TierPlacement];