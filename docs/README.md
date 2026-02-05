# WebDriverBiDi.NET-Relaxed Documentation

This directory contains the documentation source for WebDriverBiDi.NET-Relaxed, built using [DocFX](https://dotnet.github.io/docfx/).

## Structure

```
docs/
├── index.md             # Documentation home page
├── toc.yml              # Main table of contents
├── docfx.json           # DocFX configuration
├── articles/            # Conceptual documentation
│   ├── getting-started.md
│   ├── browser-setup.md
│   ├── first-application.md
│   ├── core-concepts.md
│   ├── architecture.md
│   ├── events-observables.md
│   ├── remote-values.md
│   ├── advanced/        # Advanced use case guides
│   │   ├── custom-modules.md
│   │   ├── error-handling.md
│   │   └── performance.md
│   ├── modules/         # Module-specific guides
│   │   ├── browser.md
│   │   ├── browsing-context.md
│   │   └── emulation.md
│   │   ├── network.md
│   │   ├── input.md
│   │   ├── log.md
│   │   ├── script.md
│   │   ├── session.md
│   │   ├── storage.md
│   │   └── additional-modules.md
│   ├── examples/        # Example tutorials
│   │   └── common-scenarios.md
│   │   └── form-submission.md
│   │   └── network-interception.md
│   │   └── console-monitoring.md
│   │   └── preload-scripts.md
│   └── toc.yml         # Articles table of contents
├── api/                # API reference
│   └── index.md        # API reference home
└── _site/              # Generated documentation (gitignored)
```

## Building the Documentation

### Prerequisites

Install DocFX:

```bash
dotnet tool install -g docfx
```

### Build

From the `docs` directory:

```bash
docfx build docfx.json
```

This will:
1. Extract API documentation from XML comments
2. Process markdown files
3. Generate the complete documentation site in `_site/`

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

Use complete, runnable examples:

```csharp
using WebDriverBiDi;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
await driver.StartAsync("ws://localhost:9222/session");

// ... your code here ...

await driver.StopAsync();
```

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

- Must be complete and runnable
- Include necessary using statements
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
- **build.template**: Visual theme (currently "default")

### Customization

To customize the appearance:

1. Create a custom template in `templates/`
2. Update `docfx.json` to reference your template
3. See [DocFX templating docs](https://dotnet.github.io/docfx/tutorial/howto_customize_docfx_flavored_markdown.html)

## Publishing

To publish documentation to a web server:

1. Build the documentation: `docfx build`
2. Deploy the `_site` folder to your web host
3. Ensure proper MIME types for `.json`, `.yml` files

For GitHub Pages:

```bash
# Build docs
docfx build

# Commit _site folder
git add _site -f
git commit -m "Update documentation"

# Push to gh-pages branch
git subtree push --prefix docs/_site origin gh-pages
```

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

- Open an issue: https://github.com/hardkoded/webdriverbidi-net-relaxed/issues
- Tag with `documentation` label

