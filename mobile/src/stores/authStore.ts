import { create } from "zustand";
import { createJSONStorage, persist, type StateStorage } from "zustand/middleware";

import { appStorage } from "../lib/storage";

export interface AuthUser {
  id: string;
  email: string;
  displayName: string;
  emailVerified: boolean;
}

export interface AuthSession {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
  user: AuthUser;
}

export interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  accessTokenExpiresAtUtc: string | null;
  user: AuthUser | null;
  /** True once the persisted session (if any) has been read from MMKV. Routing decisions must wait for this. */
  isHydrated: boolean;
  /** Stores a freshly-obtained session (login, register+auto-login, otp/verify, google, apple, refresh). */
  setSession: (session: AuthSession) => void;
  /** Patches the current user, e.g. after `/me` or `/verify-email` confirms emailVerified changed. */
  updateUser: (user: AuthUser) => void;
  /** Clears the session, e.g. on explicit logout or an unrecoverable 401 refresh failure. */
  clearSession: () => void;
  /** Marks hydration complete. Called automatically once MMKV has been read; exposed for tests/manual re-hydration. */
  setHydrated: (hydrated: boolean) => void;
  /** Forces a re-read of the persisted session from MMKV. */
  hydrate: () => void;
}

const mmkvStorage: StateStorage = {
  getItem: (name) => appStorage.getString(name) ?? null,
  setItem: (name, value) => appStorage.setString(name, value),
  removeItem: (name) => appStorage.delete(name),
};

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      accessTokenExpiresAtUtc: null,
      user: null,
      isHydrated: false,

      setSession: (session) =>
        set({
          accessToken: session.accessToken,
          refreshToken: session.refreshToken,
          accessTokenExpiresAtUtc: session.accessTokenExpiresAtUtc,
          user: session.user,
        }),

      updateUser: (user) => set({ user }),

      clearSession: () =>
        set({
          accessToken: null,
          refreshToken: null,
          accessTokenExpiresAtUtc: null,
          user: null,
        }),

      setHydrated: (hydrated) => set({ isHydrated: hydrated }),

      hydrate: () => {
        void useAuthStore.persist.rehydrate();
      },
    }),
    {
      name: "studyverse-auth",
      storage: createJSONStorage(() => mmkvStorage),
      partialize: (state) => ({
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        accessTokenExpiresAtUtc: state.accessTokenExpiresAtUtc,
        user: state.user,
      }),
      onRehydrateStorage: () => (state) => {
        // Runs after the persisted slice (if any) has been merged into the
        // live store, whether or not anything was actually found in MMKV.
        state?.setHydrated(true);
      },
    },
  ),
);

/** Convenience selector for gating navigation on auth state. */
export function useIsAuthenticated(): boolean {
  return useAuthStore((s) => Boolean(s.accessToken && s.user));
}
