# Git Workflow

## Branches

- `main`: production-ready only
- `develop`: integration and staging
- `feature/<ticket-or-scope>`: isolated changes

## Commit policy

- Use conventional commits such as `feat(auth): add totp verification flow`
- Enable GPG signing locally with `git config --global commit.gpgsign true`
- Open pull requests into `develop`, then promote `develop` to `main`

## Review simulation

Every PR should include:

- Threat and abuse-case notes
- Rollback plan
- Evidence of lint, tests and security scans
