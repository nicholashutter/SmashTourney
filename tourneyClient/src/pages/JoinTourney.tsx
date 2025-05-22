 
import User from "@/app/resources/user";

import "./page.css";
import Link from "next/link";
import { Button } from "@mui/material";

const JoinTourney = () => {
  return (
    <>
      <meta charSet="UTF-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <title>Join Room</title>
      <div className="container">
        <h2 className="title">Join Room</h2>
        <form>
          <div className="form-group">
            <label htmlFor="session-code">Session Code:</label>
            <input
              type="text"
              id="session-code"
              name="session-code"
              maxLength={4}
            />
          </div>
          <div className="form-group">
            <Button id="submitButton" variant="contained" type="submit">Join Room</Button>
            <Link href="/tourneyMenu" className="cancel-link">
              Cancel and Return to Main Menu
            </Link>
          </div>
        </form>
        <div id="waiting-message" className="waiting-message">
          Waiting for other players...
        </div>
      </div>
    </>
  );
};

export default JoinTourney;
