import { Platform } from "react-native";
import * as Device from "expo-device";
import { createMMKV, type MMKV } from "react-native-mmkv";

/**
 * Single MMKV (v4, Nitro-backed) instance backing all local persistence in
 * the app: auth session, device id, cached preferences, etc. MMKV relies on
 * the OS-level app sandbox for on-disk protection by default; call
 * `storage.encrypt(key)` with a key held in `expo-secure-store` (iOS
 * Keychain / Android Keystore) if a future phase needs storage encrypted
 * at rest as well.
 */
export const storage: MMKV = createMMKV({
  id: "studyverse-storage",
});

/**
 * Plain get/set/delete/clear wrapper used throughout the app so callers
 * never touch the MMKV instance directly. Values are JSON-serialized so
 * any serializable shape can be stored and retrieved with full typing.
 */
export const appStorage = {
  getString(key: string): string | undefined {
    return storage.getString(key);
  },

  setString(key: string, value: string): void {
    storage.set(key, value);
  },

  getObject<T>(key: string): T | undefined {
    const raw = storage.getString(key);
    if (raw === undefined) return undefined;
    try {
      return JSON.parse(raw) as T;
    } catch {
      return undefined;
    }
  },

  setObject<T>(key: string, value: T): void {
    storage.set(key, JSON.stringify(value));
  },

  getBoolean(key: string): boolean | undefined {
    return storage.getBoolean(key);
  },

  setBoolean(key: string, value: boolean): void {
    storage.set(key, value);
  },

  delete(key: string): void {
    storage.remove(key);
  },

  clear(): void {
    storage.clearAll();
  },

  contains(key: string): boolean {
    return storage.contains(key);
  },
};

const DEVICE_ID_KEY = "studyverse.deviceId";

/**
 * Returns a stable per-device id, generating and persisting a UUID the
 * first time it's requested. This is used as the `deviceId` field for
 * every auth endpoint (login, refresh, logout, otp/verify, google, apple)
 * so the backend can track/revoke sessions per device.
 */
export function getOrCreateDeviceId(): string {
  const existing = appStorage.getString(DEVICE_ID_KEY);
  if (existing) return existing;

  const generated = generateUuidV4();
  appStorage.setString(DEVICE_ID_KEY, generated);
  return generated;
}

/**
 * Human-readable device name sent as `deviceName` on every auth endpoint
 * that creates or refreshes a session, so the backend can show the user a
 * friendly "signed in on ..." list. Falls back to a generic platform label
 * when the native model name isn't available (e.g. some Android OEMs, web).
 */
export function getDeviceName(): string {
  if (Device.modelName) return Device.modelName;
  return Platform.OS === "ios" ? "iOS Device" : Platform.OS === "android" ? "Android Device" : "Unknown Device";
}

function generateUuidV4(): string {
  // RFC 4122 v4 UUID using Math.random(). Good enough for a local device
  // identifier (not used for cryptographic purposes).
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}
