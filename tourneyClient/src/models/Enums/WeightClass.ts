export const WeightClass = {
    SUPER_HEAVYWEIGHT: "Super Heavyweight",
    HEAVYWEIGHT: "Heavyweight",
    MIDDLEWEIGHT: "Middleweight",
    LIGHTWEIGHT: "Lightweight",
    FEATHERWEIGHT: "Featherweight",
    BALLOONWEIGHT: "Balloonweight",
} as const

export type WeightClass = typeof WeightClass[keyof typeof WeightClass];