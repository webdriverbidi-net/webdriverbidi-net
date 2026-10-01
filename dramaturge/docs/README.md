# Dramaturge Documentation

- `articles/`: the guides.
- `code/`: the C# samples the guides show through DocFX's `[!code-csharp[]()]` syntax, and the samples in the packed
  package READMEs, compiled by `Dramaturge.DocSnippets` so that they cannot drift from the API. A README sample is
  written out in full, because nuget.org renders the README, and names the region it mirrors in a
  `<!-- readme-csharp: docs/code/File.cs#Region -->` marker; the path is relative to the repository root.
- `tools/validate-doc-regions.sh`: checks that every reference names an existing region, and that every README sample
  matches its region.

The DocFX site itself is set up with the package's productionization.
