using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SlowCheetah.VisualStudio
{
    public class PackageUtilities
    {
        private static readonly List<string> ExcludedExtensions = new List<string>(new string[] { ".htm", ".html", ".cs.vb", ".txt", ".jpg", ".png", ".ico", ".aspx", ".snk", ".dll", ".pdb", ".settings" });

        /// <summary>
        /// Given a string which contains a list of extensions separated by semi-colons, this function sorts through the list
        /// throwing out any blank or empty items, and then makes sure that the extensions start with a ".".
        /// The validated list is returned.
        /// </summary>
        /// <param name="items">A list of extensions separated by semi-colans. The items may be prefixed with the ".", but don't have to be.</param>
        /// <returns>The validated and culled list of extensions. The returned items all begin with a ".".</returns>
        public static string[] GetExtensionList(string items)
        {
            const string extensionPrefix = ".";
            List<string> list = new List<string>();
            if (!string.IsNullOrWhiteSpace(items))
            {
                string[] extensions = items.Split(';');
                foreach (string extension in extensions)
                {
                    string extensionValue = string.IsNullOrWhiteSpace(extension) ? string.Empty : extension.Trim();
                    if (extensionValue != string.Empty)
                    {
                        if (!extensionValue.StartsWith(extensionPrefix))
                        {
                            extensionValue = extensionPrefix + extensionValue;
                        }

                        list.Add(extensionValue);
                    }
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Gets a list of unsupported transformation extensions,
        /// each starting with a ".".
        /// </summary>
        /// <returns>The list of extensions</returns>
        public static List<string> GetExcludedExtensions()
        {
            return ExcludedExtensions;
        }

        /// <summary>
        /// Verifies if the extension of the given file is supported
        /// </summary>
        /// <param name="filepath">Full path to the file</param>
        /// <returns>True if the file is supported</returns>
        public static bool IsExtensionSupportedForFile(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath)) { throw new ArgumentNullException("filepath"); }
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("File not found", filepath);
            }

            FileInfo fi = new FileInfo(filepath);

            var isExcludedQuery = from extension in GetExcludedExtensions()
                                  where string.Compare(fi.Extension, extension, StringComparison.OrdinalIgnoreCase) == 0
                                  select extension;
            var isExcluded = isExcludedQuery.Count() > 0 ? true : false;

            return !isExcluded;
        }

        /// <summary>
        /// Creates a temporary file and returns the full path
        /// </summary>
        /// <param name="ensureFileDoesntExist">Wheter it is ensured that a file with the same name exists</param>
        /// <param name="extension">Optional extension for the file</param>
        /// <returns>Full path to the file created</returns>
        public static string GetTempFilename(bool ensureFileDoesntExist, string extension = null)
        {
            string path = Path.GetTempFileName();

            if (!string.IsNullOrWhiteSpace(extension))
            {
                // delete the file at path and then add the extension to it
                if (File.Exists(path))
                {
                    File.Delete(path);

                    extension = extension.Trim();
                    if (!extension.StartsWith("."))
                    {
                        extension = "." + extension;
                    }

                    path += extension;
                }
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
            if (string.IsNullOrWhiteSpace(filepath)) { throw new ArgumentNullException("filepath"); }
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
