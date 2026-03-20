const buckets = new Map<string, number[]>();

export function assertClientRateLimit(key: string, limit: number, windowMs: number) {
  const now = Date.now();
  const existing = buckets.get(key) ?? [];
  const fresh = existing.filter((timestamp) => now - timestamp < windowMs);

  if (fresh.length >= limit) {
    throw new Error("Too many attempts. Please wait and try again.");
  }

  fresh.push(now);
  buckets.set(key, fresh);
}
