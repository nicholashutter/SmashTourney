
import BasicHeading from "../components/BasicHeading";
import SubmitButton from "../components/SubmitButton";


const TourneyMenu = () =>
{
  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw">
      <title>Super Smash Bros Inspired Main Menu</title>
      <audio id="background-music">
        <source src="smash.mp3" type="audio/mpeg" />
        Your browser does not support the audio element.
      </audio>
      <BasicHeading headingText="Where Do We Start?" />
      <SubmitButton buttonLabel="Host Tourney!" onSubmit={() => { }} />
      <SubmitButton buttonLabel="Join Tourney!" onSubmit={() => { }} />
      <SubmitButton buttonLabel="User Profile!" onSubmit={() => { }} />
    </div>
  );
};

export default TourneyMenu;
