import { Navigate, Route, Routes } from 'react-router-dom';
import { ToastProvider } from './state/ToastContext';
import { GardenProvider } from './state/GardenContext';
import { AppShell } from './components/layout/AppShell';
import { ROUTES } from './routes/paths';
import { TodayPage } from './features/today/TodayPage';
import { PlantsPage } from './features/plants/PlantsPage';
import { PlantProfilePage } from './features/plants/PlantProfilePage';
import { RepotPage } from './features/plants/RepotPage';
import { AddPlantPage } from './features/add/AddPlantPage';
import { CalendarPage } from './features/calendar/CalendarPage';
import { GroupsPage } from './features/groups/GroupsPage';

export const App = () => (
  <ToastProvider>
    <GardenProvider>
      <Routes>
        <Route element={<AppShell />}>
          <Route path={ROUTES.today} element={<TodayPage />} />
          <Route path={ROUTES.plants} element={<PlantsPage />} />
          <Route path={ROUTES.plant} element={<PlantProfilePage />} />
          <Route path={ROUTES.repot} element={<RepotPage />} />
          <Route path={ROUTES.add} element={<AddPlantPage />} />
          <Route path={ROUTES.calendar} element={<CalendarPage />} />
          <Route path={ROUTES.groups} element={<GroupsPage />} />
          <Route path="*" element={<Navigate to={ROUTES.today} replace />} />
        </Route>
      </Routes>
    </GardenProvider>
  </ToastProvider>
);
