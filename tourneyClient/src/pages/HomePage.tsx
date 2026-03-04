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

// Renders the sign-in page and starts the authenticated user flow.
const HomePage = () =>
{
  const navigate = useNavigate();

  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");

  // Stores username input from the sign-in form.
  const userNameHandler = (e: ChangeEvent<HTMLInputElement>) =>
  {
    setUserName(e.target.value);
  }

  // Stores password input from the sign-in form.
  const passwordHandler = (e: ChangeEvent<HTMLInputElement>) =>
  {
    setPassword(e.target.value);
  }

  // Submits login request and routes to the tournament menu on success.
  const onSubmit = async () =>
  {
    const isUserNameValid = validateInput(userName).isValid;
    const isPasswordValid = validateInput(password).isValid;

    if (!isUserNameValid || !isPasswordValid)
    {
      window.alert(INVALID_CHARACTERS("Login"));
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

      window.alert("Signed in successfully. You are now moving to the tournament menu.");
      navigate("/tourneyMenu");
    }
    catch (err)
    {
      window.alert("Sign in failed. You will stay on this page so you can try again.");
      console.error(err);
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
          <BasicButton buttonLabel="Sign Up" href="/signUp" />
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
