// <copyright file="BrowserPlatform.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Runtime.InteropServices;

/// <summary>
/// The operating system and processor architecture for which browsers and drivers are located and downloaded.
/// </summary>
/// <param name="OperatingSystem">The operating system.</param>
/// <param name="Architecture">The processor architecture.</param>
public sealed record BrowserPlatform(OperatingSystemFamily OperatingSystem, Architecture Architecture)
{
    /// <summary>
    /// Gets the platform of the current process.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Thrown when the current operating system is not Windows, macOS, or Linux.</exception>
    public static BrowserPlatform Current
    {
        get
        {
            OperatingSystemFamily operatingSystem;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                operatingSystem = OperatingSystemFamily.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                operatingSystem = OperatingSystemFamily.MacOS;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                operatingSystem = OperatingSystemFamily.Linux;
            }
            else
            {
                throw new PlatformNotSupportedException($"Browsers cannot be located on this operating system ({RuntimeInformation.OSDescription}).");
            }

            return new BrowserPlatform(operatingSystem, RuntimeInformation.ProcessArchitecture);
        }
    }

    /// <summary>
    /// Returns a string describing the platform, such as "Linux-X64".
    /// </summary>
    /// <returns>A string describing the platform.</returns>
    public override string ToString()
    {
        return $"{this.OperatingSystem}-{this.Architecture}";
    }
}
