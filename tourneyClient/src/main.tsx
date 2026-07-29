
import './index.css';
import { createRoot } from 'react-dom/client';
import { HomePage } from './pages/HomePage.tsx';
import { BrowserRouter, Routes, Route } from "react-router";
import { NotFound } from './pages/NotFound.tsx';
import { CreateTourney } from './pages/CreateTourney.tsx';
import { JoinTourney } from './pages/JoinTourney.tsx';
import { BrowseTourneys } from './pages/BrowseTourneys.tsx';
import { SignUp } from './pages/SignUp.tsx';
import { TourneyMenu } from './pages/TourneyMenu.tsx';
import { Lobby } from './pages/Lobby.tsx';
import { InMatch } from './pages/InMatch.tsx';
import { ShowBracket } from './pages/ShowBracket.tsx';
import { GameDataProvider } from './components/GameIdContext.tsx';
import RequireAuth from './components/RequireAuth.tsx';
import GameSessionGuard from './components/GameSessionGuard.tsx';
import GameRouteGuard from './components/GameRouteGuard.tsx';
import LegacyGameRedirect from './components/LegacyGameRedirect.tsx';
import { inMatchPath, lobbyPath, showBracketPath } from './services/gameRoutes.ts';

// Boots the React application and registers all client routes.
const rootElement = document.getElementById('root');

if (!rootElement)
{
  throw new Error('Root element not found');
}

createRoot(rootElement).render(
  <GameDataProvider>
    <BrowserRouter>
      <GameSessionGuard />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/signUp" element={<SignUp />} />
        <Route path="*" element={<NotFound />} />
        <Route element={<RequireAuth />}>
          <Route path="/createTourney" element={<CreateTourney />} />
          <Route path="/joinTourney" element={<JoinTourney />} />
          <Route path="/browseTourneys" element={<BrowseTourneys />} />
          <Route path="/tourneyMenu" element={<TourneyMenu />} />

          {/* The three in-game screens carry the game id in the path, and sit
              behind a guard that rebuilds the session from it. That is what
              makes reopening the URL a way back into a running tournament
              rather than a way to land on a screen with nothing to show. */}
          <Route element={<GameRouteGuard />}>
            <Route path="/lobby/:gameId" element={<Lobby />} />
            <Route path="/inMatch/:gameId" element={<InMatch />} />
            <Route path="/showBracket/:gameId" element={<ShowBracket />} />
          </Route>

          <Route path="/lobby" element={<LegacyGameRedirect buildPath={lobbyPath} />} />
          <Route path="/inMatch" element={<LegacyGameRedirect buildPath={inMatchPath} />} />
          <Route path="/showBracket" element={<LegacyGameRedirect buildPath={showBracketPath} />} />
        </Route>
      </Routes>
    </BrowserRouter>
  </GameDataProvider>
)
