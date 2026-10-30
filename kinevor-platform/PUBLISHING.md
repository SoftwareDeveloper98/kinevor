# Publishing kinevor-platform

> **Monorepo note:** Kinevor is published as one showcase monorepo. These steps are optional and apply only if you extract this folder into a standalone repository or publish its packages to a registry.

The platform publishes the shared packages before any module release.

## Published artifacts

| Ecosystem | Package |
| --- | --- |
| NuGet | `Kinevor.Platform.Domain` |
| NuGet | `Kinevor.Platform.Application` |
| NuGet | `Kinevor.Platform.Api` |
| npm | `@softwaredeveloper98/kinevor-platform-web` |

## Prerequisites

- The monorepo exists at `https://github.com/SoftwareDeveloper98/kinevor`.
- The NuGet and npm versions are `0.1.0` in `backend/Directory.Build.props`,
  the Angular package and `CHANGELOG.md`.
- A token with `write:packages` for GitHub Packages, or equivalent access to
  your registry.

Only the root CI workflow ships with the monorepo. Publishing is manual; run
the commands below from a clean checkout.

## Release steps

1. Run `dotnet test backend/Kinevor.Platform.slnx -c Release`.
2. Run `cd web; npm ci; npm run build`.
3. Pack the NuGet packages:

   ```powershell
   dotnet pack backend/Kinevor.Platform.slnx -c Release -o backend/artifacts/packages
   ```

4. Publish them:

   ```powershell
   dotnet nuget push "backend/artifacts/packages/*.nupkg" `
     --source https://nuget.pkg.github.com/SoftwareDeveloper98/index.json `
     --api-key $env:GITHUB_TOKEN
   ```

5. Publish the Angular package from the build output:

   ```powershell
   npm publish dist/kinevor-platform-web --access restricted
   ```

6. Tag the commit if this is a release snapshot:

   ```powershell
   git tag v0.1.0
   git push origin main
   git push origin v0.1.0
   ```

## Verify

- The Packages tab lists three NuGet packages and one npm package at `0.1.0`.
- A clean module checkout restores the packages from GitHub Packages.
- The npm tarball contains `README.md`, `LICENSE` and `publishConfig` metadata.

## Consumer access

Modules restore from the local feed by default, so no token is needed for the
monorepo. If you switch them to GitHub Packages, create one
`KINEVOR_PACKAGES_TOKEN` for the repository and add the source to each module's
NuGet or npm configuration.

Tag every release. Once packages are published for a tag, treat it as immutable
and publish a patch version for fixes.
