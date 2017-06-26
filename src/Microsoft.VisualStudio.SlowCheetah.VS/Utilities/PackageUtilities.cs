// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Utilities class for the Visual Studio Extension Package
    /// </summary>
    public class PackageUtilities
    {
        /// <summary>
        /// List of extensions that should not be transformed
        /// </summary>
        public static readonly IReadOnlyCollection<string> ExcludedExtensions = new List<string>(new string[] { ".htm", ".html", ".cs", ".vb", ".txt", ".jpg", ".png", ".ico", ".aspx", ".snk", ".dll", ".pdb", ".settings" });

        /// <summary>
        /// Verifies if the extension of the given file is supported
        /// </summary>
        /// <param name="filepath">Full path to the file</param>
        /// <returns>True if the file is supported</returns>
        public static bool IsExtensionSupportedForFile(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
            {
                throw new ArgumentNullException(nameof(filepath));
            }

            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("File not found", filepath);
            }

            FileInfo fi = new FileInfo(filepath);

            return !ExcludedExtensions.Any(ext => string.Equals(fi.Extension, ext, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Creates a temporary file name with an optional extension
        /// </summary>
        /// <param name="ensureFileDoesntExist">Whether it is ensured the file does not exist</param>
        /// <param name="extension">Optional extension for the file</param>
        /// <returns>Full path to the temporary file</returns>
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
        /// Checks if a file is a transform of another file according to their names.
        /// If a given file is  "name.extension", a transfomation file should be "name.configuration.extension"
        /// </summary>
        /// <param name="documentName">File to be transformed</param>
        /// <param name="transformName">File to</param>
        /// <param name="configs">Project configurations</param>
        /// <returns>True if the name</returns>
        public static bool IsFileTransform(string documentName, string transformName, IEnumerable<string> configs)
        {
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
                string configName = regex.Replace(trnNameNoExt, string.Empty);
                return !configName.Equals(trnNameNoExt) && configs.Any(s => { return string.Compare(s, configName, StringComparison.OrdinalIgnoreCase) == 0; });
            }
        }

        /// <summary>
        /// Gets an item from the project hierarchy
        /// </summary>
        /// <typeparam name="T">Type of object to be fetched</typeparam>
        /// <param name="pHierarchy">Current IVsHierarchy</param>
        /// <param name="itemID">ID of the desired item in the project</param>
        /// <returns>The desired object typed to T</returns>
        public static T GetAutomationFromHierarchy<T>(IVsHierarchy pHierarchy, uint itemID)
            where T : class
        {
            ErrorHandler.ThrowOnFailure(pHierarchy.GetProperty(itemID, (int)__VSHPROPID.VSHPROPID_ExtObject, out object propertyValue));
            T projectItem = propertyValue as T;

            return projectItem;
        }
    }
}
