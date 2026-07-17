import { create as createAxiosInstance, type AxiosError, type InternalAxiosRequestConfig } from "axios";
import Constants from "expo-constants";

import { getOrCreateDeviceId } from "../lib/storage";
import { useAuthStore } from "../stores/authStore";

const rawApiBaseUrl =
  (Constants.expoConfig?.extra?.apiBaseUrl as string | undefined) ?? "http://localhost:5000";
const trimmedApiBaseUrl = rawApiBaseUrl.replace(/\/+$/, "");

/** Backend base URL, per the API contract: `<API_BASE_URL>/api/v1/auth`. */
export const AUTH_API_BASE_URL = `${trimmedApiBaseUrl}/api/v1/auth`;

/**
 * Backend base URL for every non-auth controller (dashboard, notifications,
 * leaderboard, and future feature areas): `<API_BASE_URL>/api/v1`. Auth
 * endpoints live one level deeper (see `AUTH_API_BASE_URL`) and keep their
 * own client instance so `./auth.ts` is unaffected by this.
 */
export const API_BASE_URL = `${trimmedApiBaseUrl}/api/v1`;

interface RefreshResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
}

interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

/**
 * A second, interceptor-free client used only to call `/refresh` itself so
 * that a failing refresh request never re-triggers the response
 * interceptor below (which would otherwise recurse).
 */
const refreshClient = createAxiosInstance({
  baseURL: AUTH_API_BASE_URL,
  timeout: 15000,
  headers: {
    "Content-Type": "application/json",
  },
});

/**
 * Pending requests queued while a token refresh triggered by *any*
 * authenticated client is in flight. Shared at module scope (rather than
 * per-client) because there is only ever one refresh token per session — a
 * 401 from the dashboard client and a 401 from the auth client should both
 * wait on the same in-flight refresh instead of racing separate ones.
 */
interface PendingRequest {
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}

let isRefreshing = false;
let pendingRequests: PendingRequest[] = [];

function flushQueue(error: unknown, token: string | null) {
  for (const request of pendingRequests) {
    if (token) {
      request.resolve(token);
    } else {
      request.reject(error);
    }
  }
  pendingRequests = [];
}

/**
 * Builds an axios instance scoped to `baseURL` with the shared
 * attach-token / refresh-and-retry-on-401 behavior every authenticated
 * client in this app needs. `apiClient` (auth-scoped) and `coreApiClient`
 * (everything else) are both just this factory applied to a different base
 * path — see `AUTH_API_BASE_URL` / `API_BASE_URL` above.
 */
function createAuthenticatedClient(baseURL: string) {
  const client = createAxiosInstance({
    baseURL,
    timeout: 15000,
    headers: {
      "Content-Type": "application/json",
    },
  });

  client.interceptors.request.use((config) => {
    const { accessToken } = useAuthStore.getState();
    if (accessToken) {
      config.headers.set("Authorization", `Bearer ${accessToken}`);
    }
    return config;
  });

  client.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
      const originalRequest = error.config as RetryableRequestConfig | undefined;

      const status = error.response?.status;
      const isUnauthorized = status === 401;
      const isRefreshCall = originalRequest?.url?.includes("/refresh");

      if (!originalRequest || !isUnauthorized || originalRequest._retry || isRefreshCall) {
        return Promise.reject(error);
      }

      const { refreshToken } = useAuthStore.getState();
      if (!refreshToken) {
        useAuthStore.getState().clearSession();
        return Promise.reject(error);
      }

      originalRequest._retry = true;

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          pendingRequests.push({
            resolve: (token) => {
              originalRequest.headers.set("Authorization", `Bearer ${token}`);
              resolve(client(originalRequest));
            },
            reject,
          });
        });
      }

      isRefreshing = true;
      try {
        const deviceId = getOrCreateDeviceId();
        const { data } = await refreshClient.post<RefreshResponse>("/refresh", {
          refreshToken,
          deviceId,
        });

        const currentUser = useAuthStore.getState().user;
        if (currentUser) {
          useAuthStore.getState().setSession({
            accessToken: data.accessToken,
            refreshToken: data.refreshToken,
            accessTokenExpiresAtUtc: data.accessTokenExpiresAtUtc,
            user: currentUser,
          });
        }

        flushQueue(null, data.accessToken);

        originalRequest.headers.set("Authorization", `Bearer ${data.accessToken}`);
        return client(originalRequest);
      } catch (refreshError) {
        flushQueue(refreshError, null);
        useAuthStore.getState().clearSession();
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    },
  );

  return client;
}

/** Main axios instance used by every function in `./auth.ts`. */
export const apiClient = createAuthenticatedClient(AUTH_API_BASE_URL);

/**
 * Axios instance for every non-auth controller (dashboard, notifications,
 * leaderboard, ...). Same token-attach / refresh-and-retry behavior as
 * `apiClient`, just scoped to `API_BASE_URL` instead of `AUTH_API_BASE_URL`.
 */
export const coreApiClient = createAuthenticatedClient(API_BASE_URL);
