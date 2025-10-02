import BasicInput from "@/components/BasicInput";
import BasicHeading from "@/components/HeadingOne";
import SubmitButton from "@/components/SubmitButton";
import { useState } from "react";
import { RequestService } from "@/services/RequestService";
import { SERVER_ERROR } from "@/constants/StatusMessages";
import { ApplicationUser } from "../models/entities/ApplicationUser";
import { validateInput } from "@/services/ValidationService";



const SignUp = () =>
{
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [applicationUser, setApplicationUser] = useState({} as ApplicationUser);


  const handleUserNameChange = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setUserName(e.target.value);
  };
  const handleEmailChange = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setEmail(e.target.value);
  };
  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setPassword(e.target.value);
  };

  const handleSubmit = async (e: React.MouseEvent<HTMLAnchorElement>) =>
  {
    e.preventDefault();

    const validateUserName = validateInput(userName);
    const validatePassword = validateInput(password);

    if (validateUserName && validatePassword)
    {
      setApplicationUser({
        UserName: userName, Password: password
      });

      try
      {
        await RequestService("register",
          {
            body: applicationUser
          }
        );
      }
      catch (err)
      {
        console.error(err);
      }
    }
    else
    {
      window.alert(SERVER_ERROR("Invalid Characters"));
    }


  };

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw">
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
        <title>User Registration</title>
        <BasicHeading headingText="Fill Out the Form Below" headingColors="white" />
        <BasicInput labelText="Username:" htmlFor="username" name="username" id="username" value={userName} onChange={handleUserNameChange} />
        <BasicInput labelText="Email:" htmlFor="email" name="email" id="email" value={email} onChange={handleEmailChange} />
        <BasicInput labelText="Password:" htmlFor="password" name="password" id="password" value={password} onChange={handlePasswordChange} />
        <SubmitButton buttonLabel="Sign Up" onSubmit={
          handleSubmit
        } />
      </div>
    </div>
  );
};

export default SignUp;
