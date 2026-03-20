"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useVerifyMfa } from "@/features/auth/hooks/use-auth";
import { Panel } from "@/components/ui/panel";

const schema = z.object({
  otpCode: z.string().min(6).max(6)
});

type MfaValues = z.infer<typeof schema>;

export function MfaView() {
  const params = useSearchParams();
  const router = useRouter();
  const verifyMfa = useVerifyMfa();
  const [errorMessage, setErrorMessage] = useState("");
  const ticket = params.get("ticket") ?? "";
  const form = useForm<MfaValues>({
    resolver: zodResolver(schema)
  });

  const submit = form.handleSubmit(async (values) => {
    setErrorMessage("");

    try
    {
      await verifyMfa.mutateAsync({
        mfaTicket: ticket,
        otpCode: values.otpCode
      });
      router.replace("/dashboard");
    }
    catch (error)
    {
      setErrorMessage(error instanceof Error ? error.message : "MFA verification failed.");
    }
  });

  return (
    <div className="shell flex min-h-screen items-center justify-center py-10">
      <Panel className="w-full max-w-xl">
        <p className="text-xs uppercase tracking-[0.35em] text-[var(--info)]">Step-up verification</p>
        <h1 className="mt-4 text-3xl font-semibold">Confirm your TOTP challenge</h1>
        <p className="mt-3 text-sm text-[var(--muted)]">
          Use the code from your authenticator app. If biometric login was simulated, this still enforces OTP before the session is upgraded.
        </p>

        <form onSubmit={submit} className="mt-8 space-y-4">
          <label className="block">
            <span className="mb-2 block text-sm text-[var(--muted)]">One-time password</span>
            <input
              inputMode="numeric"
              maxLength={6}
              {...form.register("otpCode")}
              className="w-full rounded-2xl border border-white/10 bg-black/20 px-4 py-3 text-center text-2xl tracking-[0.4em] outline-none transition focus:border-[var(--primary)]"
            />
          </label>

          {errorMessage ? <p className="rounded-2xl bg-[var(--danger)]/10 p-3 text-sm text-[var(--danger)]">{errorMessage}</p> : null}

          <button type="submit" className="w-full rounded-full bg-[var(--primary)] px-5 py-3 font-medium text-slate-950">
            {verifyMfa.isPending ? "Verifying..." : "Verify and continue"}
          </button>
        </form>
      </Panel>
    </div>
  );
}
