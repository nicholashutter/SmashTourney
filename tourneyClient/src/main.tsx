
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
import GenericBracket from './pages/GenericBracket.tsx';
import WaitingForPlayers from './pages/WaitingForPlayers.tsx';

createRoot(document.getElementById('root')!).render(
  <BrowserRouter>
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/createTourney" element={<CreateTourney />} />
      <Route path="/joinTourney" element={<JoinTourney />} />
      <Route path="/signUp" element={<SignUp />} />
      <Route path="/tourneyMenu" element={<TourneyMenu />} />
      <Route path="/guestSignUp" element={<GuestSignIn />} />
      <Route path="/lobby" element={<WaitingForPlayers/>}/>
      <Route path="/genericBracket" element={<GenericBracket children={<label></label>} />} />
      <Route path="*" element={<NotFound />} />
    </Routes>
  </BrowserRouter>
)
