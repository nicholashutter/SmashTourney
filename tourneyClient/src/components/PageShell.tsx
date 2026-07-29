import { ReactNode } from "react";

type AppProps =
    {
        pageTitle: string;
        children: ReactNode;
    };

// Renders the centered translucent panel every screen sits inside.
//
// This markup was duplicated verbatim across ten files, which is why the
// screens had drifted apart: changing the panel meant editing all ten and
// nobody ever did. Keeping it in one place is what makes the look consistent
// rather than coincidentally similar.
const PageShell = ({ pageTitle, children }: AppProps) =>
{
    return (
        <div className="flex flex-col items-center justify-center h-dvh w-dvw">
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10">
                <title>{pageTitle}</title>
                {children}
            </div>
        </div>
    );
}

export default PageShell;
