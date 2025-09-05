
const drawService = {
    hidden: { pathLength: 0, opacity: 0 },
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    visible: (i: any) =>
    {
        const delay = 1 + i * 0.5;
        return {
            pathLength: 1,
            opacity: 1,
            transition: {
                pathLength: { delay, type: "spring", duration: 1.5, bounce: 0 },
                opacity: { delay, duration: 0.01 }
            }
        };
    }
};

export default drawService