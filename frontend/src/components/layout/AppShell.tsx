import { type ReactNode } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../../features/auth/AuthContext";

const NAV_ITEMS = [
  { to: "/", label: "Home", icon: "🏠" },
  { to: "/assignments", label: "Tasks", icon: "📋" },
  { to: "/study-groups", label: "Groups", icon: "👥" },
  { to: "/calendar", label: "Calendar", icon: "📅" },
  { to: "/profile", label: "Profile", icon: "👤" },
];

export function AppShell({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="mx-auto flex min-h-screen max-w-md flex-col bg-gray-50 pb-16">
      <header className="sticky top-0 z-10 flex items-center justify-between border-b border-gray-200 bg-white px-4 py-3">
        <h1 className="text-lg font-semibold text-indigo-600">Student Study Hub</h1>
        {user && (
          <button
            type="button"
            onClick={handleLogout}
            className="text-xs font-medium text-gray-400 hover:text-gray-600"
            title={`Log out ${user.firstName}`}
          >
            Log out
          </button>
        )}
      </header>

      <main className="flex-1 px-4 py-4">{children}</main>

      <nav className="fixed inset-x-0 bottom-0 z-10 mx-auto flex max-w-md items-center justify-around border-t border-gray-200 bg-white py-2">
        {NAV_ITEMS.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.to === "/"}
            className={({ isActive }) =>
              `flex flex-col items-center text-xs ${
                isActive ? "text-indigo-600" : "text-gray-400"
              }`
            }
          >
            <span className="text-lg">{item.icon}</span>
            {item.label}
          </NavLink>
        ))}
      </nav>
    </div>
  );
}
