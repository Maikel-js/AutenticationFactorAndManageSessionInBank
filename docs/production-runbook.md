# Production Runbook

## Recommended topology

- Frontend: Vercel on `app.fintechshield.example`
- API: managed container or app service on `api.fintechshield.example`
- Edge: Cloudflare in front of both origins with WAF, bot mitigation and DDoS protection
- Storage: managed relational database and centralized logs

## Required environment variables

### Web

- `NEXT_PUBLIC_API_BASE_URL`
- `SENTRY_DSN`

### API

- `ConnectionStrings__AuthDb`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SecretKey`
- `Security__FrontendOrigin`
- `Security__CookieDomain`
- `Security__RequireHttpsCookies`
- `Sentry__Dsn`

## Hardening checklist

- Enforce TLS 1.2+
- Enable HSTS preload after domain validation
- Keep cookies on `__Host-` prefix whenever the domain model allows it
- Route logs to a centralized SIEM
- Enable alerting on repeated MFA failures, refresh token reuse and suspicious session spikes
- Put the API behind Cloudflare WAF managed rules and rate limiting

## Observability

- Sentry for frontend and API exceptions
- Prometheus scraping `/metrics` when added behind auth/network controls
- Correlate request ids across edge, web and API tiers
