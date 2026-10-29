# Security Policy

## Supported versions

This is a reference implementation. Security fixes are applied to the current
`main` branch and the latest `0.1.x` line. Older snapshots are not supported.

## Reporting a vulnerability

Please report security issues **privately**:

1. Use GitHub's **Report a vulnerability** flow on the repository's Security tab
   (private security advisory), or
2. Contact the repository owner directly through their GitHub profile.

Do not open a public issue for a vulnerability. Include:

- the affected module and endpoint;
- reproduction steps and expected impact;
- the version or commit you tested;
- any suggested remediation.

You can expect an initial response within a few business days. Please allow time
for a fix before public disclosure.

## Scope

The repository is a reference implementation. The following are
**known, intentional limitations**, not vulnerabilities:

- **Demo authentication** — every demo API accepts `X-Demo-Role`,
  `X-Demo-Email`, `X-Demo-Member` and `X-Branch-Id` headers to make screenshots
  and tests deterministic. This bypasses real authentication by design. Never
  enable the demo authentication handler in production.
- **Demo signing keys** — identity demo configuration contains development-only
  JWT material. Replace it with managed secrets in any real deployment.
- **SQLite** — services use local SQLite files for zero-setup demos. They are not
  hardened for multi-tenant or production workloads.
- **Simulated payments** — no card data is collected and no payment provider is
  called.
- **Simulated notifications** — no real e-mail or push provider is called.
- **No rate limiting or WAF** — the demo hosts are not internet-facing.

## Deployment checklist

If you adapt this code for a real environment:

- Replace demo authentication with an OIDC/OAuth provider and validate issuer,
  audience and signing keys.
- Remove the demo header middleware and the seeded accounts.
- Store connection strings, JWT keys and provider credentials in a secret
  manager, never in source or `.env` files committed to Git.
- Use a production database with migrations, backups and least-privilege
  credentials.
- Restrict CORS to the real client origins.
- Add rate limiting, request-size limits and audit logging at the edge.
- Serve over TLS and set HSTS, CSP and other security headers.
- Review authorization policies per endpoint, not only per controller.
- Run dependency and container scanning in CI.

## Repository hygiene

The root `.gitignore` excludes build output, SQLite databases, environment files,
package artifacts and editor state. Do not commit:

- access tokens, PATs, private keys or `.env` files;
- production connection strings;
- customer or member data;
- generated databases or screenshot fixtures containing personal data.
