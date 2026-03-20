import { apiRequest } from "@/lib/api/http-client";
import type { DeviceRecord, SecurityNotification, SessionRecord } from "@/lib/api/types";

export const securityApi = {
  listSessions: () => apiRequest<SessionRecord[]>("/sessions"),
  revokeSession: (sessionId: string) => apiRequest<void>(`/sessions/${sessionId}`, { method: "DELETE" }),
  listNotifications: () => apiRequest<SecurityNotification[]>("/security/notifications"),
  listDevices: () => apiRequest<DeviceRecord[]>("/security/devices")
};
