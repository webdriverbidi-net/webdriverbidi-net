// <copyright file="StoredContentRegistrar.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.DemoWebSite;

using PinchHitter;

/// <summary>
/// A helper class for registering handlers for content stored on permanent storage.
/// At some point, it might be more appropriate to move this class into the main
/// server library.
/// </summary>
public class StoredContentRegistrar
{
    private static readonly Dictionary<string, string> knownMimeTypes = new()
    {
        { ".html", "text/html;charset=utf-8" },
        { ".css", "text/css" },
        { ".js", "text/javascript" },
        { ".gif", "image/gif" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
    };

    /// <summary>
    /// Recursively registers all files and directories in a given directory as resources that can be served.
    /// </summary>
    /// <param name="server">The Server instance to use to register the resources.</param>
    /// <param name="relativeUrlPath">The relative URL to which each file and directory will be accessible.</param>
    /// <param name="rootDirectory">The full path of the directory to register.</param>
    public static void RegisterDirectory(Server server, string relativeUrlPath, string rootDirectory)
    {
        if (Directory.Exists(rootDirectory))
        {
            foreach (string file in Directory.EnumerateFiles(rootDirectory))
            {
                RegisterFile(server, relativeUrlPath, file);
            }

            foreach (string directory in Directory.EnumerateDirectories(rootDirectory))
            {
                string relativeDirectory = GetDirectoryName(directory);
                string relativeSubdirectory = $"{relativeUrlPath}/{relativeDirectory}";
                RegisterDirectory(server, relativeSubdirectory, directory);
            }
        }
    }

    /// <summary>
    /// Registers a file as a resource that can be served.
    /// </summary>
    /// <param name="server">The Server instance to use to register the resources.</param>
    /// <param name="relativeUrlPath">The relative URL path under which the file will be accessible. This should not include the file name.</param>
    /// <param name="fileToRegister">The full path and file name of the file to register.</param>
    public static void RegisterFile(Server server, string relativeUrlPath, string fileToRegister)
    {
        string fileExtension = Path.GetExtension(fileToRegister);
        byte[] fileContent = File.ReadAllBytes(fileToRegister);
        string relativeUrl = $"{relativeUrlPath}/{Path.GetFileName(fileToRegister)}";
        string mimeType = "text/plain";
        if (knownMimeTypes.ContainsKey(fileExtension))
        {
            mimeType = knownMimeTypes[fileExtension];
        }

        HttpRequestHandler handler = new WebResourceRequestHandler(fileContent)
        {
            MimeType = mimeType,
        };
        server.RegisterHandler(relativeUrl, handler);
    }

    private static string GetDirectoryName(string directoryPath)
    {
        if (directoryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            directoryPath = directoryPath.Substring(0, directoryPath.Length - 1);
        }

        return Path.GetFileName(directoryPath);
    }
}
