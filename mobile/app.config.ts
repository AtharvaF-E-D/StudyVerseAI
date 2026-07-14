import type { ConfigContext, ExpoConfig } from "expo/config";
import "dotenv/config";
import * as dotenv from "dotenv";
import * as fs from "fs";
import * as path from "path";

// APP_ENV selects which .env file is loaded: development | staging | production.
// It is set on the shell for local runs (`APP_ENV=staging npx expo start`) and
// via the `env` block of each eas.json build profile for EAS builds.
const APP_ENV = process.env.APP_ENV ?? "development";

const envFile = path.resolve(__dirname, `.env.${APP_ENV}`);
if (fs.existsSync(envFile)) {
  dotenv.config({ path: envFile, override: true });
} else {
  console.warn(
    `[app.config.ts] No env file found at ${envFile} for APP_ENV="${APP_ENV}". Falling back to process.env only.`,
  );
}

const API_BASE_URL = process.env.API_BASE_URL ?? "http://localhost:5000";
const APP_NAME_SUFFIX = process.env.APP_NAME_SUFFIX ?? "";
const BUNDLE_ID_SUFFIX = process.env.BUNDLE_ID_SUFFIX ?? "";
const GOOGLE_IOS_CLIENT_ID = process.env.GOOGLE_IOS_CLIENT_ID ?? "";
const GOOGLE_ANDROID_CLIENT_ID = process.env.GOOGLE_ANDROID_CLIENT_ID ?? "";
const GOOGLE_WEB_CLIENT_ID = process.env.GOOGLE_WEB_CLIENT_ID ?? "";

const BASE_NAME = "StudyVerse AI";
const appName = APP_NAME_SUFFIX ? `${BASE_NAME} ${APP_NAME_SUFFIX}` : BASE_NAME;

const BASE_BUNDLE_ID = "ai.studyverse.app";
const bundleIdentifier = `${BASE_BUNDLE_ID}${BUNDLE_ID_SUFFIX}`;

const BASE_SCHEME = "studyverse";
const scheme = BUNDLE_ID_SUFFIX ? `${BASE_SCHEME}${BUNDLE_ID_SUFFIX.replace(/\./g, "-")}` : BASE_SCHEME;

export default ({ config }: ConfigContext): ExpoConfig => ({
  ...config,
  name: appName,
  slug: "studyverse-ai",
  scheme,
  version: "1.0.0",
  orientation: "portrait",
  icon: "./assets/icon.png",
  userInterfaceStyle: "automatic",
  ios: {
    supportsTablet: true,
    bundleIdentifier,
    infoPlist: {
      CFBundleAllowMixedLocalizations: true,
    },
  },
  android: {
    package: bundleIdentifier,
    adaptiveIcon: {
      backgroundColor: "#E6F4FE",
      foregroundImage: "./assets/android-icon-foreground.png",
      backgroundImage: "./assets/android-icon-background.png",
      monochromeImage: "./assets/android-icon-monochrome.png",
    },
    predictiveBackGestureEnabled: false,
  },
  web: {
    favicon: "./assets/favicon.png",
    bundler: "metro",
  },
  plugins: [
    "expo-router",
    "expo-status-bar",
    "expo-web-browser",
    [
      "expo-splash-screen",
      {
        image: "./assets/splash-icon.png",
        imageWidth: 200,
        resizeMode: "contain",
        backgroundColor: "#FFFFFF",
        dark: {
          image: "./assets/splash-icon.png",
          backgroundColor: "#0B0E14",
        },
      },
    ],
    "expo-secure-store",
    "expo-apple-authentication",
  ],
  extra: {
    apiBaseUrl: API_BASE_URL,
    appEnv: APP_ENV,
    googleIosClientId: GOOGLE_IOS_CLIENT_ID,
    googleAndroidClientId: GOOGLE_ANDROID_CLIENT_ID,
    googleWebClientId: GOOGLE_WEB_CLIENT_ID,
    eas: {
      projectId: process.env.EAS_PROJECT_ID ?? "",
    },
  },
});
