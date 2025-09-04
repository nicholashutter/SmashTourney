
import { createRoot } from 'react-dom/client';
import './index.css';
import HomePage from './pages/HomePage.tsx';
import { BrowserRouter, Routes, Route } from "react-router";
import GuestSignIn from './pages/GuestSignUp.tsx';
import CreateTourney from './pages/CreateTourney.tsx';
import JoinTourney from './pages/JoinTourney.tsx';
import SignUp from './pages/SignUp.tsx';
import TourneyMenu from './pages/TourneyMenu.tsx';

createRoot(document.getElementById('root')!).render(
  <BrowserRouter>
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/createTourney" element={<CreateTourney />} />
      <Route path="/joinTourney" element={<JoinTourney />} />
      <Route path="/signUp" element={<SignUp />} />
      <Route path="/tourneyMenu" element={<TourneyMenu />} />
      <Route path="/guestSignUp" element={<GuestSignIn />} />
    </Routes>
  </BrowserRouter>
)
