import { createBrowserRouter } from "react-router-dom";
import { AppShell } from "../components/layout/AppShell";
import { DashboardPage } from "../pages/dashboard/DashboardPage";
import { LoginPage } from "../pages/auth/LoginPage";
import { RegisterPage } from "../pages/auth/RegisterPage";
import { AssignmentsPage } from "../pages/assignments/AssignmentsPage";
import { CoursesPage } from "../pages/courses/CoursesPage";
import { ExamsPage } from "../pages/exams/ExamsPage";
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
          <AssignmentsPage />
        </AppShell>
      </ProtectedRoute>
    ),
  },
  {
    path: "/courses",
    element: (
      <ProtectedRoute>
        <AppShell>
          <CoursesPage />
        </AppShell>
      </ProtectedRoute>
    ),
  },
  {
    path: "/exams",
    element: (
      <ProtectedRoute>
        <AppShell>
          <ExamsPage />
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
