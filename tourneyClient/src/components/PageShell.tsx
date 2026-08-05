import { ReactNode, useEffect, useState } from "react";
import { motion, useReducedMotion } from "framer-motion";
import { Volume2, VolumeX } from "lucide-react";
import { getSoundState, subscribeSoundState, toggleMuted } from "@/services/soundService";

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
    const [isMuted, setIsMuted] = useState<boolean>(() => getSoundState().muted);

    // The mute toggle is shared across every screen this shell wraps, so the
    // state is read once and kept in sync with the sound service subscribers.
    // A second tab flipping the toggle updates this view too via the storage
    // event wired up inside the service.
    useEffect(() =>
    {
        const unsubscribe = subscribeSoundState((state) =>
        {
            setIsMuted(state.muted);
        });

        return () => unsubscribe();
    }, []);

    // The toggle button is deliberately small and lives in the top-right
    // corner of every screen because audio is the only app-wide setting that
    // has to be reachable from wherever the user is. A label sits next to the
    // icon for screen readers; the icon is `aria-hidden` so it is not read
    // twice.
    const handleToggleMuted = () =>
    {
        const nextMuted = toggleMuted();
        setIsMuted(nextMuted);
    };

    return (
        <div className="flex flex-col items-center justify-center h-dvh w-dvw">
            <motion.div
                initial={prefersReducedMotion ? { opacity: 0 } : { opacity: 0, y: 16, scale: 0.98 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                transition={{ duration: 0.28, ease: "easeOut" }}
                className="relative flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10"
            >
                <title>{pageTitle}</title>
                <button
                    type="button"
                    onClick={handleToggleMuted}
                    aria-label={isMuted ? "Unmute sound effects" : "Mute sound effects"}
                    aria-pressed={isMuted}
                    className="absolute top-2 right-2 inline-flex items-center gap-1 rounded bg-black/40 hover:bg-black/60 text-white text-xs px-2 py-1 focus:outline-none focus:ring-2 focus:ring-yellow-400"
                >
                    {isMuted
                        ? <VolumeX className="w-4 h-4" aria-hidden="true" />
                        : <Volume2 className="w-4 h-4" aria-hidden="true" />}
                    <span>{isMuted ? "Sound off" : "Sound on"}</span>
                </button>
                {children}
            </motion.div>
        </div>
    );
}

export default PageShell;
