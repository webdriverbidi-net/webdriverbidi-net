// <copyright file="generate-llms.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// Writes llms.txt, an index of the documentation for language models, and llms-full.txt, every article in one
// Markdown file, into the built DocFX site. It reads the articles' sources, not the site's HTML, and replaces each
// compiled sample reference with the code of its region, so that the files hold the code the site shows.
//
// Usage: dotnet run docs/tools/generate-llms.cs -- <docs directory> <site URL> [output directory]
// The output directory defaults to <docs directory>/_site. A missing article, sample file, or region fails the run.
using System.Text;
using System.Text.RegularExpressions;

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: generate-llms.cs <docs directory> <site URL> [output directory]");
    return 2;
}

string docsDirectory = Path.GetFullPath(args[0]);
string siteUrl = args[1].TrimEnd('/') + "/";
string outputDirectory = args.Length > 2 ? Path.GetFullPath(args[2]) : Path.Combine(docsDirectory, "_site");

try
{
    Generator generator = new(docsDirectory, siteUrl);
    (string index, string full) = generator.Generate();
    Directory.CreateDirectory(outputDirectory);
    File.WriteAllText(Path.Combine(outputDirectory, "llms.txt"), index);
    File.WriteAllText(Path.Combine(outputDirectory, "llms-full.txt"), full);
    Console.WriteLine($"Wrote llms.txt ({index.Length:N0} characters) and llms-full.txt ({full.Length:N0} characters, {generator.ArticleCount} articles, {generator.SampleCount} samples) to {outputDirectory}.");
    return 0;
}
catch (GeneratorException ex)
{
    Console.Error.WriteLine($"FAIL: {ex.Message}");
    return 1;
}

/// <summary>
/// An entry of a table of contents: a page, or a group of entries.
/// </summary>
/// <param name="Name">The entry's name.</param>
/// <param name="Href">The page, relative to the table of contents, with any fragment; null for a group.</param>
/// <param name="Items">The entries of a group.</param>
internal sealed record TocEntry(string Name, string? Href, List<TocEntry> Items);

/// <summary>
/// A problem with the documentation that stops the files from being written.
/// </summary>
/// <param name="message">The problem.</param>
internal sealed class GeneratorException(string message) : Exception(message);

/// <summary>
/// Builds the two files from a DocFX documentation directory.
/// </summary>
/// <param name="docsDirectory">The directory holding docfx.json, toc.yml, index.md, and the articles.</param>
/// <param name="siteUrl">The published site's URL, ending with a slash.</param>
internal sealed partial class Generator(string docsDirectory, string siteUrl)
{
    private readonly HashSet<string> writtenArticles = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the number of articles written to llms-full.txt.
    /// </summary>
    public int ArticleCount => this.writtenArticles.Count;

    /// <summary>
    /// Gets the number of sample references expanded.
    /// </summary>
    public int SampleCount { get; private set; }

    /// <summary>
    /// Builds both files.
    /// </summary>
    /// <returns>The contents of llms.txt and llms-full.txt.</returns>
    public (string Index, string Full) Generate()
    {
        string indexPath = Path.Combine(docsDirectory, "index.md");
        string indexMarkdown = ReadFile(indexPath);
        string title = FirstHeading(indexMarkdown) ?? throw new GeneratorException($"{indexPath} has no top-level heading.");
        string summary = FirstParagraph(indexMarkdown) ?? throw new GeneratorException($"{indexPath} has no paragraph to summarize the site.");

        string articlesDirectory = Path.Combine(docsDirectory, "articles");
        List<TocEntry> toc = ParseToc(Path.Combine(articlesDirectory, "toc.yml"));

        StringBuilder index = new();
        index.Append("# ").AppendLine(title).AppendLine();
        index.Append("> ").AppendLine(PlainText(summary)).AppendLine();
        index.AppendLine("Each link below is a page of the documentation. `llms-full.txt` holds every one of them, with their code samples, in one file.").AppendLine();

        List<TocEntry> pages = [.. toc.Where(entry => entry.Href is not null)];
        AppendSection(index, "Guides", pages, articlesDirectory);
        foreach (TocEntry group in toc.Where(entry => entry.Href is null))
        {
            AppendSection(index, group.Name, group.Items, articlesDirectory);
        }

        index.AppendLine("## Optional").AppendLine();
        index.Append("- [Full documentation](").Append(siteUrl).AppendLine("llms-full.txt): every page above in one file, with its code samples");
        index.Append("- [API reference](").Append(siteUrl).AppendLine("api/index.html): every public type and member, generated from the XML documentation comments");

        StringBuilder full = new();
        full.Append("# ").AppendLine(title).AppendLine();
        full.Append("> ").AppendLine(PlainText(summary)).AppendLine();
        full.Append("Source: ").AppendLine(siteUrl).AppendLine();
        full.AppendLine(this.RenderPage(indexPath, StripFirstHeading(indexMarkdown), "index.html").Trim()).AppendLine();
        foreach (TocEntry entry in Flatten(toc))
        {
            string file = StripFragment(entry.Href!);
            if (!this.writtenArticles.Add(file))
            {
                continue;
            }

            string path = Path.Combine(articlesDirectory, file);
            string markdown = ReadFile(path);
            string pageUrl = "articles/" + Path.ChangeExtension(file, ".html").Replace('\\', '/');
            full.AppendLine("---").AppendLine();
            full.Append("Source: ").Append(siteUrl).AppendLine(pageUrl).AppendLine();
            full.AppendLine(this.RenderPage(path, markdown, pageUrl).Trim()).AppendLine();
        }

        return (index.ToString().TrimEnd() + "\n", full.ToString().TrimEnd() + "\n");
    }

    private static string ReadFile(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path).Replace("\r\n", "\n") : throw new GeneratorException($"{path} does not exist.");
    }

    private static IEnumerable<TocEntry> Flatten(IEnumerable<TocEntry> entries)
    {
        foreach (TocEntry entry in entries)
        {
            if (entry.Href is not null)
            {
                yield return entry;
            }

            foreach (TocEntry child in Flatten(entry.Items))
            {
                yield return child;
            }
        }
    }

    private static string StripFragment(string href)
    {
        int hash = href.IndexOf('#', StringComparison.Ordinal);
        return hash < 0 ? href : href[..hash];
    }

    private static string? FirstHeading(string markdown)
    {
        Match match = HeadingPattern().Match(markdown);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string StripFirstHeading(string markdown)
    {
        return HeadingPattern().Replace(markdown, string.Empty, 1);
    }

    // The first paragraph of prose: not a heading, a list, a table, a quotation, a code block, or a sample reference.
    private static string? FirstParagraph(string markdown)
    {
        bool inFence = false;
        List<string> paragraph = [];
        foreach (string line in markdown.Split('\n'))
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                inFence = !inFence;
                continue;
            }

            if (inFence)
            {
                continue;
            }

            bool isProse = trimmed.Length > 0 && !trimmed.StartsWith('#') && !trimmed.StartsWith('>') && !trimmed.StartsWith('|')
                && !trimmed.StartsWith("- ", StringComparison.Ordinal) && !trimmed.StartsWith("* ", StringComparison.Ordinal)
                && !trimmed.StartsWith("[!", StringComparison.Ordinal) && !trimmed.StartsWith('<') && !ListItemPattern().IsMatch(trimmed);
            if (isProse)
            {
                paragraph.Add(trimmed);
            }
            else if (paragraph.Count > 0)
            {
                break;
            }
        }

        return paragraph.Count > 0 ? string.Join(" ", paragraph) : null;
    }

    private static string FirstSentence(string paragraph)
    {
        Match match = SentenceEndPattern().Match(paragraph);
        return match.Success ? paragraph[..(match.Index + 1)] : paragraph;
    }

    // Link text in place of each link, without emphasis or code marks, for a one-line description.
    private static string PlainText(string markdown)
    {
        string text = LinkPattern().Replace(markdown, "$1");
        return text.Replace("**", string.Empty, StringComparison.Ordinal).Replace("`", string.Empty, StringComparison.Ordinal);
    }

    // Reads the subset of YAML that DocFX tables of contents use here: lists of name, href, and items.
    private static List<TocEntry> ParseToc(string path)
    {
        string[] lines = ReadFile(path).Split('\n');
        int position = 0;
        return ParseTocList(lines, ref position, 0, path);
    }

    private static List<TocEntry> ParseTocList(string[] lines, ref int position, int indent, string path)
    {
        List<TocEntry> entries = [];
        while (position < lines.Length)
        {
            string line = lines[position];
            if (line.Trim().Length == 0 || line.TrimStart().StartsWith('#'))
            {
                position++;
                continue;
            }

            int lineIndent = line.Length - line.TrimStart().Length;
            if (lineIndent < indent)
            {
                break;
            }

            Match item = TocItemPattern().Match(line.TrimStart());
            if (!item.Success || lineIndent != indent)
            {
                throw new GeneratorException($"{path}:{position + 1} is not a table of contents entry this generator reads.");
            }

            string? name = null;
            string? href = null;
            List<TocEntry> items = [];
            ReadTocField(item.Groups[1].Value, ref name, ref href);
            position++;
            int fieldIndent = indent + 2;
            while (position < lines.Length)
            {
                string fieldLine = lines[position];
                if (fieldLine.Trim().Length == 0)
                {
                    position++;
                    continue;
                }

                int fieldLineIndent = fieldLine.Length - fieldLine.TrimStart().Length;
                if (fieldLineIndent != fieldIndent || fieldLine.TrimStart().StartsWith('-'))
                {
                    break;
                }

                string field = fieldLine.Trim();
                if (field == "items:")
                {
                    position++;
                    int childIndent = position < lines.Length ? lines[position].Length - lines[position].TrimStart().Length : fieldIndent;
                    items = ParseTocList(lines, ref position, childIndent, path);
                    continue;
                }

                ReadTocField(field, ref name, ref href);
                position++;
            }

            entries.Add(new TocEntry(name ?? throw new GeneratorException($"{path}: an entry has no name."), href, items));
        }

        return entries;
    }

    private static void ReadTocField(string field, ref string? name, ref string? href)
    {
        int colon = field.IndexOf(':', StringComparison.Ordinal);
        if (colon < 0)
        {
            return;
        }

        string key = field[..colon].Trim();
        string value = field[(colon + 1)..].Trim().Trim('"', '\'');
        if (key == "name")
        {
            name = value;
        }
        else if (key == "href")
        {
            href = value;
        }
    }

    [GeneratedRegex(@"^#[ \t]+(.+)$", RegexOptions.Multiline)]
    private static partial Regex HeadingPattern();

    [GeneratedRegex(@"^\d+\.\s")]
    private static partial Regex ListItemPattern();

    [GeneratedRegex(@"[.!?](?=\s|$)")]
    private static partial Regex SentenceEndPattern();

    [GeneratedRegex(@"\[([^\]]+)\]\([^)]*\)")]
    private static partial Regex LinkPattern();

    [GeneratedRegex(@"^-\s+(.*)$")]
    private static partial Regex TocItemPattern();

    [GeneratedRegex(@"^(?<indent>[ \t]*)\[!code-(?<language>[a-z]+)\[(?<title>[^\]]*)\]\((?<path>[^)#]+)(?:#(?<region>[^)]+))?\)\][ \t]*$", RegexOptions.Multiline)]
    private static partial Regex SamplePattern();

    [GeneratedRegex(@"^(?<prefix>[ \t]*>[ \t]*)\[!(?<kind>NOTE|TIP|IMPORTANT|WARNING|CAUTION)\][ \t]*$", RegexOptions.Multiline)]
    private static partial Regex AlertPattern();

    [GeneratedRegex(@"\]\((?<target>[^)\s]+)\)")]
    private static partial Regex LinkTargetPattern();

    [GeneratedRegex(@"^\s*#region\s+(?<name>.+?)\s*$")]
    private static partial Regex RegionStartPattern();

    [GeneratedRegex(@"^\s*#endregion\b")]
    private static partial Regex RegionEndPattern();

    private void AppendSection(StringBuilder index, string heading, List<TocEntry> entries, string articlesDirectory)
    {
        if (entries.Count == 0)
        {
            return;
        }

        index.Append("## ").AppendLine(heading).AppendLine();
        foreach (TocEntry entry in Flatten(entries))
        {
            string href = entry.Href!;
            string file = StripFragment(href);
            string fragment = href[file.Length..];
            string? paragraph = fragment.Length == 0 ? FirstParagraph(StripFirstHeading(ReadFile(Path.Combine(articlesDirectory, file)))) : null;
            index.Append("- [").Append(entry.Name).Append("](").Append(siteUrl).Append("articles/")
                .Append(Path.ChangeExtension(file, ".html").Replace('\\', '/')).Append(fragment).Append(')');
            if (paragraph is not null)
            {
                index.Append(": ").Append(PlainText(FirstSentence(paragraph)));
            }

            index.AppendLine();
        }

        index.AppendLine();
    }

    // Expands sample references, turns DocFX alerts into plain emphasis, and makes links absolute.
    private string RenderPage(string path, string markdown, string pageUrl)
    {
        string directory = Path.GetDirectoryName(path)!;
        string rendered = SamplePattern().Replace(markdown, match => this.ExpandSample(match, directory, path));
        rendered = AlertPattern().Replace(rendered, match =>
        {
            string kind = match.Groups["kind"].Value;
            return $"{match.Groups["prefix"].Value}**{kind[0]}{kind[1..].ToLowerInvariant()}:**";
        });
        string pageDirectory = pageUrl.Contains('/', StringComparison.Ordinal) ? pageUrl[..(pageUrl.LastIndexOf('/') + 1)] : string.Empty;
        return LinkTargetPattern().Replace(rendered, match => $"]({this.ResolveLink(match.Groups["target"].Value, pageDirectory)})");
    }

    private string ResolveLink(string target, string pageDirectory)
    {
        if (target.StartsWith("xref:", StringComparison.Ordinal))
        {
            string uid = target["xref:".Length..];
            int hash = uid.IndexOf('#', StringComparison.Ordinal);
            string anchor = hash < 0 ? string.Empty : uid[hash..];
            return $"{siteUrl}api/{(hash < 0 ? uid : uid[..hash])}.html{anchor}";
        }

        if (target.Contains("://", StringComparison.Ordinal) || target.StartsWith('#') || target.StartsWith("mailto:", StringComparison.Ordinal))
        {
            return target;
        }

        string file = StripFragment(target);
        string fragment = target[file.Length..];
        if (file.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            file = Path.ChangeExtension(file, ".html");
        }

        Uri resolved = new(new Uri(siteUrl + pageDirectory), file);
        return resolved.AbsoluteUri + fragment;
    }

    private string ExpandSample(Match match, string directory, string page)
    {
        string indent = match.Groups["indent"].Value;
        string samplePath = Path.GetFullPath(Path.Combine(directory, match.Groups["path"].Value));
        string source = ReadFile(samplePath);
        string code = match.Groups["region"].Success
            ? ExtractRegion(source, match.Groups["region"].Value) ?? throw new GeneratorException($"{page} refers to region '{match.Groups["region"].Value}' of {samplePath}, which does not have it.")
            : source;
        this.SampleCount++;
        StringBuilder block = new();
        block.Append(indent).Append("```").AppendLine(match.Groups["language"].Value);
        foreach (string line in Dedent(code))
        {
            block.Append(line.Length == 0 ? string.Empty : indent).AppendLine(line);
        }

        block.Append(indent).Append("```");
        return block.ToString();
    }

    // The lines of a region, without its markers or those of regions nested in it.
    private static string? ExtractRegion(string source, string name)
    {
        string[] lines = source.Split('\n');
        int depth = 0;
        bool found = false;
        List<string> body = [];
        foreach (string line in lines)
        {
            Match start = RegionStartPattern().Match(line);
            if (!found)
            {
                found = start.Success && start.Groups["name"].Value == name;
                continue;
            }

            if (start.Success)
            {
                depth++;
                continue;
            }

            if (RegionEndPattern().IsMatch(line))
            {
                if (depth == 0)
                {
                    return string.Join('\n', body);
                }

                depth--;
                continue;
            }

            body.Add(line);
        }

        return null;
    }

    private static List<string> Dedent(string code)
    {
        List<string> lines = [.. code.Split('\n').Select(line => line.TrimEnd())];
        while (lines.Count > 0 && lines[0].Length == 0)
        {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && lines[^1].Length == 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        int common = lines.Where(line => line.Length > 0).Select(line => line.Length - line.TrimStart().Length).DefaultIfEmpty(0).Min();
        return [.. lines.Select(line => line.Length == 0 ? line : line[common..])];
    }
}
