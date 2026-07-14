import React, { useEffect, useState } from "react";
import { Alert, Platform, Text, View } from "react-native";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, router } from "expo-router";
import { useMutation } from "@tanstack/react-query";
import * as WebBrowser from "expo-web-browser";
import * as AppleAuthentication from "expo-apple-authentication";
import Constants from "expo-constants";
import { isAxiosError } from "axios";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { TextField } from "../../src/components/TextField";
import { Button } from "../../src/components/Button";
import { GoogleSignInButton } from "../../src/components/GoogleSignInButton";
import { useTheme } from "../../src/theme/ThemeProvider";
import { loginSchema, type LoginFormValues } from "../../src/validation/authSchemas";
import {
  login as loginRequest,
  requestOtp,
  signInWithApple,
  signInWithGoogle,
} from "../../src/api/auth";
import { useAuthStore } from "../../src/stores/authStore";
import { getDeviceName, getOrCreateDeviceId } from "../../src/lib/storage";

WebBrowser.maybeCompleteAuthSession();

const googleIosClientId = (Constants.expoConfig?.extra?.googleIosClientId as string | undefined) || undefined;
const googleAndroidClientId =
  (Constants.expoConfig?.extra?.googleAndroidClientId as string | undefined) || undefined;
const googleWebClientId = (Constants.expoConfig?.extra?.googleWebClientId as string | undefined) || undefined;

// expo-auth-session's Google hook throws synchronously if no client id is
// configured for the current platform, so only mount it once one exists.
const hasGoogleClientId = Platform.select({
  ios: !!googleIosClientId,
  android: !!googleAndroidClientId,
  default: !!googleWebClientId,
});

function extractErrorMessage(error: unknown, fallback: string): string {
  if (isAxiosError(error)) {
    const status = error.response?.status;
    const serverMessage = (error.response?.data as { message?: string } | undefined)?.message;
    if (status === 401) return serverMessage ?? "Incorrect email or password.";
    if (status === 423) return serverMessage ?? "Your account is temporarily locked. Try again later.";
    return serverMessage ?? fallback;
  }
  return fallback;
}

export default function LoginScreen() {
  const { scheme } = useTheme();
  const setSession = useAuthStore((s) => s.setSession);
  const [isAppleAvailable, setIsAppleAvailable] = useState(false);
  const [isOtpRequestPending, setIsOtpRequestPending] = useState(false);

  const {
    control,
    handleSubmit,
    trigger,
    getValues,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });

  useEffect(() => {
    if (Platform.OS !== "ios") return;
    AppleAuthentication.isAvailableAsync().then(setIsAppleAvailable);
  }, []);

  const loginMutation = useMutation({
    mutationFn: (values: LoginFormValues) =>
      loginRequest({
        email: values.email,
        password: values.password,
        deviceId: getOrCreateDeviceId(),
        deviceName: getDeviceName(),
      }),
    onSuccess: (session) => {
      setSession(session);
      router.replace("/(app)");
    },
    onError: (error) => {
      Alert.alert("Couldn't log in", extractErrorMessage(error, "Something went wrong. Please try again."));
    },
  });

  const googleMutation = useMutation({
    mutationFn: (idToken: string) =>
      signInWithGoogle({
        idToken,
        deviceId: getOrCreateDeviceId(),
        deviceName: getDeviceName(),
      }),
    onSuccess: (session) => {
      setSession(session);
      router.replace("/(app)");
    },
    onError: (error) => {
      Alert.alert("Google sign-in failed", extractErrorMessage(error, "Please try again."));
    },
  });

  const appleMutation = useMutation({
    mutationFn: (payload: { identityToken: string; fullName?: string }) =>
      signInWithApple({
        identityToken: payload.identityToken,
        fullName: payload.fullName,
        deviceId: getOrCreateDeviceId(),
        deviceName: getDeviceName(),
      }),
    onSuccess: (session) => {
      setSession(session);
      router.replace("/(app)");
    },
    onError: (error) => {
      Alert.alert("Apple sign-in failed", extractErrorMessage(error, "Please try again."));
    },
  });

  async function handleAppleSignIn() {
    try {
      const credential = await AppleAuthentication.signInAsync({
        requestedScopes: [
          AppleAuthentication.AppleAuthenticationScope.FULL_NAME,
          AppleAuthentication.AppleAuthenticationScope.EMAIL,
        ],
      });
      if (!credential.identityToken) {
        Alert.alert("Apple sign-in failed", "No identity token was returned.");
        return;
      }
      const fullName = credential.fullName
        ? AppleAuthentication.formatFullName(credential.fullName)
        : undefined;
      appleMutation.mutate({ identityToken: credential.identityToken, fullName: fullName || undefined });
    } catch (error) {
      const code = (error as { code?: string }).code;
      if (code === "ERR_REQUEST_CANCELED") return;
      Alert.alert("Apple sign-in failed", "Please try again.");
    }
  }

  async function handleOtpLogin() {
    const isEmailValid = await trigger("email");
    if (!isEmailValid) return;

    const email = getValues("email");
    setIsOtpRequestPending(true);
    try {
      await requestOtp({ channel: "email", destination: email, purpose: "login" });
      router.push({
        pathname: "/(auth)/otp",
        params: { purpose: "login", channel: "email", destination: email },
      });
    } catch (error) {
      Alert.alert("Couldn't send code", extractErrorMessage(error, "Please try again."));
    } finally {
      setIsOtpRequestPending(false);
    }
  }

  const onSubmit = handleSubmit((values) => loginMutation.mutate(values));

  return (
    <ScreenContainer>
      <View className="mb-8 mt-6">
        <Text className="text-display font-bold text-ink-primary dark:text-ink-primary-dark">
          Welcome back
        </Text>
        <Text className="mt-2 text-body text-ink-secondary dark:text-ink-secondary-dark">
          Log in to keep up with your study plan.
        </Text>
      </View>

      <Controller
        control={control}
        name="email"
        render={({ field: { onChange, onBlur, value } }) => (
          <TextField
            label="Email"
            keyboardType="email-address"
            autoComplete="email"
            textContentType="emailAddress"
            value={value}
            onChangeText={onChange}
            onBlur={onBlur}
            error={errors.email?.message}
          />
        )}
      />

      <Controller
        control={control}
        name="password"
        render={({ field: { onChange, onBlur, value } }) => (
          <TextField
            label="Password"
            isPassword
            autoComplete="password"
            textContentType="password"
            value={value}
            onChangeText={onChange}
            onBlur={onBlur}
            error={errors.password?.message}
          />
        )}
      />

      <Link href="/(auth)/forgot-password" asChild>
        <Text className="mb-6 self-end text-caption font-medium text-brand dark:text-brand-light">
          Forgot password?
        </Text>
      </Link>

      <Button title="Log in" onPress={onSubmit} loading={loginMutation.isPending} />

      <Button
        title="Log in with a one-time code"
        variant="ghost"
        onPress={handleOtpLogin}
        loading={isOtpRequestPending}
        style={{ marginTop: 8 }}
      />

      <View className="my-7 flex-row items-center">
        <View className="h-px flex-1 bg-border dark:bg-border-dark" />
        <Text className="mx-3 text-caption text-ink-secondary dark:text-ink-secondary-dark">
          or continue with
        </Text>
        <View className="h-px flex-1 bg-border dark:bg-border-dark" />
      </View>

      {hasGoogleClientId ? (
        <GoogleSignInButton
          iosClientId={googleIosClientId}
          androidClientId={googleAndroidClientId}
          webClientId={googleWebClientId}
          loading={googleMutation.isPending}
          onIdToken={(idToken) => googleMutation.mutate(idToken)}
          onError={(message) => Alert.alert("Google sign-in failed", message)}
        />
      ) : null}

      {isAppleAvailable ? (
        <AppleAuthentication.AppleAuthenticationButton
          buttonType={AppleAuthentication.AppleAuthenticationButtonType.SIGN_IN}
          buttonStyle={
            scheme === "dark"
              ? AppleAuthentication.AppleAuthenticationButtonStyle.WHITE
              : AppleAuthentication.AppleAuthenticationButtonStyle.BLACK
          }
          cornerRadius={10}
          style={{ height: 50, width: "100%" }}
          onPress={handleAppleSignIn}
        />
      ) : null}

      <View className="mt-8 flex-row justify-center">
        <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">
          Don&apos;t have an account?{" "}
        </Text>
        <Link href="/(auth)/register">
          <Text className="text-body font-semibold text-brand dark:text-brand-light">Sign up</Text>
        </Link>
      </View>
    </ScreenContainer>
  );
}
