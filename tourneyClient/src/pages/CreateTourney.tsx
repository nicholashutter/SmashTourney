import BasicHeading from "../components/BasicHeading";
import BasicInput from "../components/BasicInput";

function handleSubmit()
{
  window.alert("Submission Success");
}



const CreateTourney = () =>
{
  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
      <div className="container">
        <BasicHeading headingText="Create Tourney" />
        <form id="createTourneyForm" >
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
            <label htmlFor="ruleset">Ruleset:</label>
            <select id="ruleset" name="ruleset" >
              <option value="single">Single Elimination</option>
              <option value="double">Double Elimination</option>
            </select>
          </div>
          <div className="form-group">
            <label htmlFor="max-players">Max Players:</label>
            <input
              type="number"
              id="max-players"
              name="max-players"
              min={2}

            />
          </div>
          <div className="form-group">
            <Button type="submit" id="submitButton" onClick={handleSubmit} >Create Tourney</Button>
            <Link href="/tourneyMenu" className="cancel-link">
              Cancel and Return to Main Menu
            </Link>
          </div>
        </form>
      </div>
    </div>
  );
}

export default CreateTourney;
