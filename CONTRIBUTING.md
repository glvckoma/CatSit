# Contributing to CatSit

This is a solo-maintained hobby project. Contributions are welcome, but keep the following in mind.

## Before you start

For anything beyond a small fix, open an issue first to discuss the approach. This is reviewed by one person, so it avoids spending effort on a PR that ends up going a different direction.

## Building and testing

```bash
dotnet build CatSit.sln -c Release
dotnet test tests/CatSit.Core.Tests
```

## Code style

Match what's already there: nullable enabled, file-scoped namespaces, no comments unless they explain a non-obvious *why*. Don't add abstractions or configuration beyond what the change needs.

**No new PackageReferences in `src/CatSit` or `src/CatSit.Core` without discussion first.** Zero third-party dependencies in the shipped app is a deliberate design goal, not an accident. Test-only dependencies in `tests/CatSit.Core.Tests` are fine.

## Tests

Behavior changes in `CatSit.Core` should come with `[Fact]`/`[Theory]` coverage. The detection, learning, sprite, and audio logic all live there specifically so they can be tested without a UI.

## Pull requests

Keep PRs focused and small. CI (`dotnet build` + `dotnet test` on `windows-latest`) must pass before merge.

## Reporting bugs or requesting features

Use the issue templates. For bugs, include repro steps, expected vs. actual behavior, your Windows version, and whether you're running the installed release or a self-built copy.
