import { useState, type ChangeEvent } from "react";
import { useNavigate } from "react-router";
import { RequestService } from "@/services/RequestService";
import { ApplicationUser } from '@/models/entities/ApplicationUser';
import { validateInput } from "@/services/validationService";
import { INVALID_CHARACTERS } from "@/constants/AppConstants";
import HeadingTwo from "@/components/HeadingTwo";
import BasicInput from "@/components/BasicInput";
import BasicHeading from "@/components/HeadingOne";
import SubmitButton from "@/components/SubmitButton";
import BasicButton from "@/components/BasicButton";
import PageShell from "@/components/PageShell";
import StatusBanner, { StatusMessage } from "@/components/StatusBanner";

// Renders the sign-in page and starts the authenticated user flow.
const HomePage = () =>
{
  const navigate = useNavigate();

  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [status, setStatus] = useState<StatusMessage | null>(null);

  // Stores username input from the sign-in form.
  const handleUserNameChange = (e: ChangeEvent<HTMLInputElement>) =>
  {
    setUserName(e.target.value);
  }

  // Stores password input from the sign-in form.
  const handlePasswordChange = (e: ChangeEvent<HTMLInputElement>) =>
  {
    setPassword(e.target.value);
  }

  // Submits login request and routes to the tournament menu on success.
  const handleSubmit = async () =>
  {
    const isUserNameValid = validateInput(userName).isValid;
    const isPasswordValid = validateInput(password).isValid;

    if (!isUserNameValid || !isPasswordValid)
    {
      setStatus({ text: INVALID_CHARACTERS("Login"), tone: "error" });
      return;
    }

    const user: ApplicationUser =
    {
      UserName: userName,
      Password: password
    };

    try
    {
      await RequestService(
        "login",
        {
          body: user
        }
      )

      // No success message: the navigation is the confirmation, and a banner
      // that unmounts immediately would only ever be seen as a flicker.
      navigate("/tourneyMenu");
    }
    catch (err)
    {
      setStatus({ text: "Sign in failed. Check your details and try again.", tone: "error" });
      console.error(err);
    }
  }

  return (
    <PageShell pageTitle="Smash Tourney">
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <BasicHeading headingText="Welcome!" headingColors="white" />
          <BasicInput labelText="Username:" htmlFor="username" name="username" id="username" value={userName} onChange={handleUserNameChange} />
          <BasicInput labelText="Password:" htmlFor="password" name="password" id="password" value={password} onChange={handlePasswordChange} />

          <StatusBanner status={status} />
          <SubmitButton buttonLabel="Sign In" onSubmit={handleSubmit} />
          <HeadingTwo headingText="Or" />
          <BasicButton buttonLabel="Sign Up" href="/signUp" />

        </div>
    </PageShell>
  );
}

export { HomePage };
