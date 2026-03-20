"use client";

import { Panel } from "@/components/ui/panel";
import { StatusBadge } from "@/components/ui/status-badge";
import { useRevokeSession, useSessions } from "@/features/security/hooks/use-security";

export function SessionsView() {
  const sessions = useSessions();
  const revokeSession = useRevokeSession();

  return (
    <Panel>
      <div className="flex items-center justify-between">
        <div>
          <p className="text-xs uppercase tracking-[0.35em] text-[var(--primary)]">Active session ledger</p>
          <h2 className="mt-3 text-2xl font-semibold">Monitor and revoke risky sessions</h2>
        </div>
      </div>

      <div className="mt-6 overflow-x-auto">
        <table className="min-w-full text-left text-sm">
          <thead className="text-[var(--muted)]">
            <tr>
              <th className="px-3 py-3">Device</th>
              <th className="px-3 py-3">IP</th>
              <th className="px-3 py-3">Risk</th>
              <th className="px-3 py-3">Status</th>
              <th className="px-3 py-3">Last seen</th>
              <th className="px-3 py-3" />
            </tr>
          </thead>
          <tbody>
            {(sessions.data ?? []).map((session) => (
              <tr key={session.sessionId} className="border-t border-white/5">
                <td className="px-3 py-4">{session.deviceId}</td>
                <td className="px-3 py-4 text-[var(--muted)]">{session.ipAddress}</td>
                <td className="px-3 py-4">{session.riskScore.toFixed(2)}</td>
                <td className="px-3 py-4"><StatusBadge value={session.status} /></td>
                <td className="px-3 py-4 text-[var(--muted)]">{new Date(session.lastSeenAtUtc).toLocaleString()}</td>
                <td className="px-3 py-4 text-right">
                  <button
                    onClick={() => revokeSession.mutate(session.sessionId)}
                    className="rounded-full bg-[var(--danger)]/15 px-4 py-2 text-xs font-medium uppercase tracking-[0.25em] text-[var(--danger)]"
                  >
                    Revoke
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {!sessions.data?.length ? <p className="pt-6 text-sm text-[var(--muted)]">No sessions were returned.</p> : null}
      </div>
    </Panel>
  );
}
