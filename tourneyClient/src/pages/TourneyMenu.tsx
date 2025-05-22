
import "./page.css";


const TourneyMenu = () => {
  return (
    <>
      <meta charSet="UTF-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <title>Super Smash Bros Inspired Main Menu</title>
      <audio id="background-music">
        <source src="smash.mp3" type="audio/mpeg" />
        Your browser does not support the audio element.
      </audio>
      <div className="container">
        <button className="play-button">Play Music</button>
        <div className="title">Where Do We Start?</div>
        <div className="menu">
          <Link href="/createTourney" >
            <button className="menu-button">Host Tourney!</button>
          </Link>
          <Link href="/joinTourney" >
            <button className="menu-button">Join Tourney!</button>
          </Link>
          <Link href="/aboutMe" >
            <button className="menu-button">User Profile</button>
          </Link>
        </div>
      </div>
    </>
  );
};

export default TourneyMenu;
