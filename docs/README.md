# WebDriverBiDi.NET Documentation

This directory contains the documentation source for WebDriverBiDi.NET, built using [DocFX](https://dotnet.github.io/docfx/).

## Structure

```
docs/
├── index.md                # Documentation home page
├── winston.md              # Project mascot page
├── toc.yml                 # Main table of contents
├── docfx.json              # DocFX configuration
├── articles/               # Conceptual documentation
│   ├── getting-started.md
│   ├── browser-setup.md
│   ├── first-application.md
│   ├── core-concepts.md
│   ├── quick-reference.md
│   ├── protocol-reference.md
│   ├── common-pitfalls.md
│   ├── architecture.md
│   ├── events-observables.md
│   ├── remote-values.md
│   ├── modules/            # Module-specific guides
│   │   ├── browser.md
│   │   ├── browsing-context.md
│   │   ├── script.md
│   │   ├── network.md
│   │   ├── input.md
│   │   ├── log.md
│   │   ├── session.md
│   │   ├── storage.md
│   │   ├── emulation.md
│   │   ├── permissions.md
│   │   ├── bluetooth.md
│   │   ├── webextension.md
│   │   ├── speculation.md
│   │   ├── user-agent-client-hints.md
│   │   ├── digital-credentials.md
│   │   └── additional-modules.md
│   ├── examples/           # Example tutorials
│   │   ├── common-scenarios.md
│   │   ├── form-submission.md
│   │   ├── network-interception.md
│   │   ├── console-monitoring.md
│   │   └── preload-scripts.md
│   ├── advanced/           # Advanced use case guides
│   │   ├── analyzers.md
│   │   ├── observability.md
│   │   ├── webdriverbidi-logging.md
│   │   ├── api-design.md
│   │   ├── error-handling.md
│   │   ├── performance.md
│   │   ├── connection-management.md
│   │   ├── custom-modules.md
│   │   └── aot-compatibility.md
│   └── toc.yml             # Articles table of contents
├── api/                    # API reference
│   └── index.md            # API reference home
├── code/                   # Compilable snippets included in articles (see code/README.md)
│   ├── README.md
│   ├── DocsReadmeSamples.cs
│   ├── IndexSamples.cs
│   ├── QuickReferenceSamples.cs
│   ├── WebDriverBiDi.DocSnippets.csproj
│   ├── advanced/
│   ├── api/
│   ├── api-design/
│   ├── architecture/
│   ├── common-pitfalls/
│   ├── core-concepts/
│   ├── error-handling/
│   ├── events-observables/
│   ├── examples/
│   ├── modules/
│   ├── remote-values/
│   └── script/
├── images/                 # Mascot image and favicon
├── templates/              # Custom DocFX template
├── tools/                  # validate-doc-regions.sh (run by CI on doc changes)
└── _site/                  # Generated documentation (gitignored)
```

## Building the Documentation

### Prerequisites

Install DocFX:

```bash
dotnet tool install -g docfx
```

### Build

From the repository root:

```bash
dotnet build src/WebDriverBiDi/WebDriverBiDi.csproj --configuration Release
dotnet build docs/code/WebDriverBiDi.DocSnippets.csproj
docfx metadata docs/docfx.json
docfx build docs/docfx.json
```

These steps:
1. Build the library in Release (`docfx metadata` reads the API surface from `src/WebDriverBiDi/bin/Release/netstandard2.0/WebDriverBiDi.dll`)
2. Compile the documentation code samples in `docs/code/` (every `[!code-csharp]` region must compile)
3. Extract API documentation from XML comments (`docfx metadata`)
4. Process markdown files and generate the complete documentation site in `docs/_site/` (`docfx build`)

### Serve Locally

To preview the documentation:

```bash
docfx serve _site
```

Then open your browser to `http://localhost:8080`.

### Build and Serve in One Step

```bash
docfx docfx.json --serve
```

## Writing Documentation

### Markdown Files

- Use standard Markdown syntax
- Code blocks should specify language: \`\`\`csharp
- Use relative links for cross-references
- Include practical examples

### Code Examples

Do not paste code into markdown. Put it in a `.cs` file under `docs/code/` inside a `#region Name` /
`#endregion` block and reference it with `[!code-csharp[Title](code/File.cs#Name)]`, so that it compiles
with the `WebDriverBiDi.DocSnippets` project and `docs/tools/validate-doc-regions.sh` (run in CI) can check
that every reference points at an existing region. See [code/README.md](code/README.md). For example:

[!code-csharp[Complete Runnable Example](code/DocsReadmeSamples.cs#CompleteRunnableExample)]

### Cross-References

Link to other documentation:

```markdown
See [Core Concepts](core-concepts.md) for more information.
See the [API Reference](../api/index.md) for complete details.
```

### API Documentation

API documentation is generated from XML comments in the source code. To improve API docs:

1. Edit the XML comments in `src/WebDriverBiDi/**/*.cs`
2. Rebuild the project
3. Rebuild documentation

## Documentation Guidelines

### Style

- **Be concise**: Get to the point quickly
- **Be practical**: Include working examples
- **Be complete**: Cover common scenarios
- **Be accurate**: Test all code examples

### Structure

- Start with overview/context
- Provide examples early
- Include troubleshooting sections
- Link to related topics

### Code Examples

- Live in `docs/code/` as compiled `#region` snippets, referenced from markdown (see above)
- Must compile against the current library; a region may be a method body fragment, in which case the prose should say what the reader must add (usings, variables)
- Handle errors appropriately
- Show output/results when helpful

### Screenshots

If adding screenshots:

1. Place in `docs/images/`
2. Reference with relative path: `![Description](images/screenshot.png)`
3. Keep file sizes reasonable

## Contributing

To contribute to documentation:

1. Edit markdown files in `docs/articles/`
2. Test locally with `docfx serve`
3. Submit a pull request

For API documentation changes:

1. Edit XML comments in source files
2. Verify with IntelliSense
3. Rebuild documentation to test

## Configuration

### docfx.json

Key configuration sections:

- **metadata**: API documentation extraction settings
- **build.content**: Files to include in build
- **build.template**: Visual theme (currently `default` + `modern` + the custom `templates/webdriverbidi` overrides)

### Customization

To customize the appearance:

1. Edit the existing custom template in `templates/webdriverbidi/` (it is already referenced from `docfx.json`)
2. Add a further template directory to the `template` array in `docfx.json` if you need a separate one
3. See [DocFX templating docs](https://dotnet.github.io/docfx/tutorial/howto_customize_docfx_flavored_markdown.html)

## Publishing

Documentation is published automatically. Pushing a release tag (`vX.Y.Z`) runs `.github/workflows/release.yml`,
whose `build-docs` job builds the site exactly as described above and whose `deploy-docs` job publishes it to
GitHub Pages with `actions/deploy-pages`. The `_site/` directory is gitignored and must not be committed; merges
to `main` between releases do not change the published site.

## Troubleshooting

### "Command not found: docfx"

Install DocFX globally:
```bash
dotnet tool install -g docfx
```

### "File not found" errors

Ensure all referenced files exist and paths are correct. Check:
- Link targets exist
- Paths use forward slashes
- No typos in filenames

### API documentation not generating

1. Ensure project builds successfully
2. Check XML documentation is enabled in `.csproj`
3. Verify `docfx.json` metadata section is correct

### Changes not appearing

1. Clear the `_site` folder: `rm -rf _site`
2. Rebuild: `docfx build`
3. Hard refresh in browser (Ctrl+F5)

## Resources

- [DocFX Documentation](https://dotnet.github.io/docfx/)
- [Markdown Guide](https://www.markdownguide.org/)
- [DocFX Template Documentation](https://dotnet.github.io/docfx/tutorial/intro_template.html)

## Questions?

For questions about the documentation:

- Open an issue: https://github.com/webdriverbidi-net/webdriverbidi-net/issues
- Tag with `documentation` label
