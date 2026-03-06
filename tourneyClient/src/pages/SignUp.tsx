import BasicInput from "@/components/BasicInput";
import BasicHeading from "@/components/HeadingOne";
import SubmitButton from "@/components/SubmitButton";
import { useState, type ChangeEvent } from "react";
import { RequestService } from "@/services/RequestService";
import { SERVER_ERROR } from "@/constants/AppConstants";
import { ApplicationUser } from "@/models/entities/ApplicationUser";
import { validateInput } from "@/services/validationService";
import { useNavigate } from 'react-router';

// Renders account registration form and submits new user signup.
const SignUp = () =>
{
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const navigate = useNavigate();

  // Stores username input for registration.
  const handleUserNameChange = (e: ChangeEvent<HTMLInputElement>) =>
  {
    setUserName(e.target.value);
  };

  // Stores email input for registration.
  const handleEmailChange = (e: ChangeEvent<HTMLInputElement>) =>
  {
    setEmail(e.target.value);
  };

  // Stores password input for registration.
  const handlePasswordChange = (e: ChangeEvent<HTMLInputElement>) =>
  {
    setPassword(e.target.value);
  };

  // Submits registration request and returns to sign-in page on success.
  const handleSubmit = async (e: React.MouseEvent<HTMLAnchorElement>) =>
  {
    e.preventDefault();

    const isUserNameValid = validateInput(userName).isValid;
    const isPasswordValid = validateInput(password).isValid;

    if (!isUserNameValid || !isPasswordValid)
    {
      window.alert(SERVER_ERROR("Invalid Characters"));
      return;
    }

    const applicationUser: ApplicationUser = {
      UserName: userName,
      Password: password
    };

    try
    {
      await RequestService("register",
        {
          body: applicationUser
        }
      );

      window.alert("Account created successfully. You are now moving to the sign-in page.");

      navigate("/");
    }
    catch (err)
    {
      window.alert("We could not create your account. You will stay on this page so you can try again.");
      console.error(err);
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

export { SignUp };
