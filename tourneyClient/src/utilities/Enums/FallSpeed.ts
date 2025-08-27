export const FallSpeed =
{
    FAST_FALLERS: "FAST_FALLERS",
    FLOATY: "FLOATY"
} as const;

export type FallSpeedType = typeof FallSpeed[keyof typeof FallSpeed];

