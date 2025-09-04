
import BasicHeading from "../components/BasicHeading";
import SubmitButton from "../components/SubmitButton";


const TourneyMenu = () =>
{
  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw">
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
        <title>Super Smash Bros Inspired Main Menu</title>
        <audio id="background-music">
          <source src="smash.mp3" type="audio/mpeg" />
          Your browser does not support the audio element.
        </audio>
        <BasicHeading headingText="Where Do We Start?" headingColors="white" />
        <SubmitButton buttonLabel="Host Tourney!" onSubmit={() => { }} />
        <SubmitButton buttonLabel="Join Tourney!" onSubmit={() => { }} />
        <SubmitButton buttonLabel="User Profile!" onSubmit={() => { }} />
      </div>
    </div>
  );
};

export default TourneyMenu;
