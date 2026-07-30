
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

          {/* The session code comes first because it is the only way into a game
              you are not already in. The list below it is for getting back into
              one you are: it shows the caller's own tournaments and nobody
              else's, so it cannot be used to go looking for a game to join. */}
          <BasicButton buttonLabel="Join With A Session Code" href="/joinTourney" />
          <BasicButton buttonLabel="My Tourneys" href="/browseTourneys" />


        </div>
    </PageShell>
  );
};

export { TourneyMenu };
