import { createBrowserRouter } from "react-router-dom";
import { AppShell } from "../components/layout/AppShell";
import { DashboardPage } from "../pages/dashboard/DashboardPage";
import { LoginPage } from "../pages/auth/LoginPage";
import { RegisterPage } from "../pages/auth/RegisterPage";
import { AssignmentsPage } from "../pages/assignments/AssignmentsPage";
import { CoursesPage } from "../pages/courses/CoursesPage";
import { ExamsPage } from "../pages/exams/ExamsPage";
import { StudyGroupsPage } from "../pages/study-groups/StudyGroupsPage";
import { StudyGroupDetailPage } from "../pages/study-groups/StudyGroupDetailPage";
import { ScholarshipsPage } from "../pages/scholarships/ScholarshipsPage";
import { CollegeApplicationsPage } from "../pages/college-applications/CollegeApplicationsPage";
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
          <StudyGroupsPage />
        </AppShell>
      </ProtectedRoute>
    ),
  },
  {
    path: "/study-groups/:id",
    element: (
      <ProtectedRoute>
        <AppShell>
          <StudyGroupDetailPage />
        </AppShell>
      </ProtectedRoute>
    ),
  },
  {
    path: "/scholarships",
    element: (
      <ProtectedRoute>
        <AppShell>
          <ScholarshipsPage />
        </AppShell>
      </ProtectedRoute>
    ),
  },
  {
    path: "/college-applications",
    element: (
      <ProtectedRoute>
        <AppShell>
          <CollegeApplicationsPage />
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
