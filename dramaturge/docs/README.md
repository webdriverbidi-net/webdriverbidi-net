# Dramaturge Documentation

- `articles/`: the guides.
- `code/`: the C# samples the guides show through DocFX's `[!code-csharp[]()]` syntax, and the samples in the packed
  package READMEs, compiled by `Dramaturge.DocSnippets` so that they cannot drift from the API. A README sample is
  written out in full, because nuget.org renders the README, and names the region it mirrors in a
  `<!-- readme-csharp: docs/code/File.cs#Region -->` marker; the path is relative to the repository root.
- `tools/validate-doc-regions.sh`: checks that every reference names an existing region, and that every README sample
  matches its region.
- `tools/generate-llms.cs`: writes `llms.txt` and `llms-full.txt` into the built site (see below).
- `docfx.json`, `toc.yml`, `index.md`, `api/index.md`, and `templates/dramaturge`: the DocFX site, published to
  https://webdriverbidi-net.github.io/dramaturge/. Its API reference is read from the netstandard2.0 builds.

To build the site, with DocFX pinned in `.config/dotnet-tools.json`, run from the `dramaturge` directory:

    dotnet build src/Dramaturge/Dramaturge.csproj --configuration Release
    dotnet tool restore
    cd docs
    dotnet docfx metadata docfx.json
    dotnet docfx build docfx.json
    cd ..
    dotnet run docs/tools/generate-llms.cs -- docs https://webdriverbidi-net.github.io/dramaturge/
    dotnet docfx serve docs/_site

The `generate-llms.cs` step writes `llms.txt` and `llms-full.txt` into the site, for language models: an index of
the articles, and every article in one Markdown file with its code samples written out. They are published with the
site, at its root.
