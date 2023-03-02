// <copyright file="PrintCommandParameters.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the browsingContext.navigate command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class PrintCommandParameters : CommandParameters<PrintCommandResult>
{
    private string browsingContextId;
    private bool? background;
    private PrintMarginParameters? margins;
    private PrintOrientation? orientation;
    private PrintPageParameters? page;
    private List<object> pageRanges = new();
    private double? scale;
    private bool? shrinkToFit;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrintCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context to print.</param>
    public PrintCommandParameters(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "browsingContext.print";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to capture the screenshot.
    /// </summary>
    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets a value indicating whether to print background images. Defaults to false.
    /// </summary>
    [JsonProperty("background", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Background { get => this.background; set => this.background = value; }

    /// <summary>
    /// Gets or sets a value containing the margins for the printed page.
    /// </summary>
    [JsonProperty("margin", NullValueHandling = NullValueHandling.Ignore)]
    public PrintMarginParameters? Margins { get => this.margins; set => this.margins = value; }

    /// <summary>
    /// Gets or sets the orientation of the printed page. If omitted, defaults to Portrait.
    /// </summary>
    [JsonProperty("orientation", NullValueHandling = NullValueHandling.Ignore)]
    public PrintOrientation? Orientation { get => this.orientation; set => this.orientation = value; }

    /// <summary>
    /// Gets or sets a value containing information about the size of the printed page.
    /// </summary>
    [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
    public PrintPageParameters? Page { get => this.page; set => this.page = value; }

    /// <summary>
    /// Gets or sets the scale factor of the printed page.
    /// The value must be between 0.1 and 2.0 inclusive, and if omitted, defaults to 1.0.
    /// </summary>
    [JsonProperty("scale", NullValueHandling = NullValueHandling.Ignore)]
    public double? Scale
    {
        get
        {
            return this.scale;
        }

        set
        {
            if (value is not null && (value.Value < 0.1 || value.Value > 2.0))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0.1 and 2.0");
            }

            this.scale = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether or shrink the content to fit on a single page. If omitted, defaults to true.
    /// </summary>
    [JsonProperty("shrinkToFit", NullValueHandling = NullValueHandling.Ignore)]
    public bool? ShrinkToFit { get => this.shrinkToFit; set => this.shrinkToFit = value; }

    /// <summary>
    /// Gets or sets the list of page ranges to print in the resulting output.
    /// The objects of the list must be strings or longs. Other value types
    /// will cause an error when sending the browsingContext.print command.
    /// </summary>
    public List<object> PageRanges { get => this.pageRanges; set => this.pageRanges = value; }

    /// <summary>
    /// Gets the list of page ranges to print for serialization purposes.
    /// </summary>
    [JsonProperty("pageRanges", NullValueHandling = NullValueHandling.Ignore)]
    internal List<object>? SerializablePageRanges
    {
        get
        {
            if (this.pageRanges.Count == 0)
            {
                return null;
            }

            List<object> serializable = new();
            foreach (object pageRange in this.pageRanges)
            {
                if (pageRange is string || pageRange is long || pageRange is int || pageRange is short)
                {
                    serializable.Add(pageRange);
                }
                else
                {
                    throw new WebDriverBidiException("Page range must be a string or an integer value.");
                }
            }

            return serializable;
        }
    }
}