const drawService = {
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
                type: "spring",
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
