
import BasicButton from "@/components/BasicButton";
import BasicHeading from "@/components/HeadingOne";

/* Ready for E2E testing */

const TourneyMenu = () =>
{

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw">
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
        <title>Super Smash Bros Inspired Main Menu</title>
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <audio id="background-music">
            <source src="smash.mp3" type="audio/mpeg" />
            Your browser does not support the audio element.
          </audio>

          <BasicHeading headingText="Where Do We Start?" headingColors="white" />
          <BasicButton buttonLabel="Host Tourney" href="/createTourney" />
          <BasicButton buttonLabel="Join Tourney" href="/joinTourney" />
          {
            /* 
                this was an originally intended feature but is being cut for MVP
                <BasicButton buttonLabel="User Profile" href="/aboutMe" />
            */
          }


        </div>
      </div>
    </div>
  );
};

export default TourneyMenu;
