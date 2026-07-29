import { Variants } from "framer-motion";

// Draws a stroked SVG path in, staggered by a per-element delay.
//
// `type` is asserted const because framer-motion types it as a union of
// generator names rather than string. Without it the whole object widens to
// Variants-incompatible and every consumer fails to typecheck.
const drawService: Variants = {
    hidden: {
        pathLength: 0,
        opacity: 0,
    },
    visible: (delay = 0) => ({
        pathLength: 1,
        opacity: 1,
        transition: {
            pathLength: {
                delay,
                type: "spring" as const,
                duration: 1.2,
                bounce: 0,
            },
            opacity: {
                delay,
                duration: 0.25,
            },
        },
    }),
};

export { drawService };
