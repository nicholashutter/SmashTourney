import React from "react";
import { useState } from 'react';
import { useNavigate } from "react-router";
import { RequestService } from "@/services/RequestService";
import { ApplicationUser } from "@/models/entities/ApplicationUser";
import BasicInput from "@/components/BasicInput";
import HeadingOne from "@/components/HeadingOne";
import HeadingTwo from "@/components/HeadingTwo";
import SubmitButton from "@/components/SubmitButton";
import BasicButton from "@/components/BasicButton";


//Will need to create api endpoint using a predefined guest object
//with permissions that keep it from modifying db state
//or this feature will need to be dropped

const GuestSignUp = () =>
{
  const navigate = useNavigate();
  const [userName, setUserName] = useState("");

  const password = "null";



  const guestUser: ApplicationUser =
  {
    UserName: userName,
    Password: password
  }

  const userNameHandler = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setUserName(e.target.value);
  }

  const onSubmit = () =>
  {
    window.alert("submission success");
    RequestService(
      "createUserSession",
      {
        body: guestUser
      }
    )
    navigate("/tourneyMenu");
  }

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw">
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
        <title>Guest Sign Up</title>
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <HeadingOne headingText="Guest Sign Up" headingColors="white" />
          <BasicInput labelText="Username:" htmlFor="userName" name="userName"
            id="userName" value={userName} onChange={userNameHandler} />
          <SubmitButton buttonLabel="Guest Sign Up" onSubmit={onSubmit} />
          <HeadingTwo headingText="Already have An Account?" />
          <BasicButton buttonLabel="Sign In Here" href="/" />
        </div>
      </div>
    </div>
  );
};

export default GuestSignUp;
