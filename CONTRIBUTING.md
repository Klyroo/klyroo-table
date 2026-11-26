# Contributing to klyroo-table

Thanks for helping build Klyroo. This guide explains how to propose a change to **Klyroo.Table** and what a pull
request needs before it is merged. Klyroo is a family of independent repositories: each one has its own issues,
tests, versions and releases, so a change usually belongs to exactly one of them.

## Prerequisites

- **.NET SDK 10** (`global.json` pins the feature band and rolls forward). Install from [dot.net](https://dot.net).
- Some projects reference sibling Klyroo repositories by relative path (`../klyroo-ui`, ...). Clone the
  repositories you need next to each other:

  ```bash
  mkdir klyroo && cd klyroo
  git clone https://github.com/klyroo/klyroo-table.git
  # plus the siblings it references:
  git clone https://github.com/klyroo/klyroo-query.git
  git clone https://github.com/klyroo/klyroo-ui.git
  ```

## Build, test, format

```bash
dotnet build Klyroo.Table.slnx -c Release
dotnet test Klyroo.Table.slnx -c Release --no-build
dotnet format Klyroo.Table.slnx --include src/ tests/   # fix formatting of the files you touched
```

The build treats warnings as errors and nullable reference types are enabled everywhere: fix a warning instead
of suppressing it, and justify any `#pragma` or `[SuppressMessage]` in the code.

## The browser module

`src/Klyroo.Table/wwwroot/Klyroo.Table.lib.module.js` has no source in any repository: it is minified output whose original was lost, and it is now **maintained by hand**. Edit the file itself, keep the new
code readable, keep the `export` list at the end, and run `node --test tests/js/*.test.mjs` (Node 22 or later, no dependencies) after a change. The other modules in `wwwroot` (`klyroo-table-storage.js`,
`klyroo-table-download.js`) are plain readable source.

## Workflow

1. **Discuss first** for anything larger than a bug fix: open an issue (or comment on an existing one) so the
   design is agreed before you write code.
2. **Fork and branch** from `main`. Name the branch after the change: `feat/<short-topic>`, `fix/<short-topic>`,
   `docs/<short-topic>`, `refactor/...`, `test/...`, `ci/...` or `chore/...`.
3. **Commit** with [Conventional Commits](https://www.conventionalcommits.org/):

   ```text
   <type>(<optional scope>): <summary in the imperative, lower case, no period>

   <body: what and why, wrapped at 100 columns>

   BREAKING CHANGE: <what breaks and how to migrate>   # only when it applies
   ```

   Types: `feat`, `fix`, `docs`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`. A `!` after the type
   (`feat!:`) or a `BREAKING CHANGE:` footer marks a breaking change.
4. **Open a pull request** against `main` and fill in the template.

## What a pull request needs

- **One concern per pull request.** Unrelated refactors or reformatting go in their own pull request.
- **Tests.** A bug fix comes with a test that fails without it; a feature comes with tests of its public
  behavior. Never delete or disable a test to make a build pass: fix the cause, or explain in the pull request
  why the test was wrong.
- **Public API.** Every public type and member has XML documentation. A change to a public API is called out in
  the pull request; a breaking one needs a `BREAKING CHANGE:` footer and an entry in `CHANGELOG.md`.
- **Documentation.** Update `README.md` and `docs/` when behavior or usage changes. Examples must compile against
  the real API: never document an API that does not exist.
- **Changelog.** Add a line under `## [Unreleased]` in `CHANGELOG.md` for any user-visible change.
- **CI is green**: restore, build, tests, NativeAOT/trimming analyzers where they apply, and packaging.

## Review

A maintainer reviews every pull request. Expect questions about the public API, render modes (Static SSR,
Interactive Server, WebAssembly, MAUI Blazor Hybrid), accessibility and NativeAOT compatibility. At least one
maintainer approval and a green CI are required; the pull request is squash-merged, so its title must be a valid
Conventional Commit.

## Versioning and releases

Klyroo.Table follows [Semantic Versioning](https://semver.org/). While the version is `0.x`, minor versions may contain
breaking changes, always listed in the changelog. The version lives in `Directory.Build.props`
(`VersionPrefix` / `VersionSuffix`). Maintainers release by bumping it, moving the `Unreleased` changelog entries
under the new version, and pushing a `v<version>` tag; the release workflow checks that the tag matches the
version, builds, tests, packs, and waits for approval before publishing to NuGet and creating the GitHub Release.

## Issues

- **Bug**: use the bug template, with the package version, the render mode or platform, and a minimal reproduction.
- **Feature**: use the feature template and describe the problem before the solution.
- **Security**: never in a public issue; follow [SECURITY.md](./SECURITY.md).
- **Question**: use GitHub Discussions.

By contributing you agree that your contribution is licensed under the [MIT License](./LICENSE) and that you
follow the [Code of Conduct](./CODE_OF_CONDUCT.md).
