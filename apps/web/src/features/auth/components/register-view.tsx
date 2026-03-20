"use client";

import Link from "next/link";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRegister } from "@/features/auth/hooks/use-auth";
import { Panel } from "@/components/ui/panel";
import { sanitizeText } from "@/lib/utils/sanitize";

const registerSchema = z.object({
  email: z.string().email(),
  fullName: z.string().min(3),
  password: z.string().min(12),
  tenantId: z.string().min(3),
  enableMfa: z.boolean()
});

type RegisterValues = z.infer<typeof registerSchema>;

export function RegisterView() {
  const registerMutation = useRegister();
  const [message, setMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const form = useForm<RegisterValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      enableMfa: true
    }
  });

  const submit = form.handleSubmit(async (values) => {
    setMessage("");
    setErrorMessage("");

    try
    {
      const response = await registerMutation.mutateAsync({
        email: sanitizeText(values.email),
        fullName: sanitizeText(values.fullName),
        password: values.password,
        tenantId: sanitizeText(values.tenantId),
        enableMfa: values.enableMfa
      });

      setMessage(
        response.mfaEnabled && response.totpSecret
          ? `Registration complete. Save the TOTP secret securely: ${response.totpSecret}`
          : "Registration complete. You can now sign in."
      );
      form.reset({ enableMfa: true });
    }
    catch (error)
    {
      setErrorMessage(error instanceof Error ? error.message : "Registration failed.");
    }
  });

  return (
    <div className="shell flex min-h-screen items-center justify-center py-10">
      <Panel className="w-full max-w-2xl">
        <h1 className="text-3xl font-semibold">Create secure access</h1>
        <p className="mt-2 text-sm text-[var(--muted)]">Provision users with MFA from day one and keep the tenant boundary explicit.</p>

        <form onSubmit={submit} className="mt-8 grid gap-4 md:grid-cols-2">
          {[
            { name: "fullName", label: "Full name", type: "text" },
            { name: "email", label: "Email", type: "email" },
            { name: "password", label: "Password", type: "password" },
            { name: "tenantId", label: "Tenant", type: "text" }
          ].map((field) => (
            <label key={field.name} className="block">
              <span className="mb-2 block text-sm text-[var(--muted)]">{field.label}</span>
              <input
                type={field.type}
                {...form.register(field.name as keyof RegisterValues)}
                className="w-full rounded-2xl border border-white/10 bg-black/20 px-4 py-3 outline-none transition focus:border-[var(--primary)]"
              />
            </label>
          ))}

          <label className="flex items-center gap-3 rounded-2xl border border-white/10 bg-black/10 px-4 py-3 md:col-span-2">
            <input type="checkbox" {...form.register("enableMfa")} />
            <span className="text-sm text-[var(--muted)]">Enable TOTP MFA during registration</span>
          </label>

          {message ? <p className="rounded-2xl bg-emerald-400/10 p-3 text-sm text-emerald-300 md:col-span-2">{message}</p> : null}
          {errorMessage ? <p className="rounded-2xl bg-[var(--danger)]/10 p-3 text-sm text-[var(--danger)] md:col-span-2">{errorMessage}</p> : null}

          <button type="submit" className="rounded-full bg-[var(--primary)] px-5 py-3 font-medium text-slate-950 md:col-span-2">
            {registerMutation.isPending ? "Provisioning..." : "Create account"}
          </button>
        </form>

        <p className="mt-6 text-sm text-[var(--muted)]">
          Already onboarded? <Link href="/login" className="text-[var(--primary)]">Return to login</Link>
        </p>
      </Panel>
    </div>
  );
}
