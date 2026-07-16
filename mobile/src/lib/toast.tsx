import React, { createContext, useCallback, useContext, useMemo, useRef, useState } from "react";
import { View } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import { Toast, type ToastVariant } from "../components/Toast";

export type { ToastVariant };

interface ToastState {
  id: number;
  message: string;
  variant: ToastVariant;
}

export interface ToastContextValue {
  show: (message: string, variant?: ToastVariant) => void;
}

const ToastContext = createContext<ToastContextValue | undefined>(undefined);

const AUTO_DISMISS_MS = 3000;

/**
 * Provides `useToast()` app-wide and renders the single active toast
 * anchored above the bottom safe area, overlaying whatever screen is
 * mounted below it. A new `show()` call while a toast is visible replaces
 * it immediately — this app only ever needs one attention-grabbing message
 * on screen at a time, so a full stacked queue would be unused complexity.
 * Mount this once, high in the tree (see `app/_layout.tsx`), inside the
 * `SafeAreaProvider` so `useSafeAreaInsets` resolves correctly.
 */
export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toast, setToast] = useState<ToastState | null>(null);
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const nextId = useRef(0);
  const insets = useSafeAreaInsets();

  const dismiss = useCallback(() => setToast(null), []);

  const show = useCallback((message: string, variant: ToastVariant = "neutral") => {
    if (timeoutRef.current) clearTimeout(timeoutRef.current);
    nextId.current += 1;
    setToast({ id: nextId.current, message, variant });
    timeoutRef.current = setTimeout(() => setToast(null), AUTO_DISMISS_MS);
  }, []);

  const value = useMemo<ToastContextValue>(() => ({ show }), [show]);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <View
        pointerEvents="box-none"
        style={{
          position: "absolute",
          left: 0,
          right: 0,
          bottom: insets.bottom + 16,
          alignItems: "center",
        }}
      >
        {toast ? <Toast key={toast.id} message={toast.message} variant={toast.variant} onDismiss={dismiss} /> : null}
      </View>
    </ToastContext.Provider>
  );
}

export function useToast(): ToastContextValue {
  const ctx = useContext(ToastContext);
  if (!ctx) {
    throw new Error("useToast must be used within a ToastProvider");
  }
  return ctx;
}
