"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { Bell, LayoutDashboard, Laptop, LogOut, ShieldCheck } from "lucide-react";
import { cn } from "@/lib/utils/cn";
import { useLogout } from "@/features/auth/hooks/use-auth";

const navItems = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/sessions", label: "Sessions", icon: ShieldCheck },
  { href: "/security", label: "Alerts", icon: Bell },
  { href: "/devices", label: "Devices", icon: Laptop }
];

export function AppShell({ children }: Readonly<{ children: React.ReactNode }>) {
  const pathname = usePathname();
  const router = useRouter();
  const logout = useLogout();

  return (
    <div className="pb-10 pt-6">
      <div className="shell">
        <header className="glass flex flex-col gap-4 rounded-[28px] px-6 py-5 md:flex-row md:items-center md:justify-between">
          <div>
            <p className="text-xs uppercase tracking-[0.35em] text-[var(--primary)]">FintechShield</p>
            <h1 className="mt-2 text-2xl font-semibold">Security Operations Console</h1>
          </div>
          <nav className="flex flex-wrap gap-2">
            {navItems.map(({ href, label, icon: Icon }) => (
              <Link
                key={href}
                href={href}
                className={cn(
                  "inline-flex items-center gap-2 rounded-full px-4 py-2 text-sm transition",
                  pathname.startsWith(href)
                    ? "bg-[var(--primary)] text-slate-950"
                    : "bg-white/5 text-[var(--muted)] hover:bg-white/10 hover:text-white"
                )}
              >
                <Icon className="h-4 w-4" />
                {label}
              </Link>
            ))}
            <button
              onClick={async () => {
                await logout.mutateAsync();
                router.replace("/login");
              }}
              className="inline-flex items-center gap-2 rounded-full bg-[var(--danger)]/15 px-4 py-2 text-sm text-[var(--danger)]"
            >
              <LogOut className="h-4 w-4" />
              Logout
            </button>
          </nav>
        </header>
        <main className="mt-6">{children}</main>
      </div>
    </div>
  );
}
