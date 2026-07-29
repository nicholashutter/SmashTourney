
import BasicButton from "@/components/BasicButton";
import BasicHeading from "@/components/HeadingOne";
import PageShell from "@/components/PageShell";

// Renders the fallback route when no client page matches the URL.
const NotFound = () =>
{

    return (
        <PageShell pageTitle="Not Found">
            <div className='shrink flex flex-col text-2xl p-4 m-4 '>

                <BasicHeading headingText="The page you are looking for cannot be found." headingColors="white" />
                <BasicButton buttonLabel="Return Home" href="/" />

            </div>
        </PageShell>
    );
};

export { NotFound };