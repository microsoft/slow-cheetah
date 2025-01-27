// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Utilities class for the Visual Studio Extension Package.
    /// </summary>
    public class PackageUtilities
    {
        /// <summary>
        /// Set of extensions we explicitly do not support transforms for.
        /// We use this avoid hitting the disk in most scenarios.
        /// </summary>
        private static readonly HashSet<string> ExcludedExtensions = new HashSet<string>()
        {
            ".dll",
            ".pdb",
            ".txt",
            ".settings",
            ".snk",

            // source files
            ".aspx",
            ".c",
            ".cpp",
            ".cs",
            ".cshtml",
            ".css",
            ".h",
            ".htm",
            ".html",
            ".js",
            ".jsx",
            ".less",
            ".resx",
            ".ts",
            ".sass",
            ".vb",
            ".vbs",
            ".wsf",

            // image files
            ".bmp",
            ".gif",
            ".ico",
            ".jpeg",
            ".jpg",
            ".png",
        };

        /// <summary>
        /// Verifies if the extension of the given file is supported.
        /// </summary>
        /// <param name="filePath">Full path to the file.</param>
        /// <returns>True if the file is supported.</returns>
        public static bool IsExtensionSupportedForFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            return !ExcludedExtensions.Contains(Path.GetExtension(filePath));
        }

        /// <summary>
        /// Creates a temporary file name with an optional extension.
        /// </summary>
        /// <param name="ensureFileDoesntExist">Whether it is ensured the file does not exist.</param>
        /// <param name="extension">Optional extension for the file.</param>
        /// <returns>Full path to the temporary file.</returns>
        public static string GetTempFilename(bool ensureFileDoesntExist, string extension = null)
        {
            string path = Path.GetTempFileName();

            if (!string.IsNullOrWhiteSpace(extension))
            {
                // delete the file at path and then add the extension to it
                File.Delete(path);

                extension = extension.Trim();
                if (!extension.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                {
                    extension = "." + extension;
                }

                path += extension;
            }

            if (ensureFileDoesntExist && File.Exists(path))
            {
                File.Delete(path);
            }

            return path;
        }

        /// <summary>
        /// Checks if a file is a transform of another file according to their names and the given configurations
        /// If a given file is  "name.extension", a transfomation file should be "name.{configuration}.extension".
        /// </summary>
        /// <param name="documentName">Name of the source file.</param>
        /// <param name="transformName">Name of the potential transform file.</param>
        /// <param name="configs">Project configurations.</param>
        /// <returns>True if the names correspond to compatible transformation files.</returns>
        public static bool IsFileTransformForBuildConfiguration(string documentName, string transformName, IEnumerable<string> configs)
        {
            if (configs == null || !configs.Any())
            {
                return false;
            }

            if (TryGetFileTransform(documentName, transformName, out string config))
            {
                return configs.Any(s => s.Equals(config, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a file is a generic transform of another file
        /// If a given file is  "name.extension", a transfomation file should be "name.{something}.extension".
        /// </summary>
        /// <param name="documentName">Name of the source file.</param>
        /// <param name="transformName">Name of the potential transform file.</param>
        /// <returns>True if the names correspond to compatible transformation files.</returns>
        public static bool IsGenericFileTransform(string documentName, string transformName)
        {
            return TryGetFileTransform(documentName, transformName, out _);
        }

        /// <summary>
        /// Gets if a path is valid or not.
        /// </summary>
        /// <param name="path">The path to check if it is valid.</param>
        /// <returns>True if path is valid, false otherwise.</returns>
        public static bool IsPathValid(string path)
        {
            // empty path is valid
            if (string.IsNullOrEmpty(path))
            {
                return true;
            }

            return !path.Any(c => Path.GetInvalidPathChars().Contains(c));
        }

        /// <summary>
        /// Gets an item from the project hierarchy.
        /// </summary>
        /// <typeparam name="T">Type of object to be fetched.</typeparam>
        /// <param name="pHierarchy">Current IVsHierarchy.</param>
        /// <param name="itemID">ID of the desired item in the project.</param>
        /// <returns>The desired object typed to T.</returns>
        public static T GetAutomationFromHierarchy<T>(IVsHierarchy pHierarchy, uint itemID)
            where T : class
        {
            ErrorHandler.ThrowOnFailure(pHierarchy.GetProperty(itemID, (int)__VSHPROPID.VSHPROPID_ExtObject, out object propertyValue));
            T projectItem = propertyValue as T;

            return projectItem;
        }

        private static bool TryGetFileTransform(string documentName, string transformName, out string config)
        {
            config = null;

            if (string.IsNullOrEmpty(documentName))
            {
                return false;
            }

            if (string.IsNullOrEmpty(transformName))
            {
                return false;
            }

            if (!Path.GetExtension(documentName).Equals(Path.GetExtension(transformName), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            else
            {
                string docNameNoExt = Path.GetFileNameWithoutExtension(documentName);
                string trnNameNoExt = Path.GetFileNameWithoutExtension(transformName);
                Regex regex = new Regex("^" + docNameNoExt + @"\.", RegexOptions.IgnoreCase);
                config = regex.Replace(trnNameNoExt, string.Empty);
                return !string.IsNullOrEmpty(config) && !config.Equals(trnNameNoExt, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
