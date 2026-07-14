import React, { useState } from "react";
import { Text, View } from "react-native";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { router } from "expo-router";
import { useMutation } from "@tanstack/react-query";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { TextField } from "../../src/components/TextField";
import { Button } from "../../src/components/Button";
import {
  forgotPasswordSchema,
  type ForgotPasswordFormValues,
} from "../../src/validation/authSchemas";
import { forgotPassword } from "../../src/api/auth";

export default function ForgotPasswordScreen() {
  const [submittedEmail, setSubmittedEmail] = useState<string | null>(null);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: { email: "" },
  });

  const mutation = useMutation({
    mutationFn: (values: ForgotPasswordFormValues) => forgotPassword(values),
    onSuccess: (_data, variables) => {
      setSubmittedEmail(variables.email);
    },
    onError: () => {
      // Deliberately shown as success below too: we never reveal whether an
      // account exists for a given email, to avoid leaking account presence.
      // The backend is expected to always return 202 for this endpoint.
    },
  });

  const onSubmit = handleSubmit((values) => mutation.mutate(values));

  if (submittedEmail) {
    return (
      <ScreenContainer scrollable={false}>
        <View className="flex-1 items-center justify-center px-4">
          <Text className="mb-3 text-center text-heading text-ink-primary dark:text-ink-primary-dark">
            Check your inbox
          </Text>
          <Text className="text-center text-body text-ink-secondary dark:text-ink-secondary-dark">
            If an account exists for {submittedEmail}, we&apos;ve sent instructions to reset your password.
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

  return (
    <ScreenContainer>
      <View className="mb-8 mt-6">
        <Text className="text-display font-bold text-ink-primary dark:text-ink-primary-dark">
          Forgot password?
        </Text>
        <Text className="mt-2 text-body text-ink-secondary dark:text-ink-secondary-dark">
          Enter your email and we&apos;ll send you instructions to reset it.
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

      <Button
        title="Send reset instructions"
        onPress={onSubmit}
        loading={mutation.isPending}
        style={{ marginTop: 8 }}
      />
    </ScreenContainer>
  );
}
