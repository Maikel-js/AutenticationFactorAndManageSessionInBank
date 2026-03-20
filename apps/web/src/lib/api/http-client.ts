import { sanitizeText } from "@/lib/utils/sanitize";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "https://localhost:5001/api";
let refreshInFlight: Promise<void> | null = null;

function readCookie(name: string) {
  if (typeof document === "undefined") {
    return "";
  }

  const value = document.cookie
    .split("; ")
    .find((cookie) => cookie.startsWith(`${name}=`))
    ?.split("=")[1];

  return value ? decodeURIComponent(value) : "";
}

async function ensureCsrfToken() {
  const token = readCookie("XSRF-TOKEN");
  if (token) {
    return token;
  }

  const response = await fetch(`${API_BASE_URL}/auth/csrf`, {
    method: "GET",
    credentials: "include"
  });

  if (!response.ok) {
    throw new Error("Could not initialize CSRF protection.");
  }

  const payload = (await response.json()) as { csrfToken: string };
  return payload.csrfToken;
}

async function refreshSession() {
  const csrf = await ensureCsrfToken();
  const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
    method: "POST",
    credentials: "include",
    headers: {
      "X-CSRF-TOKEN": csrf
    }
  });

  if (!response.ok) {
    throw new Error("Session refresh failed.");
  }
}

export async function apiRequest<T>(path: string, init?: RequestInit, retry = true): Promise<T> {
  const method = init?.method?.toUpperCase() ?? "GET";
  const headers = new Headers(init?.headers);

  if (method !== "GET") {
    headers.set("X-CSRF-TOKEN", await ensureCsrfToken());
  }

  if (!headers.has("Content-Type") && init?.body) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers,
    credentials: "include"
  });

  if (response.status === 401 && retry) {
    refreshInFlight ??= refreshSession().finally(() => {
      refreshInFlight = null;
    });

    await refreshInFlight;
    return apiRequest<T>(path, init, false);
  }

  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as { detail?: string; title?: string } | null;
    throw new Error(sanitizeText(problem?.detail ?? problem?.title ?? "Unexpected request failure."));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
