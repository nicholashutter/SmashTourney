
import { createRoot } from 'react-dom/client';
import './index.css';
import HomePage from './pages/HomePage.tsx';
import { BrowserRouter } from "react-router";

createRoot(document.getElementById('root')!).render(
  <BrowserRouter>
    <HomePage />
  </BrowserRouter>
)
