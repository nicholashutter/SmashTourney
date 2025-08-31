import React from "react";
import { useState } from "react";
import BasicInput from "../components/BasicInput";
import BasicHeading from "../components/BasicHeading";
import SubmitButton from "../components/SubmitButton";




const JoinTourney = () =>
{
  const [sessionCode, setSessionCode] = useState("");

  const handleSessionCode = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setSessionCode(e.target.value);
  }

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
      <BasicHeading headingText="Join Room" />
      <h2 className="title">Join Room</h2>
      <BasicInput labelText="Session Code:" htmlFor="sessionCode"
        id="sessionCode" name="sessionCode" value={sessionCode} onChange={handleSessionCode} />
      <SubmitButton buttonLabel="Join Room" onSubmit={() => { }} />
      <SubmitButton buttonLabel="Return To Main Menu" onSubmit={() =>
      {
        //when react router is setup, will route back to main page here
        //then extract it to an external function as all other event handlers
      }} />
    </div >
  );
};

export default JoinTourney;
