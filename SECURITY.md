# Security Policy

## Supported branches

- `main`: production
- `develop`: pre-production integration
- `feature/*`: short-lived work branches

## Reporting

Report vulnerabilities privately to the platform security owner. Do not open public issues for exploitable findings.

## Baseline controls

- Secrets only through environment variables or managed secret stores
- HttpOnly cookies for session material
- CSRF token required for state-changing requests
- MFA and suspicious session telemetry enabled
- Secret scanning and CodeQL enforced in CI
