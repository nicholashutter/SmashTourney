import BasicHeading from "@/components/HeadingOne";
import PageShell from "@/components/PageShell";

// Renders the startup splash while the first bracket view is prepared.
const StartGame = () =>
{
    return (
        <PageShell pageTitle="Starting Game!">
            <BasicHeading headingText="Starting Game. Please Wait..." headingColors="white" />
            <div className='shrink flex flex-col text-2xl p-4 m-4 '>
            </div>
        </PageShell>);

}
export default StartGame;