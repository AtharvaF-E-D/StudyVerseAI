import React, { forwardRef, useState } from "react";
import { Pressable, Text, TextInput, View, type TextInputProps } from "react-native";

export interface TextFieldProps extends TextInputProps {
  label: string;
  error?: string;
  helperText?: string;
  /** Renders a show/hide toggle and manages secureTextEntry internally. */
  isPassword?: boolean;
  containerClassName?: string;
}

/**
 * Labeled text input with error/helper text, used by every auth form.
 * Forwards its ref so it composes with React Hook Form's `register`
 * (uncontrolled) or `Controller` (controlled) without extra wrapping.
 */
export const TextField = forwardRef<TextInput, TextFieldProps>(function TextField(
  { label, error, helperText, isPassword = false, containerClassName = "", secureTextEntry, ...inputProps },
  ref,
) {
  const [isSecure, setIsSecure] = useState(isPassword);

  return (
    <View className={["mb-4", containerClassName].join(" ")}>
      <Text className="mb-1.5 text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
        {label}
      </Text>
      <View
        className={[
          "flex-row items-center rounded-md border bg-surface px-3.5 dark:bg-surface-dark",
          error ? "border-danger" : "border-border dark:border-border-dark",
        ].join(" ")}
      >
        <TextInput
          ref={ref}
          className="flex-1 py-3 text-body text-ink-primary dark:text-ink-primary-dark"
          placeholderTextColor="#9AA1B1"
          secureTextEntry={isPassword ? isSecure : secureTextEntry}
          autoCapitalize="none"
          autoCorrect={false}
          accessibilityLabel={label}
          {...inputProps}
        />
        {isPassword ? (
          <Pressable
            onPress={() => setIsSecure((prev) => !prev)}
            hitSlop={8}
            accessibilityRole="button"
            accessibilityLabel={isSecure ? "Show password" : "Hide password"}
          >
            <Text className="text-caption font-medium text-brand dark:text-brand-light">
              {isSecure ? "Show" : "Hide"}
            </Text>
          </Pressable>
        ) : null}
      </View>
      {error ? (
        <Text className="mt-1 text-caption text-danger">{error}</Text>
      ) : helperText ? (
        <Text className="mt-1 text-caption text-ink-secondary dark:text-ink-secondary-dark">
          {helperText}
        </Text>
      ) : null}
    </View>
  );
});
