import BasicInput from "../components/BasicInput";
import BasicHeading from "../components/BasicHeading";
import SubmitButton from "../components/SubmitButton";



const SignUp = () =>
{

  return (
    <>
      <meta charSet="UTF-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <title>User Registration</title>
      <div className="flex flex-col items-center justify-center h-dvh w-dvw">
        <BasicHeading headingText="Welcome!" />
        <BasicInput labelText="Username:" htmlFor="username" name="username" id="username" />
        <BasicInput labelText="Email:" htmlFor="email" name="email" id="email" />
        <BasicInput labelText="Password:" htmlFor="password" name="password" id="password" />
        <BasicHeading headingText="Already have an account? Sign In Here" />
        <SubmitButton buttonLabel="Sign Up" onSubmit={
          () =>
          {
            window.alert("Submission Success");
          }
        } />
      </div>
    </>
  );
};

export default SignUp;
