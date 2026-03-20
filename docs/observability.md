# Observability Notes

## Logging

- Use JSON structured logs in production
- Include request id, tenant id, session id and correlation id
- Never log passwords, tokens, OTP codes or full cookie contents

## Alerts to wire

- Repeated MFA failures from the same IP or tenant
- Refresh token reuse events
- Sudden spikes in suspicious sessions
- Elevated 401/403/429 rates

## Sentry

- Frontend: initialize in `app/layout.tsx` or `instrumentation-client.ts`
- API: add Sentry ASP.NET Core SDK with environment-based DSN

## Prometheus

- Prefer private scraping through a sidecar or internal load balancer
- Expose metrics only on internal network paths
- Track request latency, auth success/failure, refresh attempts and revocations
