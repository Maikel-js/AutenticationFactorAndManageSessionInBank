import { cn } from "@/lib/utils/cn";

const tones: Record<string, string> = {
  active: "bg-emerald-400/15 text-emerald-300",
  suspicious: "bg-amber-400/15 text-amber-300",
  revoked: "bg-rose-400/15 text-rose-300",
  expired: "bg-slate-400/15 text-slate-300",
  pendingmfa: "bg-sky-400/15 text-sky-300",
  high: "bg-rose-400/15 text-rose-300",
  medium: "bg-amber-400/15 text-amber-300",
  low: "bg-emerald-400/15 text-emerald-300"
};

export function StatusBadge({ value }: Readonly<{ value: string }>) {
  const key = value.replace(/\s+/g, "").toLowerCase();
  return (
    <span className={cn("rounded-full px-3 py-1 text-xs font-medium uppercase tracking-[0.25em]", tones[key] ?? "bg-white/10 text-white")}>
      {value}
    </span>
  );
}
