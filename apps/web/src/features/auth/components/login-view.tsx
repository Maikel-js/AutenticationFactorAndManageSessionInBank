"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Fingerprint, Shield } from "lucide-react";
import { useLogin } from "@/features/auth/hooks/use-auth";
import { Panel } from "@/components/ui/panel";
import { assertClientRateLimit } from "@/lib/security/client-rate-limit";
import { sanitizeText } from "@/lib/utils/sanitize";

const loginSchema = z.object({
  email: z.string().email(),
  password: z.string().min(8),
  tenantId: z.string().min(3),
  deviceId: z.string().min(8)
});

type LoginValues = z.infer<typeof loginSchema>;

export function LoginView() {
  const router = useRouter();
  const params = useSearchParams();
  const login = useLogin();
  const [errorMessage, setErrorMessage] = useState("");
  const form = useForm<LoginValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "admin@fintech.local",
      password: "P@ssword123!",
      tenantId: "tenant-demo",
      deviceId: "device-web-001"
    }
  });

  const submit = form.handleSubmit(async (values) => {
    setErrorMessage("");

    try
    {
      assertClientRateLimit("login", 5, 60_000);
      const response = await login.mutateAsync({
        email: sanitizeText(values.email),
        password: values.password,
        tenantId: sanitizeText(values.tenantId),
        deviceId: sanitizeText(values.deviceId)
      });

      if (response.requiresMfa && response.mfaTicket) {
        router.push(`/mfa?ticket=${encodeURIComponent(response.mfaTicket)}`);
        return;
      }

      router.push(params.get("next") || "/dashboard");
    }
    catch (error)
    {
      setErrorMessage(error instanceof Error ? error.message : "Authentication failed.");
    }
  });

  return (
    <div className="shell flex min-h-screen items-center py-10">
      <div className="grid w-full gap-6 lg:grid-cols-[1.1fr_0.9fr]">
        <section className="rounded-[32px] border border-white/10 bg-gradient-to-br from-emerald-400/15 via-sky-400/10 to-transparent p-8">
          <p className="text-xs uppercase tracking-[0.35em] text-[var(--primary)]">Bank-grade identity</p>
          <h1 className="mt-4 max-w-xl text-5xl font-semibold leading-tight">
            Defend every login, session and device with fintech-grade controls.
          </h1>
          <div className="mt-8 grid gap-4 md:grid-cols-3">
            {[
              "HttpOnly cookie sessions",
              "TOTP step-up verification",
              "Risk scoring and suspicious activity tracking"
            ].map((item) => (
              <div key={item} className="glass rounded-3xl p-4 text-sm text-[var(--muted)]">
                {item}
              </div>
            ))}
          </div>
        </section>

        <Panel>
          <div className="flex items-center gap-3">
            <div className="rounded-2xl bg-[var(--primary)]/15 p-3 text-[var(--primary)]">
              <Shield className="h-5 w-5" />
            </div>
            <div>
              <h2 className="text-2xl font-semibold">Secure login</h2>
              <p className="text-sm text-[var(--muted)]">Cookies stay on the server boundary. Nothing is written to localStorage.</p>
            </div>
          </div>

          <form onSubmit={submit} className="mt-8 space-y-4">
            {[
              { name: "email", label: "Email", type: "email" },
              { name: "password", label: "Password", type: "password" },
              { name: "tenantId", label: "Tenant", type: "text" },
              { name: "deviceId", label: "Device fingerprint", type: "text" }
            ].map((field) => (
              <label key={field.name} className="block">
                <span className="mb-2 block text-sm text-[var(--muted)]">{field.label}</span>
                <input
                  type={field.type}
                  {...form.register(field.name as keyof LoginValues)}
                  className="w-full rounded-2xl border border-white/10 bg-black/20 px-4 py-3 outline-none transition focus:border-[var(--primary)]"
                />
              </label>
            ))}

            {errorMessage ? <p className="rounded-2xl bg-[var(--danger)]/10 p-3 text-sm text-[var(--danger)]">{errorMessage}</p> : null}

            <button
              type="submit"
              disabled={login.isPending}
              className="w-full rounded-full bg-[var(--primary)] px-5 py-3 font-medium text-slate-950"
            >
              {login.isPending ? "Validating..." : "Sign in"}
            </button>

            <button
              type="button"
              onClick={() => form.setValue("deviceId", `biometric-${crypto.randomUUID()}`)}
              className="inline-flex w-full items-center justify-center gap-2 rounded-full border border-white/10 px-5 py-3 text-sm text-[var(--muted)]"
            >
              <Fingerprint className="h-4 w-4" />
              Simulate trusted biometric device
            </button>
          </form>

          <p className="mt-6 text-sm text-[var(--muted)]">
            Need an account? <Link href="/register" className="text-[var(--primary)]">Register securely</Link>
          </p>
        </Panel>
      </div>
    </div>
  );
}
