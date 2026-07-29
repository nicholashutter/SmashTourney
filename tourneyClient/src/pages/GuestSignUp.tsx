import { useState, type ChangeEvent } from 'react';
import { useNavigate } from "react-router";
import BasicInput from "@/components/BasicInput";
import HeadingOne from "@/components/HeadingOne";
import HeadingTwo from "@/components/HeadingTwo";
import SubmitButton from "@/components/SubmitButton";
import BasicButton from "@/components/BasicButton";
import PageShell from "@/components/PageShell";

// Renders guest signup flow using temporary guest-session credentials.
const GuestSignUp = () =>
{
  const navigate = useNavigate();
  const [userName, setUserName] = useState("");

  // Stores guest username input for session creation.
  const handleUserNameChange = (e: ChangeEvent<HTMLInputElement>) =>
  {
    setUserName(e.target.value);
  }

  // Submits guest session creation and routes to tournament menu.
  const handleSubmit = async () =>
  {
    window.alert("Guest signup is no longer supported. Please sign up or sign in.");
    navigate("/");
  }

  return (
    <PageShell pageTitle="Guest Sign Up">
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <HeadingOne headingText="Guest Sign Up" headingColors="white" />
          <BasicInput labelText="Username:" htmlFor="userName" name="userName"
            id="userName" value={userName} onChange={handleUserNameChange} />
          <SubmitButton buttonLabel="Guest Sign Up" onSubmit={handleSubmit} />
          <HeadingTwo headingText="Already have An Account?" />
          <BasicButton buttonLabel="Sign In Here" href="/" />
        </div>
    </PageShell>
  );
};

export { GuestSignUp };
