import React from "react";
import { Alert, Text, View } from "react-native";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, router } from "expo-router";
import { useMutation } from "@tanstack/react-query";
import { isAxiosError } from "axios";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { TextField } from "../../src/components/TextField";
import { Button } from "../../src/components/Button";
import { registerSchema, type RegisterFormValues } from "../../src/validation/authSchemas";
import { register as registerRequest, requestOtp } from "../../src/api/auth";

function extractErrorMessage(error: unknown, fallback: string): string {
  if (isAxiosError(error)) {
    const serverMessage = (error.response?.data as { message?: string } | undefined)?.message;
    if (error.response?.status === 409) return serverMessage ?? "An account with this email already exists.";
    return serverMessage ?? fallback;
  }
  return fallback;
}

export default function RegisterScreen() {
  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { displayName: "", email: "", password: "", confirmPassword: "" },
  });

  const registerMutation = useMutation({
    mutationFn: (values: RegisterFormValues) =>
      registerRequest({
        email: values.email,
        password: values.password,
        displayName: values.displayName,
      }),
    onSuccess: async (data) => {
      try {
        await requestOtp({ channel: "email", destination: data.email, purpose: "emailVerification" });
      } catch {
        // Registration already succeeded; if the OTP send fails the user can
        // still request a fresh code from the otp screen's resend button.
      }
      router.push({
        pathname: "/(auth)/otp",
        params: { purpose: "emailVerification", channel: "email", destination: data.email },
      });
    },
    onError: (error) => {
      Alert.alert("Couldn't create account", extractErrorMessage(error, "Something went wrong. Please try again."));
    },
  });

  const onSubmit = handleSubmit((values) => registerMutation.mutate(values));

  return (
    <ScreenContainer>
      <View className="mb-8 mt-6">
        <Text className="text-display font-bold text-ink-primary dark:text-ink-primary-dark">
          Create your account
        </Text>
        <Text className="mt-2 text-body text-ink-secondary dark:text-ink-secondary-dark">
          Start learning smarter with StudyVerse AI.
        </Text>
      </View>

      <Controller
        control={control}
        name="displayName"
        render={({ field: { onChange, onBlur, value } }) => (
          <TextField
            label="Full name"
            autoComplete="name"
            textContentType="name"
            value={value}
            onChangeText={onChange}
            onBlur={onBlur}
            error={errors.displayName?.message}
          />
        )}
      />

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
            autoComplete="password-new"
            textContentType="newPassword"
            value={value}
            onChangeText={onChange}
            onBlur={onBlur}
            error={errors.password?.message}
            helperText={
              errors.password ? undefined : "At least 8 characters, with upper, lower, and a number."
            }
          />
        )}
      />

      <Controller
        control={control}
        name="confirmPassword"
        render={({ field: { onChange, onBlur, value } }) => (
          <TextField
            label="Confirm password"
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
        title="Create account"
        onPress={onSubmit}
        loading={registerMutation.isPending}
        style={{ marginTop: 8 }}
      />

      <View className="mt-8 flex-row justify-center">
        <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">
          Already have an account?{" "}
        </Text>
        <Link href="/(auth)/login">
          <Text className="text-body font-semibold text-brand dark:text-brand-light">Log in</Text>
        </Link>
      </View>
    </ScreenContainer>
  );
}
