# Documentation Code Snippets

This directory contains C# code snippets that are included in the documentation via DocFx's `[!code-csharp[]()]` syntax. The snippets are compiled at build time to prevent API drift between the documentation and the WebDriverBiDi library.

## Structure

- `error-handling/` - Snippets for `docs/articles/advanced/error-handling.md`
- `api-design/` - Snippets for `docs/articles/advanced/api-design.md`
- `events-observables/` - Snippets for `docs/articles/events-observables.md`
- `core-concepts/` - Snippets for `docs/articles/core-concepts.md`
- `common-pitfalls/` - Snippets for `docs/articles/common-pitfalls.md`
- `script/` - Snippets for `docs/articles/modules/script.md`, `docs/articles/examples/preload-scripts.md`
- `remote-values/` - Snippets for `docs/articles/remote-values.md`
- `examples/` - Snippets for `docs/articles/examples/*.md` (common-scenarios, console-monitoring, form-submission, network-interception) and for `getting-started.md`, `first-application.md` and `browser-setup.md`
- `modules/` - Snippets for every guide in `docs/articles/modules/*.md`
- `advanced/` - Snippets for the remaining `docs/articles/advanced/*.md` guides (AOT compatibility, connection management, custom modules, observability, performance)
- `api/` - Snippets for `docs/api/index.md`
- `architecture/` - Snippets for `docs/articles/architecture.md`
- `IndexSamples.cs` - Snippets for `docs/index.md`
- `DocsReadmeSamples.cs` - Snippets for `docs/README.md`
- `QuickReferenceSamples.cs` - Snippets for `docs/articles/quick-reference.md`

## Validation

The `WebDriverBiDi.DocSnippets` project references the main WebDriverBiDi library. Building this project validates that all snippet code compiles against the current API. If the API changes and the snippets are not updated, the build will fail.

Build the snippets project:

```bash
dotnet build docs/code/WebDriverBiDi.DocSnippets.csproj --configuration Release
```

The main solution build includes this project, so validation runs automatically during `dotnet build`.

## Adding New Snippets

1. Create or add to a `.cs` file in the appropriate subdirectory.
2. Wrap the snippet in a compilable method (namespace, class, usings), and surround the lines to show with
   `#region Name` / `#endregion`. Keep `#endregion` inside the method body — placing it after the method's closing
   brace renders a stray `}` in the article.
3. Reference the snippet in the markdown using `[!code-csharp[Title](path/to/file.cs#Name)]`, where `Name` is the
   region name. `docs/tools/validate-doc-regions.sh` (run by CI) fails the build if a referenced region does not exist.
4. Paths in markdown are relative to the markdown file (e.g., `../../code/error-handling/TimeoutSamples.cs` from `articles/advanced/error-handling.md`).
