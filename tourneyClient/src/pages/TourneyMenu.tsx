
import BasicButton from "@/components/BasicButton";
import BasicHeading from "@/components/HeadingOne";
import PageShell from "@/components/PageShell";

// Renders the authenticated tournament navigation menu.
const TourneyMenu = () =>
{

  return (
    <PageShell pageTitle="Super Smash Bros Inspired Main Menu">
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <audio id="background-music">
            <source src="smash.mp3" type="audio/mpeg" />
            Your browser does not support the audio element.
          </audio>

          <BasicHeading headingText="Where Do We Start?" headingColors="white" />
          <BasicButton buttonLabel="Host Tourney" href="/createTourney" />
          <BasicButton buttonLabel="Join Tourney" href="/joinTourney" />


        </div>
    </PageShell>
  );
};

export { TourneyMenu };
