import { useEffect, useState, type ChangeEvent } from "react";
import { useNavigate } from "react-router";
import { RequestService } from "@/services/RequestService";
import { ApplicationUser } from '@/models/entities/ApplicationUser';
import { validateInput } from "@/services/validationService";
import { SERVER_ERROR, SUBMIT_SUCCESS, INVALID_CHARACTERS } from "@/constants/AppConstants";
import HeadingTwo from "@/components/HeadingTwo";
import BasicInput from "@/components/BasicInput";
import BasicHeading from "@/components/HeadingOne";
import SubmitButton from "@/components/SubmitButton";
import BasicButton from "@/components/BasicButton";

/*Ready for E2E testing */

const HomePage = () =>
{
  type DemoCredentials = {
    UserName: string;
    Password: string;
  }

  //dynamic import react router useNavigate
  const navigate = useNavigate();

  //user input values
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [demoCredentials, setDemoCredentials] = useState<DemoCredentials | null>(null);

  useEffect(() =>
  {
    const getDemoCredentials = async () =>
    {
      try
      {
        const result = await RequestService<"demoCredentials", never, DemoCredentials>("demoCredentials");
        setDemoCredentials(result);
      }
      catch
      {
        setDemoCredentials(null);
      }
    }

    getDemoCredentials();
  }, []);

  //setup user object RequestService expects
  const user: ApplicationUser =
  {
    UserName: userName,
    Password: password
  }

  const userNameHandler = (e: ChangeEvent<HTMLInputElement>) =>
  {
    setUserName(e.target.value);
  }

  const passwordHandler = (e: ChangeEvent<HTMLInputElement>) =>
  {
    setPassword(e.target.value);
  }

  const onSubmit = async () =>
  {
    const validateUserName = validateInput(userName).isValid;
    const validatePassword = validateInput(password).isValid;

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

  const useDemoLogin = () =>
  {
    if (demoCredentials === null)
    {
      return;
    }

    setUserName(demoCredentials.UserName);
    setPassword(demoCredentials.Password);
  }



  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
        <title>Smash Tourney</title>
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <BasicHeading headingText="Welcome!" headingColors="white" />
          <BasicInput labelText="Username:" htmlFor="username" name="username" id="username" value={userName} onChange={userNameHandler} />
          <BasicInput labelText="Password:" htmlFor="password" name="password" id="password" value={password} onChange={passwordHandler} />

          {demoCredentials && (
            <div className="text-base text-white/90 mb-2">
              <p>Demo Username: {demoCredentials.UserName}</p>
              <p>Demo Password: {demoCredentials.Password}</p>
              <SubmitButton buttonLabel="Use Demo Account" onSubmit={useDemoLogin} />
            </div>
          )}

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
