import React, { useMemo, useRef } from "react";
import { TextInput, View, type NativeSyntheticEvent, type TextInputKeyPressEventData } from "react-native";

export interface OtpInputProps {
  length?: number;
  value: string;
  onChange: (value: string) => void;
  error?: string;
  autoFocus?: boolean;
}

/**
 * A row of `length` single-digit boxes backed by one string value. Typing a
 * digit advances focus to the next box; backspace on an empty box moves
 * focus back and clears the previous digit. Pasting (or autofill from an
 * SMS/email OTP suggestion) a full code into any box distributes it across
 * all boxes.
 */
export function OtpInput({ length = 6, value, onChange, error, autoFocus = false }: OtpInputProps) {
  const inputRefs = useRef<(TextInput | null)[]>([]);
  const digits = useMemo(() => {
    const chars = value.split("").slice(0, length);
    return Array.from({ length }, (_, i) => chars[i] ?? "");
  }, [value, length]);

  function setDigitAt(index: number, char: string) {
    const next = digits.slice();
    next[index] = char;
    onChange(next.join("").slice(0, length));
  }

  function handleChangeText(text: string, index: number) {
    // Handles both a single keystroke and a multi-character paste/autofill.
    const sanitized = text.replace(/[^0-9]/g, "");

    if (sanitized.length > 1) {
      onChange(sanitized.slice(0, length));
      const nextIndex = Math.min(sanitized.length, length) - 1;
      inputRefs.current[nextIndex]?.focus();
      return;
    }

    setDigitAt(index, sanitized);
    if (sanitized && index < length - 1) {
      inputRefs.current[index + 1]?.focus();
    }
  }

  function handleKeyPress(e: NativeSyntheticEvent<TextInputKeyPressEventData>, index: number) {
    if (e.nativeEvent.key === "Backspace" && !digits[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
      setDigitAt(index - 1, "");
    }
  }

  return (
    <View>
      <View className="flex-row justify-between">
        {digits.map((digit, index) => (
          <TextInput
            key={index}
            ref={(el) => {
              inputRefs.current[index] = el;
            }}
            value={digit}
            onChangeText={(text) => handleChangeText(text, index)}
            onKeyPress={(e) => handleKeyPress(e, index)}
            keyboardType="number-pad"
            textContentType="oneTimeCode"
            autoComplete="sms-otp"
            maxLength={length}
            autoFocus={autoFocus && index === 0}
            accessibilityLabel={`Digit ${index + 1} of ${length}`}
            className={[
              "h-14 w-12 rounded-md border text-center text-heading text-ink-primary dark:text-ink-primary-dark bg-surface dark:bg-surface-dark",
              error ? "border-danger" : "border-border dark:border-border-dark",
            ].join(" ")}
          />
        ))}
      </View>
    </View>
  );
}
