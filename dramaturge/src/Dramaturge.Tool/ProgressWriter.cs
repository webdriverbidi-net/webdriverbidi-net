// <copyright file="ProgressWriter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Tool;

using System.Globalization;
using Dramaturge.Browsers;

/// <summary>
/// Writes a line as each download starts and passes each quarter of its size, as the download reports it.
/// </summary>
/// <param name="writer">The writer.</param>
internal sealed class ProgressWriter(TextWriter writer) : IProgress<BrowserDownloadProgress>
{
    private readonly Dictionary<string, int> reportedQuarters = [];

    /// <inheritdoc/>
    public void Report(BrowserDownloadProgress value)
    {
        lock (this.reportedQuarters)
        {
            // Reported synchronously by the downloader, so the lines are in order with the tool's other output.
            long total = value.TotalBytes ?? 0;
            int quarter = total > 0 ? (int)Math.Min(4, value.BytesReceived * 4 / total) : 0;
            if (this.reportedQuarters.TryGetValue(value.Name, out int reported) && quarter <= reported)
            {
                return;
            }

            this.reportedQuarters[value.Name] = quarter;
            string size = value.TotalBytes is long totalBytes ? string.Format(CultureInfo.InvariantCulture, " of {0:0.0} MB", totalBytes / 1048576.0) : string.Empty;
            writer.WriteLine($"Downloading {value.Name}{size}: {quarter * 25}%");
        }
    }
}
