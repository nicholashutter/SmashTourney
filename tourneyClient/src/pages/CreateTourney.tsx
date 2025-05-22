
import './page.css';




function handleSubmit()
{
  window.alert("Submission Success");
}



const CreateTourney = () =>
{
  return (
    <>
      <meta charSet="UTF-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <title>Create Tourney</title>
      <div className="container">
        <h2 className="title">Create Tourney</h2>
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
    </>
  );
}

export default CreateTourney;
