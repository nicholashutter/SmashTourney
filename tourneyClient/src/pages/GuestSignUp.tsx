import React from "react";
import { useState } from 'react';
import BasicInput from "../components/BasicInput";
import BasicHeading from "../components/BasicHeading";
import { RequestService } from "../utilities/RequestService";
import SubmitButton from "../components/SubmitButton";
import { ApplicationUser } from "../models/entities/ApplicationUser";



const GuestSignUp: React.FC = () =>
{

  const [userName, setUserName] = useState("");

  const password = "null";



  const guestUser: ApplicationUser =
  {
    UserName: userName,
    Password: password
  }

  const handleUserNameChange = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setUserName(e.target.value);
  }

  const onSubmit = () =>
  {
    RequestService(
      "createUserSession",
      {
        body: guestUser
      }
    )
  }

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw">
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> 
      <title>Guest Sign Up</title>
      <meta charSet="UTF-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <BasicHeading headingText="Guest Sign Up" headingColors="white" />
      <BasicInput labelText="Username:" htmlFor="userName" name="userName"
        id="userName" value={userName} onChange={handleUserNameChange} />
      <BasicHeading headingText="Already Have An Account?" headingColors="white" />
      <SubmitButton buttonLabel="Sign In Here" onSubmit=
        {onSubmit} />
        </div>
    </div>
  );
};

export default GuestSignUp;
