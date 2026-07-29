import { AnimatePresence, motion } from "framer-motion";

// Severity of a status message, which decides its colour.
export type StatusTone = "error" | "success" | "info";

export type StatusMessage = {
    text: string;
    tone: StatusTone;
};

type AppProps =
    {
        status: StatusMessage | null;
    };

// Background colours per tone, kept away from the fighter tile palette so a
// status is never mistaken for a selection.
const TONE_CLASSES: Record<StatusTone, string> = {
    error: "bg-red-600/90",
    success: "bg-green-600/90",
    info: "bg-slate-700/90",
};

// Renders an inline status message in place of a modal alert.
//
// Every flow in this app previously reported through window.alert, which stops
// the page dead until it is dismissed. That is bad anywhere and worse here:
// this is played on phones by a room full of people, and a blocking dialog on
// one handset stalls that player while everyone waits. An inline banner says
// the same thing without taking the screen hostage, and it can be read while
// the form behind it is corrected.
const StatusBanner = ({ status }: AppProps) =>
{
    return (
        <div aria-live="polite" className="w-full min-h-[3rem] flex items-center justify-center">
            <AnimatePresence mode="wait">
                {status && (
                    <motion.p
                        key={status.text}
                        initial={{ opacity: 0, y: -8 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0, y: -8 }}
                        transition={{ duration: 0.2 }}
                        role={status.tone === "error" ? "alert" : undefined}
                        className={`text-base text-white rounded shadow-md px-3 py-2 m-2 max-w-full break-words ${TONE_CLASSES[status.tone]}`}
                    >
                        {status.text}
                    </motion.p>
                )}
            </AnimatePresence>
        </div>
    );
}

export default StatusBanner;
