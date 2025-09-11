export const FallSpeed =
    {
        FAST_FALLERS: "Fast Fallers",
        FLOATY: "Floaty"
    } as const;

export type FallSpeed = typeof FallSpeed[keyof typeof FallSpeed];

