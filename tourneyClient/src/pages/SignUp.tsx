import BasicInput from "@/components/BasicInput";
import BasicHeading from "@/components/HeadingOne";
import SubmitButton from "@/components/SubmitButton";
import PageShell from "@/components/PageShell";
import StatusBanner, { StatusMessage } from "@/components/StatusBanner";
import { useState, type ChangeEvent } from "react";
import BasicButton from "@/components/BasicButton";
import HeadingTwo from "@/components/HeadingTwo";
import { RequestService } from "@/services/RequestService";
import { SERVER_ERROR } from "@/constants/AppConstants";
import { validateInput } from "@/services/validationService";
import
{
  describeRegistrationFailure,
  RegisterAccountRequest,
  RegisterAccountResponse
} from "@/services/registrationService";

// Renders account registration form and submits new user signup.
const SignUp = () =>
{
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [status, setStatus] = useState<StatusMessage | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [hasRegistered, setHasRegistered] = useState(false);

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

    // The address was collected by the form and then left out of the request
    // entirely, so every account was created — when the route existed — with no
    // way to reach its owner. It is required now, because confirming it is what
    // makes the account usable.
    if (!email.trim())
    {
      setStatus({ text: "An email address is required. The confirmation link goes there.", tone: "error" });
      return;
    }

    const registration: RegisterAccountRequest = {
      userName: userName.trim(),
      email: email.trim(),
      password
    };

    try
    {
      setIsSubmitting(true);

      await RequestService<"register", RegisterAccountRequest, RegisterAccountResponse>("register",
        {
          body: registration
        }
      );

      // No redirect to sign-in any more: signing in will not work until the
      // address is confirmed, so sending someone straight to a login form they
      // cannot yet use is how you get a person convinced the app is broken.
      setHasRegistered(true);
      setStatus(null);
    }
    catch (err)
    {
      console.error(err);

      const message = err instanceof Error ? err.message : "";

      // The server explains why a password or address was refused, and that
      // reason is the only thing that lets somebody fix it. A flat "try again"
      // leaves them retyping the same rejected password.
      setStatus({ text: describeRegistrationFailure(message), tone: "error" });
    }
    finally
    {
      setIsSubmitting(false);
    }
  };

  // The screen after a successful sign-up is a dead end on purpose: the next
  // step happens in an email client, and there is nothing useful to do here
  // until it does.
  if (hasRegistered)
  {
    return (
      <PageShell pageTitle="Check Your Email">
        <div className="shrink flex flex-col text-2xl p-4 m-4">
          <BasicHeading headingText="Almost There" headingColors="white" />
          <HeadingTwo headingText={`We sent a confirmation link to ${email.trim()}. Open it, then come back and sign in.`} />
          <BasicButton buttonLabel="Go to Sign In" href="/" />
        </div>
      </PageShell>
    );
  }

  return (
    <PageShell pageTitle="User Registration">
        <BasicHeading headingText="Fill Out the Form Below" headingColors="white" />
        <BasicInput labelText="Username:" htmlFor="username" name="username" id="username" value={userName} onChange={handleUserNameChange} />
        <BasicInput labelText="Email:" htmlFor="email" name="email" id="email" value={email} onChange={handleEmailChange} />
        <BasicInput labelText="Password:" htmlFor="password" name="password" id="password" value={password} onChange={handlePasswordChange} />
        <StatusBanner status={status} />
        <SubmitButton buttonLabel={isSubmitting ? "Creating account..." : "Sign Up"} onSubmit={
          handleSubmit
        } />
        <BasicButton buttonLabel="Return to Sign In" href="/" />
    </PageShell>
  );
};

export { SignUp };
