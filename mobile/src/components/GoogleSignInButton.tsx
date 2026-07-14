import React, { useEffect } from "react";
import * as Google from "expo-auth-session/providers/google";

import { Button } from "./Button";

interface GoogleSignInButtonProps {
  iosClientId?: string;
  androidClientId?: string;
  webClientId?: string;
  loading: boolean;
  onIdToken: (idToken: string) => void;
  onError: (message: string) => void;
}

/**
 * Isolated from LoginScreen because `useIdTokenAuthRequest` throws
 * synchronously (inside a useMemo, so on every render) when no client id is
 * configured for the current platform. Mounting this component only when a
 * client id actually exists (see LoginScreen) keeps that throw from taking
 * down the whole login screen before real OAuth credentials are wired up.
 */
export function GoogleSignInButton({
  iosClientId,
  androidClientId,
  webClientId,
  loading,
  onIdToken,
  onError,
}: GoogleSignInButtonProps) {
  const [request, response, promptAsync] = Google.useIdTokenAuthRequest({
    iosClientId,
    androidClientId,
    clientId: webClientId,
  });

  useEffect(() => {
    if (response?.type === "success" && response.params.id_token) {
      onIdToken(response.params.id_token);
    } else if (response?.type === "error") {
      onError(response.error?.message ?? "Please try again.");
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [response]);

  return (
    <Button
      title="Continue with Google"
      variant="secondary"
      onPress={() => promptAsync()}
      loading={loading}
      disabled={!request}
      style={{ marginBottom: 12 }}
    />
  );
}
