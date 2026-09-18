import { Outlet } from 'react-router-dom';
import { PhoneFrame } from './PhoneFrame';
import { BottomNav } from './BottomNav';
import { Toast } from '../feedback/Toast';
import { ServerStatusPopup } from '../feedback/ServerStatusPopup';
import { AccountBadge } from '../auth/AccountBadge';

/** App chrome shared by every route: device frame, page outlet, nav and toast. */
export const AppShell = () => (
  <PhoneFrame
    footer={<BottomNav />}
    overlay={
      <>
        <Toast />
        <ServerStatusPopup />
        <AccountBadge />
      </>
    }
  >
    <Outlet />
  </PhoneFrame>
);
