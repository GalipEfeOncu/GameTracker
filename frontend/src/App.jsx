import { BrowserRouter, HashRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { UserProvider } from './context/UserContext';
import { PreferencesProvider, usePreferences } from './context/PreferencesContext';
import { ToastProvider } from './context/ToastContext';
import Layout from './components/Layout';
import PopularPage from './pages/PopularPage';
import DiscoverPage from './pages/DiscoverPage';
import LibraryPage from './pages/LibraryPage';
import AiSuggestionPage from './pages/AiSuggestionPage';
import SettingsPage from './pages/SettingsPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import GameDetailsPage from './pages/GameDetailsPage';
import InstalledGamesPage from './pages/InstalledGamesPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 1000 * 60 * 5,
    },
  },
});

function DefaultRedirect() {
  const { startPage } = usePreferences();
  const to = startPage === 'Library' ? '/library' : '/popular';
  return <Navigate to={to} replace />;
}

// Electron/desktop build'inde SPA `file://` üzerinden yüklenir ve tarayıcı tarihi /path
// tabanlı derin bağlantıları çözemez; bu yüzden VITE_DESKTOP=1 bayrağıyla HashRouter kullanılır.
// Web build'i (varsayılan) BrowserRouter'da kalır — URL'ler temiz.
const isDesktopBuild = import.meta.env.VITE_DESKTOP === '1' || import.meta.env.VITE_DESKTOP === 'true';
const Router = isDesktopBuild ? HashRouter : BrowserRouter;

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <UserProvider>
          <PreferencesProvider>
            <Router>
              <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route path="/forgot-password" element={<ForgotPasswordPage />} />

                <Route element={<Layout />}>
                  <Route path="/" element={<DefaultRedirect />} />
                  <Route path="/popular" element={<PopularPage />} />
                  <Route path="/discover" element={<DiscoverPage />} />
                  <Route path="/library" element={<LibraryPage />} />
                  <Route path="/ai" element={<AiSuggestionPage />} />
                  <Route path="/installed" element={<InstalledGamesPage />} />
                  <Route path="/settings" element={<SettingsPage />} />
                  <Route path="/game/:id" element={<GameDetailsPage />} />
                </Route>

                <Route path="*" element={<DefaultRedirect />} />
              </Routes>
            </Router>
          </PreferencesProvider>
        </UserProvider>
      </ToastProvider>
    </QueryClientProvider>
  );
}

export default App;
