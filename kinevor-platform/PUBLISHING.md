# Publishing kinevor-platform

> **Monorepo note:** Kinevor is published as one showcase monorepo. These steps are optional and apply only if you extract this folder into a standalone repository or publish its packages to a registry.


This platform publishes the shared packages before any module release.

## Published artifacts

| Ecosystem | Package |
| --- | --- |
| NuGet | `Kinevor.Platform.Domain` |
| NuGet | `Kinevor.Platform.Application` |
| NuGet | `Kinevor.Platform.Api` |
| npm | `@softwaredeveloper98/kinevor-platform-web` |

## Prerequisites

- The monorepo exists at `https://github.com/SoftwareDeveloper98/kinevor`.
- Actions are enabled with read and write workflow permissions.
- The NuGet and npm versions are `0.1.0` in `backend/Directory.Build.props`,
  the Angular package and `CHANGELOG.md`.

The platform release workflow publishes with the repository `GITHUB_TOKEN`; it
does not need the cross-repository `KINEVOR_PACKAGES_TOKEN`.

## Release Steps

1. Run `dotnet test backend/Kinevor.Platform.slnx -c Release`.
2. Run `cd web; npm ci; npm run build`.
3. Commit and push `main`.
4. Create and push the tag:

   ```powershell
   git tag v0.1.0
   git push origin main
   git push origin v0.1.0
   ```

5. Wait for the Release workflow to publish all four packages.
6. Create the `KINEVOR_PACKAGES_TOKEN` secret in every module repository. A
   classic PAT needs `repo`, `read:packages` and `write:packages`.
7. Publish the module repositories with their own `v0.1.0` tags.

## Verify

- The Packages tab lists three NuGet packages and one npm package at `0.1.0`.
- A clean module checkout restores the packages from GitHub Packages.
- The npm tarball contains `README.md`, `LICENSE` and `publishConfig` metadata.

Do not reuse or move the `v0.1.0` tag. Publish a patch version for fixes.
