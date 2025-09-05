
import { createRoot } from 'react-dom/client';
import './index.css';
import HomePage from './pages/HomePage.tsx';
import { BrowserRouter, Routes, Route } from "react-router";
import NotFound from './pages/NotFound.tsx';
import GuestSignIn from './pages/GuestSignUp.tsx';
import CreateTourney from './pages/CreateTourney.tsx';
import JoinTourney from './pages/JoinTourney.tsx';
import SignUp from './pages/SignUp.tsx';
import TourneyMenu from './pages/TourneyMenu.tsx';
import ParentBracket from './pages/Brackets/ParentBracket.tsx';
import Lobby from './pages/Lobby.tsx';
import TwoPlayers from './pages/Brackets/TwoPlayers.tsx';

createRoot(document.getElementById('root')!).render(
  <BrowserRouter>
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/createTourney" element={<CreateTourney />} />
      <Route path="/joinTourney" element={<JoinTourney />} />
      <Route path="/signUp" element={<SignUp />} />
      <Route path="/tourneyMenu" element={<TourneyMenu />} />
      <Route path="/guestSignUp" element={<GuestSignIn />} />
      <Route path="/lobby" element={<Lobby />} />
      <Route path="/genericBracket" element={<ParentBracket children={<label></label>} />} />
      <Route path="*" element={<NotFound />} />
      <Route path="brackets/twoPlayers" element={<TwoPlayers />} />


    </Routes>
  </BrowserRouter>
)
