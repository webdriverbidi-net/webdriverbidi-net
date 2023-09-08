// <copyright file="PrintPageParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Parameters of page size for printing.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class PrintPageParameters
{
    private double? height;
    private double? width;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrintPageParameters"/> class.
    /// </summary>
    public PrintPageParameters()
    {
    }

    /// <summary>
    /// Gets or sets the width in centimeters of the page for printing.
    /// The value must be greater than or equsl to zero, and if omitted, defaults to 21.59.
    /// </summary>
    [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
    public double? Width
    {
        get
        {
            return this.width;
        }

        set
        {
            if (value is not null && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero");
            }

            this.width = value;
        }
    }

    /// <summary>
    /// Gets or sets the height in centimeters of the page for printing.
    /// The value must be greater than or equsl to zero, and if omitted, defaults to 27.94.
    /// </summary>
    [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
    public double? Height
    {
        get
        {
            return this.height;
        }

        set
        {
            if (value is not null && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero");
            }

            this.height = value;
        }
    }
}