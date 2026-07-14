import React, { useEffect, useState } from "react";
import { Alert, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";
import { useMutation } from "@tanstack/react-query";
import { isAxiosError } from "axios";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { OtpInput } from "../../src/components/OtpInput";
import { Button } from "../../src/components/Button";
import { otpSchema } from "../../src/validation/authSchemas";
import { requestOtp, verifyOtp, type OtpChannel, type OtpPurpose } from "../../src/api/auth";
import { useAuthStore } from "../../src/stores/authStore";
import { getDeviceName, getOrCreateDeviceId } from "../../src/lib/storage";

const RESEND_COOLDOWN_SECONDS = 30;

function extractErrorMessage(error: unknown, fallback: string): string {
  if (isAxiosError(error)) {
    const serverMessage = (error.response?.data as { message?: string } | undefined)?.message;
    return serverMessage ?? fallback;
  }
  return fallback;
}

export default function OtpScreen() {
  const params = useLocalSearchParams<{ purpose?: string; channel?: string; destination?: string }>();
  const setSession = useAuthStore((s) => s.setSession);

  const purpose = (params.purpose === "login" ? "login" : "email_verification") as OtpPurpose;
  const channel = (params.channel === "phone" ? "phone" : "email") as OtpChannel;
  const destination = params.destination ?? "";

  const [code, setCode] = useState("");
  const [validationError, setValidationError] = useState<string | undefined>(undefined);
  const [cooldown, setCooldown] = useState(RESEND_COOLDOWN_SECONDS);

  useEffect(() => {
    if (cooldown <= 0) return;
    const interval = setInterval(() => {
      setCooldown((prev) => Math.max(0, prev - 1));
    }, 1000);
    return () => clearInterval(interval);
  }, [cooldown]);

  const verifyMutation = useMutation({
    mutationFn: () =>
      verifyOtp({
        channel,
        destination,
        code,
        purpose,
        deviceId: getOrCreateDeviceId(),
        deviceName: getDeviceName(),
      }),
    onSuccess: (session) => {
      setSession(session);
      router.replace("/(app)");
    },
    onError: (error) => {
      Alert.alert("Verification failed", extractErrorMessage(error, "That code didn't work. Please try again."));
    },
  });

  const resendMutation = useMutation({
    mutationFn: () => requestOtp({ channel, destination, purpose }),
    onSuccess: () => {
      setCooldown(RESEND_COOLDOWN_SECONDS);
      Alert.alert("Code sent", `We've sent a new code to ${destination}.`);
    },
    onError: (error) => {
      Alert.alert("Couldn't resend code", extractErrorMessage(error, "Please try again in a moment."));
    },
  });

  function handleVerify() {
    const result = otpSchema.safeParse({ code });
    if (!result.success) {
      setValidationError(result.error.issues[0]?.message ?? "Enter the 6-digit code");
      return;
    }
    setValidationError(undefined);
    verifyMutation.mutate();
  }

  if (!destination) {
    return (
      <ScreenContainer scrollable={false}>
        <View className="flex-1 items-center justify-center px-4">
          <Text className="text-center text-body text-ink-secondary dark:text-ink-secondary-dark">
            This verification link is missing information. Please go back and try again.
          </Text>
          <Button
            title="Back to login"
            variant="secondary"
            onPress={() => router.replace("/(auth)/login")}
            style={{ marginTop: 16 }}
          />
        </View>
      </ScreenContainer>
    );
  }

  return (
    <ScreenContainer>
      <View className="mb-8 mt-6">
        <Text className="text-display font-bold text-ink-primary dark:text-ink-primary-dark">
          {purpose === "login" ? "Enter your code" : "Verify your email"}
        </Text>
        <Text className="mt-2 text-body text-ink-secondary dark:text-ink-secondary-dark">
          We sent a 6-digit code to {destination}.
        </Text>
      </View>

      <OtpInput value={code} onChange={setCode} error={validationError} autoFocus />
      {validationError ? (
        <Text className="mt-2 text-caption text-danger">{validationError}</Text>
      ) : null}

      <Button
        title="Verify"
        onPress={handleVerify}
        loading={verifyMutation.isPending}
        style={{ marginTop: 24 }}
      />

      <View className="mt-6 flex-row justify-center">
        <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">
          Didn&apos;t get a code?{" "}
        </Text>
        {cooldown > 0 ? (
          <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">
            Resend in {cooldown}s
          </Text>
        ) : (
          <Text
            className="text-body font-semibold text-brand dark:text-brand-light"
            onPress={() => !resendMutation.isPending && resendMutation.mutate()}
          >
            Resend code
          </Text>
        )}
      </View>
    </ScreenContainer>
  );
}
