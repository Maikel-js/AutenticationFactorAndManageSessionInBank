"use client";

export default function GlobalError({
  error,
  reset
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <html lang="en">
      <body className="flex min-h-screen items-center justify-center p-6">
        <div className="glass w-full max-w-lg rounded-3xl p-8">
          <p className="text-sm uppercase tracking-[0.3em] text-[var(--danger)]">Platform fault</p>
          <h1 className="mt-4 text-3xl font-semibold">We blocked the failing view.</h1>
          <p className="mt-3 text-sm text-[var(--muted)]">
            An error was captured without exposing sensitive details. Retry the screen or return to the dashboard.
          </p>
          <p className="mt-4 rounded-2xl bg-black/20 p-3 text-xs text-[var(--muted)]">{error.message}</p>
          <button
            onClick={reset}
            className="mt-6 rounded-full bg-[var(--primary)] px-5 py-3 text-sm font-medium text-slate-950"
          >
            Retry
          </button>
        </div>
      </body>
    </html>
  );
}
