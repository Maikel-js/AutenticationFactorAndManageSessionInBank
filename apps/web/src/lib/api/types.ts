export type UserProfile = {
  userId: string;
  email: string;
  fullName: string;
  tenantId: string;
  mfaEnabled: boolean;
};

export type AuthStatus = {
  authenticated: boolean;
  requiresMfa: boolean;
  sessionId: string;
  riskScore: number;
  mfaTicket: string | null;
  mfaTicketExpiresAtUtc: string | null;
  user: UserProfile;
};

export type SessionRecord = {
  sessionId: string;
  status: string;
  riskScore: number;
  mfaRequired: boolean;
  mfaVerified: boolean;
  expiresAtUtc: string;
  lastSeenAtUtc: string;
  deviceId: string;
  ipAddress: string;
};

export type SecurityNotification = {
  sessionId: string;
  title: string;
  severity: string;
  description: string;
  createdAtUtc: string;
};

export type DeviceRecord = {
  deviceId: string;
  userAgent: string;
  lastIpAddress: string;
  lastSeenAtUtc: string;
  status: string;
  sessionCount: number;
};

export type RegisterResult = {
  userId: string;
  email: string;
  fullName: string;
  mfaEnabled: boolean;
  totpSecret: string | null;
  provisioningUri: string | null;
};
