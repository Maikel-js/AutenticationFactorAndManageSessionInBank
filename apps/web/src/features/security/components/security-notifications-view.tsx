"use client";

import { Panel } from "@/components/ui/panel";
import { StatusBadge } from "@/components/ui/status-badge";
import { useSecurityNotifications } from "@/features/security/hooks/use-security";

export function SecurityNotificationsView() {
  const notifications = useSecurityNotifications();

  return (
    <Panel>
      <p className="text-xs uppercase tracking-[0.35em] text-[var(--warning)]">Threat intelligence</p>
      <h2 className="mt-3 text-2xl font-semibold">Security notifications</h2>
      <div className="mt-6 space-y-4">
        {(notifications.data ?? []).map((notification) => (
          <div key={`${notification.sessionId}-${notification.createdAtUtc}`} className="rounded-3xl border border-white/5 bg-black/20 p-5">
            <div className="flex items-center justify-between gap-4">
              <div>
                <p className="font-medium">{notification.title}</p>
                <p className="mt-2 text-sm text-[var(--muted)]">{notification.description}</p>
              </div>
              <StatusBadge value={notification.severity} />
            </div>
            <p className="mt-3 text-xs uppercase tracking-[0.25em] text-[var(--muted)]">
              {new Date(notification.createdAtUtc).toLocaleString()}
            </p>
          </div>
        ))}
        {!notifications.data?.length ? <p className="text-sm text-[var(--muted)]">No notifications are active.</p> : null}
      </div>
    </Panel>
  );
}
