import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const PROTECTED_PATHS = ["/dashboard", "/sessions", "/security", "/devices"];
const ACCESS_COOKIES = ["__Host-fsa-at", "fsa-at-dev"];
const REFRESH_COOKIES = ["__Host-fsa-rt", "fsa-rt-dev"];

export function middleware(request: NextRequest) {
  const isProtected = PROTECTED_PATHS.some((path) => request.nextUrl.pathname.startsWith(path));
  const hasSessionCookie =
    ACCESS_COOKIES.some((cookie) => request.cookies.has(cookie)) ||
    REFRESH_COOKIES.some((cookie) => request.cookies.has(cookie));

  if (isProtected && !hasSessionCookie) {
    const url = new URL("/login", request.url);
    url.searchParams.set("next", request.nextUrl.pathname);
    return NextResponse.redirect(url);
  }

  if ((request.nextUrl.pathname === "/login" || request.nextUrl.pathname === "/register") && hasSessionCookie) {
    return NextResponse.redirect(new URL("/dashboard", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/dashboard/:path*", "/sessions/:path*", "/security/:path*", "/devices/:path*", "/login", "/register"]
};
