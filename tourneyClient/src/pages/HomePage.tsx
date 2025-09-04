import React from "react";
import { useState } from "react";
import BasicInput from "../components/BasicInput";
import BasicHeading from "../components/BasicHeading";
import SubmitButton from "../components/SubmitButton";
import { RequestService } from "../utilities/RequestService";
import { ApplicationUser } from '../models/entities/ApplicationUser';


const HomePage: React.FC = () =>
{

  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");

  const user: ApplicationUser =
  {
    UserName: userName,
    Password: password
  }

  const handleUserNameChange = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setUserName(e.target.value);
  }

  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setPassword(e.target.value);
  }

  const onSubmit = () =>
  {
    RequestService(
      "login",
      {
        body: user
      }
    )
  }

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
      <title>Smash Tourney</title>
        <BasicHeading headingText="Welcome!" headingColors="white"/>
        <BasicInput labelText="Username:" htmlFor="username" name="username" id="username" value={userName} onChange={handleUserNameChange} />
        <BasicInput labelText="Password:" htmlFor="password" name="password" id="password" value={password} onChange={handlePasswordChange} />
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <SubmitButton buttonLabel="Sign In" onSubmit={onSubmit} />
          <a href="#" className="m-4  rounded shadow-md">
            Or
          </a>
          <SubmitButton buttonLabel="Continue As Guest" onSubmit={()=>
            {
              //once react router setup route to other page
              // then extract to stand alone function as in other event handlers
            }
          }/>
        </div>
      </div>
    </div>
  );
}

export default HomePage;
