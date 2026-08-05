import { useReducedMotion } from "framer-motion";

// Tone selectors for `SubmitButton`. Tones change the button colour while
// keeping the same shape so the lock-in button never reads as a different
// control — only the meaning of the same control shifts.
export type SubmitButtonTone = "default" | "success" | "danger" | "warning";

type AppProps = {
    buttonLabel: string;
    onSubmit: (e: React.MouseEvent<HTMLAnchorElement>) => void;
    // Disables the button. The `onSubmit` handler is not called when the
    // button is disabled, and the visual state shows the disabled cursor and
    // lowered opacity.
    disabled?: boolean;
    // Tints the button. "default" is the original green, "success" is the
    // yellow-rim green that reads as "you have locked this in", "danger" is
    // the red used for vote errors, and "warning" is the orange used for
    // "the other player already voted".
    tone?: SubmitButtonTone;
    // When true the button renders a small spinner next to the label. Used
    // while the lock-in request is in flight.
    isLoading?: boolean;
    // When provided the button runs a one-shot shake animation on the next
    // render. Bumping the number triggers a re-mount of the inner div so the
    // animation replays even when the value is set to the same number.
    shakeKey?: number;
    // `aria-disabled` is set on the underlying `<a>` so screen readers
    // announce the button as unavailable even though it is still focusable
    // for keyboard users who want to know what they cannot do.
    ariaDisabled?: boolean;
};

// Maps each tone to its Tailwind classes. Kept here rather than the inline
// string so the button stays a one-liner to use.
const TONE_CLASSES: Record<SubmitButtonTone, string> = {
    default: "bg-green-500 hover:bg-green-700 focus:ring-green-400",
    success: "bg-emerald-500 hover:bg-emerald-600 focus:ring-emerald-300 ring-2 ring-yellow-300",
    danger: "bg-red-600 hover:bg-red-700 focus:ring-red-300",
    warning: "bg-orange-500 hover:bg-orange-600 focus:ring-orange-300",
};

const SubmitButton = ({
    buttonLabel,
    onSubmit,
    disabled = false,
    tone = "default",
    isLoading = false,
    shakeKey = 0,
    ariaDisabled,
}: AppProps) =>
{
    const prefersReducedMotion = useReducedMotion();
    const toneClasses = TONE_CLASSES[tone];

    return (
        <a
            href="#"
            role="button"
            onClick={(e) =>
            {
                if (disabled)
                {
                    e.preventDefault();
                    return;
                }
                e.preventDefault();
                onSubmit(e);
            }}
            aria-disabled={ariaDisabled ?? disabled}
            className={`p-2 m-2 shrink text-white font-bold rounded shadow-md transition duration-300 ease-in-out focus:outline-none focus:ring-2 focus:ring-opacity-75 inline-flex items-center justify-center gap-2 ${toneClasses} ${disabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer"}`}
        >
            {isLoading && (
                <span
                    aria-hidden="true"
                    className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"
                />
            )}
            <span key={shakeKey} className={prefersReducedMotion ? "" : "inline-block"}>
                {!prefersReducedMotion && shakeKey > 0 ? (
                    <span className="inline-block bracket-shake">{buttonLabel}</span>
                ) : (
                    buttonLabel
                )}
            </span>
        </a>
    );
};

export default SubmitButton;
