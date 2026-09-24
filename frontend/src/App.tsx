import { Navigate, Route, Routes } from 'react-router-dom';
import { ToastProvider } from './state/ToastContext';
import { AuthProvider, useAuth } from './state/AuthContext';
import { GardenProvider } from './state/GardenContext';
import { CareDateProvider } from './state/CareDateContext';
import { AppShell } from './components/layout/AppShell';
import { PhoneFrame } from './components/layout/PhoneFrame';
import { Toast } from './components/feedback/Toast';
import { ServerStatusPopup } from './components/feedback/ServerStatusPopup';
import { SignInPage } from './features/auth/SignInPage';
import { ROUTES } from './routes/paths';
import { TodayPage } from './features/today/TodayPage';
import { PlantsPage } from './features/plants/PlantsPage';
import { PlantProfilePage } from './features/plants/PlantProfilePage';
import { AddPlantPage } from './features/add/AddPlantPage';
import { CalendarPage } from './features/calendar/CalendarPage';
import { GroupsPage } from './features/groups/GroupsPage';

const Gate = () => {
  const { status } = useAuth();

  if (status === 'checking') {
    return <PhoneFrame overlay={<ServerStatusPopup />} />;
  }

  if (status === 'anonymous') {
    return (
      <PhoneFrame
        overlay={
          <>
            <Toast />
            <ServerStatusPopup />
          </>
        }
      >
        <SignInPage />
      </PhoneFrame>
    );
  }

  return (
    <GardenProvider>
      <CareDateProvider>
        <Routes>
          <Route element={<AppShell />}>
            <Route path={ROUTES.today} element={<TodayPage />} />
            <Route path={ROUTES.plants} element={<PlantsPage />} />
            <Route path={ROUTES.plant} element={<PlantProfilePage />} />
            <Route path={ROUTES.add} element={<AddPlantPage />} />
            <Route path={ROUTES.calendar} element={<CalendarPage />} />
            <Route path={ROUTES.groups} element={<GroupsPage />} />
            <Route path="*" element={<Navigate to={ROUTES.today} replace />} />
          </Route>
        </Routes>
      </CareDateProvider>
    </GardenProvider>
  );
};

export const App = () => (
  <ToastProvider>
    <AuthProvider>
      <Gate />
    </AuthProvider>
  </ToastProvider>
);
