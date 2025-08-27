export const WeightClass =
    {
        SUPER_HEAVYWEIGHT: "SUPER_HEAVYWEIGHT",
        HEAVYWEIGHT: "HEAVYWEIGHT",
        MIDDLEWEIGHT: "MIDDLEWEIGHT",
        LIGHTWEIGHT: "LIGHTWEIGHT",
        FEATHERWEIGHT: "FEATHERWEIGHT",
        BALLOONWEIGHT: "BALLOONWEIGHT",
    } as const;

export type WeightClassType = typeof WeightClass[keyof typeof WeightClass];