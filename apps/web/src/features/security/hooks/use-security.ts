"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { securityApi } from "@/features/security/api/security-api";

export function useSessions() {
  return useQuery({
    queryKey: ["sessions"],
    queryFn: securityApi.listSessions
  });
}

export function useDevices() {
  return useQuery({
    queryKey: ["devices"],
    queryFn: securityApi.listDevices
  });
}

export function useSecurityNotifications() {
  return useQuery({
    queryKey: ["notifications"],
    queryFn: securityApi.listNotifications
  });
}

export function useRevokeSession() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: securityApi.revokeSession,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["sessions"] }),
        queryClient.invalidateQueries({ queryKey: ["devices"] }),
        queryClient.invalidateQueries({ queryKey: ["notifications"] })
      ]);
    }
  });
}
