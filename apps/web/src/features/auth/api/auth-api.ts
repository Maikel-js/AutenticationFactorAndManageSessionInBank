import { apiRequest } from "@/lib/api/http-client";
import type { AuthStatus, RegisterResult, UserProfile } from "@/lib/api/types";

export const authApi = {
  login: (payload: { email: string; password: string; deviceId: string; tenantId: string }) =>
    apiRequest<AuthStatus>("/auth/login", {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  register: (payload: {
    email: string;
    fullName: string;
    password: string;
    tenantId: string;
    enableMfa: boolean;
  }) =>
    apiRequest<RegisterResult>("/auth/register", {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  verifyMfa: (payload: { mfaTicket: string; otpCode: string }) =>
    apiRequest<AuthStatus>("/auth/mfa/verify", {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  me: () => apiRequest<UserProfile>("/me"),
  logout: () => apiRequest<void>("/auth/logout", { method: "POST" })
};
