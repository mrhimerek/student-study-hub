import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";

const ACCESS_TOKEN_KEY = "ssh.accessToken";
const REFRESH_TOKEN_KEY = "ssh.refreshToken";

export const tokenStorage = {
  getAccessToken: () => localStorage.getItem(ACCESS_TOKEN_KEY),
  getRefreshToken: () => localStorage.getItem(REFRESH_TOKEN_KEY),
  setTokens: (accessToken: string, refreshToken: string) => {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  },
  clear: () => {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  },
};

const AUTH_SCHEME = "Bearer";

/**
 * Centralized API client. Attaches the JWT access token to every request and, on a 401,
 * attempts a single silent refresh (via /auth/refresh) before retrying the original request.
 * The refresh token itself is returned in the auth response bodies (not an httpOnly cookie,
 * since the API is plain Azure Functions with no server-side session/cookie infrastructure).
 */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "/api",
});

apiClient.interceptors.request.use((config) => {
  const accessToken = tokenStorage.getAccessToken();
  if (accessToken) {
    config.headers.Authorization = `${AUTH_SCHEME} ${accessToken}`;
  }
  return config;
});

interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

interface RefreshResponse {
  accessToken: string;
  refreshToken: string;
}

let refreshPromise: Promise<string> | null = null;

async function refreshAccessToken(): Promise<string> {
  const refreshToken = tokenStorage.getRefreshToken();
  if (!refreshToken) {
    throw new Error("No refresh token available.");
  }

  const { data } = await axios.post<RefreshResponse>(
    `${apiClient.defaults.baseURL}/auth/refresh`,
    { refreshToken },
  );
  tokenStorage.setTokens(data.accessToken, data.refreshToken);
  return data.accessToken;
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetryableRequestConfig | undefined;

    const isAuthEndpoint = originalRequest?.url?.startsWith("/auth/");
    if (error.response?.status === 401 && originalRequest && !originalRequest._retry && !isAuthEndpoint) {
      originalRequest._retry = true;

      try {
        refreshPromise ??= refreshAccessToken();
        const accessToken = await refreshPromise;
        refreshPromise = null;

        originalRequest.headers.Authorization = `${AUTH_SCHEME} ${accessToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        refreshPromise = null;
        tokenStorage.clear();
        window.location.href = "/login";
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  },
);