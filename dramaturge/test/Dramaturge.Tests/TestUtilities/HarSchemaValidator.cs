// <copyright file="HarSchemaValidator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TestUtilities;

using System.Text.Json;
using Json.Schema;

/// <summary>
/// Validates documents against the vendored HAR 1.2 JSON schema (see HarSchema/README.md).
/// </summary>
public static class HarSchemaValidator
{
    private static readonly Uri BaseUri = new("https://har-schema.test/");
    private static readonly Lazy<JsonSchema> Schema = new(LoadSchema);

    /// <summary>
    /// Gets the reasons a document does not conform to the schema.
    /// </summary>
    /// <param name="har">The HAR document.</param>
    /// <returns>The validation errors, which are empty if the document is valid.</returns>
    public static IReadOnlyList<string> Validate(string har)
    {
        using JsonDocument document = JsonDocument.Parse(har);
        EvaluationResults results = Schema.Value.Evaluate(document.RootElement, new EvaluationOptions() { OutputFormat = OutputFormat.List });
        if (results.IsValid)
        {
            return [];
        }

        return [.. (results.Details ?? []).Where(detail => detail.Errors is not null).SelectMany(detail => detail.Errors!.Select(error => $"{detail.InstanceLocation}: {error.Key} {error.Value}"))];
    }

    // Every schema file is registered under a common base, so that the relative references between them resolve.
    private static JsonSchema LoadSchema()
    {
        BuildOptions options = new() { SchemaRegistry = new SchemaRegistry() };
        string directory = Path.Combine(AppContext.BaseDirectory, "HarSchema");
        JsonSchema? root = null;
        foreach (string file in Directory.GetFiles(directory, "*.json").OrderBy(file => file, StringComparer.Ordinal))
        {
            string name = Path.GetFileName(file);
            JsonSchema schema = JsonSchema.FromText(File.ReadAllText(file), options, new Uri(BaseUri, name));
            options.SchemaRegistry.Register(new Uri(BaseUri, name), schema);
            if (name == "har.json")
            {
                root = schema;
            }
        }

        return root ?? throw new InvalidOperationException("har.json was not found.");
    }
}
