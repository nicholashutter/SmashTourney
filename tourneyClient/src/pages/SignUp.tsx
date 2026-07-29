import BasicInput from "@/components/BasicInput";
import BasicHeading from "@/components/HeadingOne";
import SubmitButton from "@/components/SubmitButton";
import PageShell from "@/components/PageShell";
import StatusBanner, { StatusMessage } from "@/components/StatusBanner";
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
  const [status, setStatus] = useState<StatusMessage | null>(null);
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
      setStatus({ text: SERVER_ERROR("Invalid Characters"), tone: "error" });
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

      // The redirect to sign-in is the confirmation; a banner here would only
      // flicker as the page unmounts.
      navigate("/");
    }
    catch (err)
    {
      setStatus({ text: "We could not create your account. Try again.", tone: "error" });
      console.error(err);
    }


  };

  return (
    <PageShell pageTitle="User Registration">
        <BasicHeading headingText="Fill Out the Form Below" headingColors="white" />
        <BasicInput labelText="Username:" htmlFor="username" name="username" id="username" value={userName} onChange={handleUserNameChange} />
        <BasicInput labelText="Email:" htmlFor="email" name="email" id="email" value={email} onChange={handleEmailChange} />
        <BasicInput labelText="Password:" htmlFor="password" name="password" id="password" value={password} onChange={handlePasswordChange} />
        <StatusBanner status={status} />
        <SubmitButton buttonLabel="Sign Up" onSubmit={
          handleSubmit
        } />
    </PageShell>
  );
};

export { SignUp };
