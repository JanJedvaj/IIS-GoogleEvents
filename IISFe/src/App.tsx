import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { RequireAuth } from './auth/RequireAuth';
import { RequireRole } from './auth/RequireRole';
import { Layout } from './layout/Layout';
import { LoginPage } from './pages/login/LoginPage';
import { RegisterPage } from './pages/register/RegisterPage';

const EventsPage = lazy(() =>
  import('./pages/events/EventsPage').then((m) => ({ default: m.EventsPage })),
);
const EventFormPage = lazy(() =>
  import('./pages/events/EventFormPage').then((m) => ({ default: m.EventFormPage })),
);
const GraphQlPage = lazy(() =>
  import('./pages/graphql/GraphQlPage').then((m) => ({ default: m.GraphQlPage })),
);
const ImportPage = lazy(() =>
  import('./pages/import/ImportPage').then((m) => ({ default: m.ImportPage })),
);
const SoapPage = lazy(() =>
  import('./pages/soap/SoapPage').then((m) => ({ default: m.SoapPage })),
);
const WeatherPage = lazy(() =>
  import('./pages/weather/WeatherPage').then((m) => ({ default: m.WeatherPage })),
);

function App() {
  return (
    <Suspense fallback={null}>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        <Route element={<RequireAuth />}>
          <Route element={<Layout />}>
            <Route index element={<Navigate to="/events" replace />} />
            <Route path="/events" element={<EventsPage />} />
            <Route path="/graphql" element={<GraphQlPage />} />
            <Route path="/soap" element={<SoapPage />} />
            <Route path="/weather" element={<WeatherPage />} />

            <Route element={<RequireRole minRole="Admin" />}>
              <Route path="/events/new" element={<EventFormPage />} />
              <Route path="/events/:id/edit" element={<EventFormPage />} />
              <Route path="/import" element={<ImportPage />} />
            </Route>
          </Route>
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  );
}

export default App;
