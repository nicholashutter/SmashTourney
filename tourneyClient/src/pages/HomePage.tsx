import React from "react";
import BasicButton from "../components/BasicButton";
import BasicInput from "../components/BasicInput";
import BasicHeading from "../components/BasicHeading";


const HomePage: React.FC = () =>
{

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
      <div className="flex flex-col content-center text-center rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
        <BasicHeading headingText="Welcome!" />
        <BasicInput labelText="Username:" htmlFor="username" name="username" id="username" />
        <BasicInput labelText="Password:" htmlFor="password" name="password" id="password" />
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <BasicButton buttonLabel="Sign In" />
          <a href="#" className="m-4 bg-black/25 rounded shadow-md">
            Or
          </a>
          <BasicButton buttonLabel="Continue As Guest" />

        </div>
      </div>
    </div>
  );
}

export default HomePage;
