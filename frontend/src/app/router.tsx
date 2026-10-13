import { createBrowserRouter } from "react-router-dom";
import { AppShell } from "../components/layout/AppShell";
import { DashboardPage } from "../pages/dashboard/DashboardPage";
import { LoginPage } from "../pages/auth/LoginPage";
import { RegisterPage } from "../pages/auth/RegisterPage";
import { PlaceholderPage } from "../pages/PlaceholderPage";
import { ProtectedRoute } from "./ProtectedRoute";

export const router = createBrowserRouter([
  {
    path: "/login",
    element: <LoginPage />,
  },
  {
    path: "/register",
    element: <RegisterPage />,
  },
  {
    path: "/",
    element: (
      <ProtectedRoute>
        <AppShell>
          <DashboardPage />
        </AppShell>
      </ProtectedRoute>
    ),
  },
  {
    path: "/assignments",
    element: (
      <ProtectedRoute>
        <AppShell>
          <PlaceholderPage title="Assignments" />
        </AppShell>
      </ProtectedRoute>
    ),
  },
  {
    path: "/study-groups",
    element: (
      <ProtectedRoute>
        <AppShell>
          <PlaceholderPage title="Study Groups" />
        </AppShell>
      </ProtectedRoute>
    ),
  },
  {
    path: "/calendar",
    element: (
      <ProtectedRoute>
        <AppShell>
          <PlaceholderPage title="Calendar" />
        </AppShell>
      </ProtectedRoute>
    ),
  },
  {
    path: "/profile",
    element: (
      <ProtectedRoute>
        <AppShell>
          <PlaceholderPage title="Profile" />
        </AppShell>
      </ProtectedRoute>
    ),
  },
]);
