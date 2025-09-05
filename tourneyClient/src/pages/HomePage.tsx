import React from "react";
import { useState } from "react";
import { useNavigate } from "react-router";
import { RequestService } from "../utilities/RequestService";
import { ApplicationUser } from '../models/entities/ApplicationUser';
import HeadingTwo from "../components/HeadingTwo";
import BasicInput from "../components/BasicInput";
import BasicHeading from "../components/BasicHeading";
import SubmitButton from "../components/SubmitButton";


const HomePage: React.FC = () =>
{
  const navigate = useNavigate();
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
    window.alert("submission success"); 
    navigate("/tourneyMenu"); 
  }

  

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
        <title>Smash Tourney</title>
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
        <BasicHeading headingText="Welcome!" headingColors="white" />
        <BasicInput labelText="Username:" htmlFor="username" name="username" id="username" value={userName} onChange={handleUserNameChange} />
        <BasicInput labelText="Password:" htmlFor="password" name="password" id="password" value={password} onChange={handlePasswordChange} />
        
          <SubmitButton buttonLabel="Sign In" onSubmit={onSubmit} />
          <HeadingTwo headingText="Or"/>
          <SubmitButton buttonLabel="Continue As Guest" onSubmit={() =>
          {
            navigate("/guestSignUp");
          }
          } />
        </div>
      </div>
    </div>
  );
}

export default HomePage;
