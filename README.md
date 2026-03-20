# FintechShield Platform

Plataforma full-stack para autenticacion robusta, gestion segura de sesiones y controles operativos orientados a fintech.

## Contenido del repositorio

- `AutenticationFactorAndManageSessionInBank.Api`
  API ASP.NET Core con cookies HttpOnly, JWT, refresh rotation, MFA TOTP, sesiones, dispositivos y alertas.
- `AutenticationFactorAndManageSessionInBank.Application`
  Casos de uso, contratos y servicios de autenticacion.
- `AutenticationFactorAndManageSessionInBank.Domain`
  Entidades y reglas centrales del dominio.
- `AutenticationFactorAndManageSessionInBank.Infrastructure`
  EF Core, SQLite, hashing, TOTP, JWT, seed y configuracion de seguridad.
- `apps/web`
  Frontend Next.js App Router con TypeScript, React Query y UI tipo fintech.
- `.github/workflows`
  CI/CD, escaneo de secretos, CodeQL y despliegues.
- `docs`
  Runbooks y flujo de Git/produccion.

## Backend: controles implementados

- JWT firmado y validado por cookie segura
- Refresh token con rotacion y deteccion de reuse
- Cookies HttpOnly para access y refresh tokens
- Proteccion CSRF con cookie + header `X-CSRF-TOKEN`
- MFA TOTP con challenge previo a elevar la sesion
- Gestion de sesiones por usuario y dispositivo
- Señales de riesgo y alertas de seguridad
- Rate limiting server-side
- Headers de seguridad:
  CSP, HSTS, X-Frame-Options, X-Content-Type-Options, Referrer-Policy y Permissions-Policy

## Frontend: capacidades

- Next.js App Router con TypeScript
- Estructura modular:
  `auth`, `dashboard`, `security`
- React Query para estado de servidor
- Cliente HTTP seguro con:
  `credentials: include`, CSRF bootstrap y refresh automatico
- Pantallas:
  login, registro, MFA, dashboard, sesiones, alertas y dispositivos
- Sin uso de `localStorage` para tokens
- Sanitizacion basica de inputs y rate limit UX para login

## Flujo de autenticacion

1. El frontend solicita `/api/auth/csrf`
2. El backend emite cookie `XSRF-TOKEN`
3. Login o registro envian `X-CSRF-TOKEN`
4. Si el usuario tiene MFA:
   el backend responde con `mfaTicket`
5. El frontend verifica OTP en `/api/auth/mfa/verify`
6. El backend emite cookies de acceso y refresh
7. El cliente renueva automaticamente la sesion con `/api/auth/refresh`

## Usuario semilla

- Email: `admin@fintech.local`
- Password: `P@ssword123!`
- Tenant: `tenant-demo`
- MFA secret TOTP: `JBSWY3DPEHPK3PXP`

Importa ese secreto en tu app autenticadora para generar el OTP del flujo MFA.

## Variables de entorno

### Web

Ver [apps/web/.env.example](c:\Users\pasante3\Desktop\EACM\AutenticationFactorAndManageSessionInBank\apps\web\.env.example)

### API

Configurar en `appsettings.*` o como variables:

- `ConnectionStrings__AuthDb`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SecretKey`
- `Security__FrontendOrigin`
- `Security__CookieDomain`
- `Security__RequireHttpsCookies`

## Git y seguridad del repositorio

- Branches esperadas:
  `main`, `develop`, `feature/*`
- Hooks:
  lint, typecheck, escaneo ligero de seguridad
- Pull request template y `CODEOWNERS`
- Secret scanning y CodeQL en CI
- No subir secretos ni `.env`

## CI/CD

- `ci.yml`
  lint, build web, restore/build API y security scans
- `deploy-web.yml`
  despliegue a Vercel
- `deploy-api.yml`
  despliegue de API a servicio cloud administrado

## Produccion recomendada

- Frontend:
  Vercel
- API:
  App Service, ECS/Fargate, Cloud Run o equivalente
- Edge:
  Cloudflare con WAF y mitigacion DDoS
- Observabilidad:
  Sentry, logs centralizados y Prometheus segun runbook

## Limitaciones de esta entrega local

El repositorio queda preparado para produccion, pero en esta sesion no pude ejecutar despliegues reales ni instalar dependencias del frontend porque:

- `node` y `npm` no estan instalados en este entorno local
- `dotnet restore/build` sigue afectado por locks de NuGet del entorno

## Siguientes pasos al abrir el entorno completo

1. Instalar Node.js 22 LTS o superior compatible con Next.js 16
2. Ejecutar `npm install` en la raiz
3. Ejecutar `dotnet restore --configfile NuGet.Config`
4. Ejecutar `dotnet build`
5. Ejecutar `npm --workspace @fintechshield/web run build`
6. Configurar secretos CI/CD y desplegar
