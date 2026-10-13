import { createBrowserRouter } from "react-router-dom";
import { AppShell } from "../components/layout/AppShell";
import { DashboardPage } from "../pages/dashboard/DashboardPage";
import { PlaceholderPage } from "../pages/PlaceholderPage";

export const router = createBrowserRouter([
  {
    path: "/",
    element: (
      <AppShell>
        <DashboardPage />
      </AppShell>
    ),
  },
  {
    path: "/assignments",
    element: (
      <AppShell>
        <PlaceholderPage title="Assignments" />
      </AppShell>
    ),
  },
  {
    path: "/study-groups",
    element: (
      <AppShell>
        <PlaceholderPage title="Study Groups" />
      </AppShell>
    ),
  },
  {
    path: "/calendar",
    element: (
      <AppShell>
        <PlaceholderPage title="Calendar" />
      </AppShell>
    ),
  },
  {
    path: "/profile",
    element: (
      <AppShell>
        <PlaceholderPage title="Profile" />
      </AppShell>
    ),
  },
]);
