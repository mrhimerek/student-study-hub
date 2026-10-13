import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { tokenStorage } from "../../lib/apiClient";
import { authApi } from "./api";
import type { AuthUser, LoginRequest, RegisterRequest } from "./types";

const USER_STORAGE_KEY = "ssh.user";

interface AuthContextValue {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isInitializing: boolean;
  login: (payload: LoginRequest) => Promise<void>;
  register: (payload: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function readStoredUser(): AuthUser | null {
  const raw = localStorage.getItem(USER_STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isInitializing, setIsInitializing] = useState(true);

  // There's no GET /auth/me endpoint yet, so on reload we optimistically restore the
  // last-known user alongside the stored tokens; the apiClient's 401 interceptor handles
  // silently refreshing (or logging out) once the access token actually expires.
  useEffect(() => {
    if (tokenStorage.getAccessToken()) {
      setUser(readStoredUser());
    }
    setIsInitializing(false);
  }, []);

  const persistSession = (nextUser: AuthUser, accessToken: string, refreshToken: string) => {
    tokenStorage.setTokens(accessToken, refreshToken);
    localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(nextUser));
    setUser(nextUser);
  };

  const login = async (payload: LoginRequest) => {
    const result = await authApi.login(payload);
    persistSession(result.user, result.accessToken, result.refreshToken);
  };

  const register = async (payload: RegisterRequest) => {
    const result = await authApi.register(payload);
    persistSession(result.user, result.accessToken, result.refreshToken);
  };

  const logout = async () => {
    const refreshToken = tokenStorage.getRefreshToken();
    try {
      if (refreshToken) {
        await authApi.logout(refreshToken);
      }
    } finally {
      tokenStorage.clear();
      localStorage.removeItem(USER_STORAGE_KEY);
      setUser(null);
    }
  };

  const value = useMemo<AuthContextValue>(
    () => ({ user, isAuthenticated: user !== null, isInitializing, login, register, logout }),
    [user, isInitializing],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return ctx;
}
