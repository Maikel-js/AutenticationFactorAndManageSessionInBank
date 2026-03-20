"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { authApi } from "@/features/auth/api/auth-api";

export function useCurrentUser() {
  return useQuery({
    queryKey: ["me"],
    queryFn: authApi.me
  });
}

export function useLogin() {
  return useMutation({
    mutationFn: authApi.login
  });
}

export function useRegister() {
  return useMutation({
    mutationFn: authApi.register
  });
}

export function useVerifyMfa() {
  return useMutation({
    mutationFn: authApi.verifyMfa
  });
}

export function useLogout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: authApi.logout,
    onSuccess: async () => {
      await queryClient.invalidateQueries();
      queryClient.clear();
    }
  });
}
