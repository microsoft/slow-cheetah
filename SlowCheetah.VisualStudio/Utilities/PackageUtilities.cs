// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

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
                throw new ArgumentNullException("filepath");
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
                if (!extension.StartsWith("."))
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
        /// Verifies if a file is in XML format.
        /// Attempts to open a file using an XML Reader.
        /// </summary>
        /// <param name="filepath">Full path to the </param>
        /// <returns>True is the file is XML</returns>
        public static bool IsXmlFile(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
            {
                throw new ArgumentNullException("filepath");
            }

            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("File not found", filepath);
            }

            bool isXmlFile = true;
            try
            {
                using (XmlTextReader xmlTextReader = new XmlTextReader(filepath))
                {
                    // This is required because if the XML file has a DTD then it will try and download the DTD!
                    xmlTextReader.DtdProcessing = DtdProcessing.Ignore;
                    xmlTextReader.Read();
                }
            }
            catch (XmlException)
            {
                isXmlFile = false;
            }

            return isXmlFile;
        }
    }
}
