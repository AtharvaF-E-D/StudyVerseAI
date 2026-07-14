import { useEffect, useState } from "react";
import NetInfo, { type NetInfoState } from "@react-native-community/netinfo";

export interface NetworkStatus {
  /** Whether the device currently has an active network connection (wifi/cellular/etc). */
  isConnected: boolean;
  /** Whether that connection can actually reach the internet, when known. `null` means "not yet determined". */
  isInternetReachable: boolean | null;
  /** The underlying NetInfo connection type, e.g. "wifi" | "cellular" | "none" | "unknown". */
  type: NetInfoState["type"];
}

const initialStatus: NetworkStatus = {
  isConnected: true,
  isInternetReachable: null,
  type: "unknown" as NetInfoState["type"],
};

/**
 * Subscribes to NetInfo and exposes the current connectivity state. Screens
 * can use this to show an offline banner or disable submit buttons that
 * would otherwise just fail with a network error.
 */
export function useNetworkStatus(): NetworkStatus {
  const [status, setStatus] = useState<NetworkStatus>(initialStatus);

  useEffect(() => {
    const unsubscribe = NetInfo.addEventListener((state) => {
      setStatus({
        isConnected: state.isConnected ?? false,
        isInternetReachable: state.isInternetReachable,
        type: state.type,
      });
    });

    NetInfo.fetch().then((state) => {
      setStatus({
        isConnected: state.isConnected ?? false,
        isInternetReachable: state.isInternetReachable,
        type: state.type,
      });
    });

    return () => unsubscribe();
  }, []);

  return status;
}
