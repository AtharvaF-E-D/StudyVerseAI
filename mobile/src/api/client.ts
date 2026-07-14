import { create as createAxiosInstance, type AxiosError, type InternalAxiosRequestConfig } from "axios";
import Constants from "expo-constants";

import { getOrCreateDeviceId } from "../lib/storage";
import { useAuthStore } from "../stores/authStore";

const rawApiBaseUrl =
  (Constants.expoConfig?.extra?.apiBaseUrl as string | undefined) ?? "http://localhost:5000";

/** Backend base URL, per the API contract: `<API_BASE_URL>/api/v1/auth`. */
export const AUTH_API_BASE_URL = `${rawApiBaseUrl.replace(/\/+$/, "")}/api/v1/auth`;

interface RefreshResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
}

interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

/**
 * Main axios instance used by every function in `./auth.ts`. A request
 * interceptor attaches the current access token (if any); a response
 * interceptor transparently refreshes an expired token exactly once per
 * request and retries, forcing a logout if the refresh itself fails.
 */
export const apiClient = createAxiosInstance({
  baseURL: AUTH_API_BASE_URL,
  timeout: 15000,
  headers: {
    "Content-Type": "application/json",
  },
});

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

apiClient.interceptors.request.use((config) => {
  const { accessToken } = useAuthStore.getState();
  if (accessToken) {
    config.headers.set("Authorization", `Bearer ${accessToken}`);
  }
  return config;
});

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

apiClient.interceptors.response.use(
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
            resolve(apiClient(originalRequest));
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
      return apiClient(originalRequest);
    } catch (refreshError) {
      flushQueue(refreshError, null);
      useAuthStore.getState().clearSession();
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  },
);
