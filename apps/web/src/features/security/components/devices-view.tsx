"use client";

import { Panel } from "@/components/ui/panel";
import { StatusBadge } from "@/components/ui/status-badge";
import { useDevices } from "@/features/security/hooks/use-security";

export function DevicesView() {
  const devices = useDevices();

  return (
    <Panel>
      <p className="text-xs uppercase tracking-[0.35em] text-[var(--info)]">Device governance</p>
      <h2 className="mt-3 text-2xl font-semibold">Trusted and observed devices</h2>
      <div className="mt-6 grid gap-4">
        {(devices.data ?? []).map((device) => (
          <div key={device.deviceId} className="rounded-3xl border border-white/5 bg-black/20 p-5">
            <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
              <div>
                <p className="font-medium">{device.deviceId}</p>
                <p className="mt-2 text-sm text-[var(--muted)]">{device.userAgent}</p>
              </div>
              <StatusBadge value={device.status} />
            </div>
            <div className="mt-4 grid gap-3 text-sm text-[var(--muted)] md:grid-cols-3">
              <p>Last IP: {device.lastIpAddress}</p>
              <p>Last seen: {new Date(device.lastSeenAtUtc).toLocaleString()}</p>
              <p>Sessions observed: {device.sessionCount}</p>
            </div>
          </div>
        ))}
        {!devices.data?.length ? <p className="text-sm text-[var(--muted)]">No managed devices yet.</p> : null}
      </div>
    </Panel>
  );
}
