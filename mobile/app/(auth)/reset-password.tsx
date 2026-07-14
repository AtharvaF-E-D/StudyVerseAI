import React, { useState } from "react";
import { Text, View } from "react-native";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { router, useLocalSearchParams } from "expo-router";
import { useMutation } from "@tanstack/react-query";
import { isAxiosError } from "axios";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { TextField } from "../../src/components/TextField";
import { Button } from "../../src/components/Button";
import {
  resetPasswordSchema,
  type ResetPasswordFormValues,
} from "../../src/validation/authSchemas";
import { resetPassword } from "../../src/api/auth";

function extractErrorMessage(error: unknown, fallback: string): string {
  if (isAxiosError(error)) {
    const serverMessage = (error.response?.data as { message?: string } | undefined)?.message;
    return serverMessage ?? fallback;
  }
  return fallback;
}

/**
 * Reached via a deep link from the "reset your password" email, e.g.
 * `studyverse://reset-password?token=...&email=...`. Both `token` and
 * `email` are required (they're exactly what `POST /reset-password`
 * needs alongside the new password) — if either is missing the link is
 * treated as invalid rather than guessing at a fallback.
 */
export default function ResetPasswordScreen() {
  const params = useLocalSearchParams<{ token?: string; email?: string }>();
  const token = params.token ?? "";
  const email = params.email ?? "";
  const [isComplete, setIsComplete] = useState(false);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { newPassword: "", confirmPassword: "" },
  });

  const mutation = useMutation({
    mutationFn: (values: ResetPasswordFormValues) =>
      resetPassword({ email, token, newPassword: values.newPassword }),
    onSuccess: () => setIsComplete(true),
  });

  const onSubmit = handleSubmit((values) => mutation.mutate(values));

  if (!token || !email) {
    return (
      <ScreenContainer scrollable={false}>
        <View className="flex-1 items-center justify-center px-4">
          <Text className="mb-3 text-center text-heading text-ink-primary dark:text-ink-primary-dark">
            This link is invalid
          </Text>
          <Text className="text-center text-body text-ink-secondary dark:text-ink-secondary-dark">
            This password reset link is missing or has expired. Request a new one from the login screen.
          </Text>
          <Button
            title="Back to login"
            variant="secondary"
            onPress={() => router.replace("/(auth)/login")}
            style={{ marginTop: 24 }}
          />
        </View>
      </ScreenContainer>
    );
  }

  if (isComplete) {
    return (
      <ScreenContainer scrollable={false}>
        <View className="flex-1 items-center justify-center px-4">
          <Text className="mb-3 text-center text-heading text-ink-primary dark:text-ink-primary-dark">
            Password updated
          </Text>
          <Text className="text-center text-body text-ink-secondary dark:text-ink-secondary-dark">
            Your password has been reset. You can now log in with your new password.
          </Text>
          <Button
            title="Back to login"
            onPress={() => router.replace("/(auth)/login")}
            style={{ marginTop: 24 }}
          />
        </View>
      </ScreenContainer>
    );
  }

  return (
    <ScreenContainer>
      <View className="mb-8 mt-6">
        <Text className="text-display font-bold text-ink-primary dark:text-ink-primary-dark">
          Reset your password
        </Text>
        <Text className="mt-2 text-body text-ink-secondary dark:text-ink-secondary-dark">
          Choose a new password for {email}.
        </Text>
      </View>

      {mutation.isError ? (
        <Text className="mb-4 text-caption text-danger">
          {extractErrorMessage(mutation.error, "Couldn't reset your password. Please try again.")}
        </Text>
      ) : null}

      <Controller
        control={control}
        name="newPassword"
        render={({ field: { onChange, onBlur, value } }) => (
          <TextField
            label="New password"
            isPassword
            autoComplete="password-new"
            textContentType="newPassword"
            value={value}
            onChangeText={onChange}
            onBlur={onBlur}
            error={errors.newPassword?.message}
          />
        )}
      />

      <Controller
        control={control}
        name="confirmPassword"
        render={({ field: { onChange, onBlur, value } }) => (
          <TextField
            label="Confirm new password"
            isPassword
            autoComplete="password-new"
            textContentType="newPassword"
            value={value}
            onChangeText={onChange}
            onBlur={onBlur}
            error={errors.confirmPassword?.message}
          />
        )}
      />

      <Button
        title="Reset password"
        onPress={onSubmit}
        loading={mutation.isPending}
        style={{ marginTop: 8 }}
      />
    </ScreenContainer>
  );
}
