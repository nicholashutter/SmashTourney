import React from "react";
import { useState } from "react";
import { useNavigate } from "react-router";
import { RequestService } from "@/services/RequestService";
import { ApplicationUser } from '../models/entities/ApplicationUser';
import { validateInput } from "@/services/ValidationService";
import { SERVER_ERROR, SUBMIT_SUCCESS, INVALID_CHARACTERS } from "@/constants/AppConstants";
import HeadingTwo from "@/components/HeadingTwo";
import BasicInput from "@/components/BasicInput";
import BasicHeading from "@/components/HeadingOne";
import SubmitButton from "@/components/SubmitButton";
import BasicButton from "@/components/BasicButton";
import { useGameData } from "@/components/GameIdContext";

/*Ready for E2E testing */

const HomePage = () =>
{
  //dynamic import react router useNavigate
  const navigate = useNavigate();

  //user input values
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");

  //setup user object RequestService expects
  const user: ApplicationUser =
  {
    UserName: userName,
    Password: password
  }

  const userNameHandler = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setUserName(e.target.value);
  }

  const passwordHandler = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setPassword(e.target.value);
  }

  const onSubmit = async () =>
  {
    const validateUserName = validateInput(userName);
    const validatePassword = validateInput(password);

    if (validateUserName && validatePassword)
    {
      try
      {
        await RequestService(
          "login",
          {
            body: user
          }
        )

        window.alert(SUBMIT_SUCCESS("Login"));
        navigate("/tourneyMenu");
      }
      catch (err)
      {
        window.alert(SERVER_ERROR("Login"));
        console.error(err);
      }
    }
    else
    {
      window.alert(INVALID_CHARACTERS("Login"));
    }
  }



  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
        <title>Smash Tourney</title>
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <BasicHeading headingText="Welcome!" headingColors="white" />
          <BasicInput labelText="Username:" htmlFor="username" name="username" id="username" value={userName} onChange={userNameHandler} />
          <BasicInput labelText="Password:" htmlFor="password" name="password" id="password" value={password} onChange={passwordHandler} />

          <SubmitButton buttonLabel="Sign In" onSubmit={onSubmit} />
          <HeadingTwo headingText="Or" />
          <BasicButton buttonLabel="Sign Up" href="/signUp"/>
          {
            /* 
          <SubmitButton buttonLabel="Continue As Guest" onSubmit={() =>
          {
            navigate("/guestSignUp");
          }
          } />*/
          }

        </div>
      </div>
    </div>
  );
}

export default HomePage;
