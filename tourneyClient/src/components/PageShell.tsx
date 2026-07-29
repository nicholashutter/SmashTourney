import { ReactNode } from "react";
import { motion, useReducedMotion } from "framer-motion";

type AppProps =
    {
        pageTitle: string;
        children: ReactNode;
    };

// Renders the centered translucent panel every screen sits inside.
//
// This markup was duplicated verbatim across eleven files, which is why the
// screens had drifted apart: changing the panel meant editing all eleven and
// nobody ever did. Keeping it in one place is what makes the look consistent
// rather than coincidentally similar — and it is why every page gained its
// entrance animation from a single edit.
const PageShell = ({ pageTitle, children }: AppProps) =>
{
    // Honours the OS "reduce motion" setting. Panels that fly in are pleasant
    // for most people and genuinely unpleasant for some, and the preference
    // exists to be read.
    const prefersReducedMotion = useReducedMotion();

    return (
        <div className="flex flex-col items-center justify-center h-dvh w-dvw">
            <motion.div
                initial={prefersReducedMotion ? { opacity: 0 } : { opacity: 0, y: 16, scale: 0.98 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                transition={{ duration: 0.28, ease: "easeOut" }}
                className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10"
            >
                <title>{pageTitle}</title>
                {children}
            </motion.div>
        </div>
    );
}

export default PageShell;
