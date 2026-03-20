"use client";

import { AlertTriangle, ShieldCheck, Smartphone, UserRound } from "lucide-react";
import { Panel } from "@/components/ui/panel";
import { useCurrentUser } from "@/features/auth/hooks/use-auth";
import { useDevices, useSecurityNotifications, useSessions } from "@/features/security/hooks/use-security";
import { StatusBadge } from "@/components/ui/status-badge";

export function DashboardView() {
  const user = useCurrentUser();
  const sessions = useSessions();
  const devices = useDevices();
  const notifications = useSecurityNotifications();

  return (
    <div className="grid gap-6">
      <div className="grid gap-6 lg:grid-cols-4">
        {[
          { label: "Protected sessions", value: sessions.data?.length ?? 0, icon: ShieldCheck },
          { label: "Known devices", value: devices.data?.length ?? 0, icon: Smartphone },
          { label: "Security alerts", value: notifications.data?.length ?? 0, icon: AlertTriangle },
          { label: "Tenant userscope", value: user.data?.tenantId ?? "n/a", icon: UserRound }
        ].map(({ label, value, icon: Icon }) => (
          <Panel key={label}>
            <div className="flex items-center justify-between">
              <p className="text-sm text-[var(--muted)]">{label}</p>
              <Icon className="h-5 w-5 text-[var(--primary)]" />
            </div>
            <p className="mt-6 text-3xl font-semibold">{value}</p>
          </Panel>
        ))}
      </div>

      <div className="grid gap-6 lg:grid-cols-[1.2fr_0.8fr]">
        <Panel>
          <p className="text-xs uppercase tracking-[0.35em] text-[var(--primary)]">Identity posture</p>
          <h2 className="mt-3 text-2xl font-semibold">{user.data?.fullName ?? "Loading user profile..."}</h2>
          <div className="mt-6 grid gap-4 md:grid-cols-2">
            <div className="rounded-3xl bg-white/5 p-4">
              <p className="text-sm text-[var(--muted)]">Email</p>
              <p className="mt-2 font-medium">{user.data?.email}</p>
            </div>
            <div className="rounded-3xl bg-white/5 p-4">
              <p className="text-sm text-[var(--muted)]">MFA</p>
              <div className="mt-2">{user.data ? <StatusBadge value={user.data.mfaEnabled ? "active" : "disabled"} /> : null}</div>
            </div>
          </div>
        </Panel>

        <Panel>
          <p className="text-xs uppercase tracking-[0.35em] text-[var(--warning)]">Recent signals</p>
          <div className="mt-4 space-y-3">
            {(notifications.data ?? []).slice(0, 3).map((item) => (
              <div key={`${item.sessionId}-${item.createdAtUtc}`} className="rounded-3xl bg-black/20 p-4">
                <div className="flex items-center justify-between">
                  <p className="font-medium">{item.title}</p>
                  <StatusBadge value={item.severity} />
                </div>
                <p className="mt-2 text-sm text-[var(--muted)]">{item.description}</p>
              </div>
            ))}
            {!notifications.data?.length ? <p className="text-sm text-[var(--muted)]">No active alerts.</p> : null}
          </div>
        </Panel>
      </div>
    </div>
  );
}
