import BasicInput from "../components/BasicInput";
import BasicHeading from "../components/BasicHeading";
import SubmitButton from "../components/SubmitButton";



const SignUp = () =>
{
  const placeholder = "";
  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw">
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
      <title>User Registration</title>
        <BasicHeading headingText="Welcome!" headingColors="white" />
        <BasicInput labelText="Username:" htmlFor="username" name="username" id="username" value={placeholder} onChange={() => { }} />
        <BasicInput labelText="Email:" htmlFor="email" name="email" id="email" value={placeholder} onChange={() => { }} />
        <BasicInput labelText="Password:" htmlFor="password" name="password" id="password" value={placeholder} onChange={() => { }} />
        <BasicHeading headingText="Already have an account? Sign In Here" headingColors="white" />
        <SubmitButton buttonLabel="Sign Up" onSubmit={
          () =>
          {
            window.alert("Submission Success");
          }
        } />
        </div>
    </div>
  );
};

export default SignUp;
