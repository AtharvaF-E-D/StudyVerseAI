import { QueryClient } from "@tanstack/react-query";
import { isAxiosError } from "axios";

/**
 * Shared React Query client. Retries are deliberately conservative: auth
 * failures (401/403) and other 4xx client errors should never be retried
 * automatically (retrying a bad request or bad credentials just wastes a
 * round trip and delays showing the user useful feedback), while
 * transient network/5xx failures get a couple of quick retries.
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60 * 1000, // 1 minute
      gcTime: 5 * 60 * 1000, // 5 minutes
      retry: (failureCount, error) => {
        if (isAxiosError(error)) {
          const status = error.response?.status;
          if (status !== undefined && status >= 400 && status < 500) {
            return false;
          }
        }
        return failureCount < 2;
      },
      refetchOnWindowFocus: false,
    },
    mutations: {
      retry: false,
    },
  },
});
