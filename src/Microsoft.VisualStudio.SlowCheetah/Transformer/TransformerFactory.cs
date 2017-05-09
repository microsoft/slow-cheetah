// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Factory for <see cref="ITransformer"/>
    /// </summary>
    public static class TransformerFactory
    {
        /// <summary>
        /// Gets the appropriate <see cref="ITransformer"/> for the given transformation
        /// </summary>
        /// <param name="source">Path to the file to be transformed</param>
        /// <param name="logger">Logger to be used in the transformer</param>
        /// <param name="useSections">Wheter or not to use sections while logging</param>
        /// <returns>The appropriate transformer for the given file</returns>
        public static ITransformer GetTransformer(string source, ITransformationLogger logger, bool useSections)
        {
            if (IsJsonFile(source))
            {
                return new JsonTransformer(logger);
            }
            else if (IsXmlFile(source))
            {
                return new XmlTransformer(logger, useSections);
            }
            else
            {
                throw new NotSupportedException($"{source} is not a supported file type for transformation");
            }
        }

        /// <summary>
        /// Verifies if a file is in XML format.
        /// Attempts to open a file using an XML Reader.
        /// </summary>
        /// <param name="filepath">Full path to the file</param>
        /// <returns>True is the file is XML</returns>
        public static bool IsXmlFile(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
            {
                throw new ArgumentNullException(nameof(filepath));
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

        /// <summary>
        /// Verifies if the given file is JSON
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <returns>True if the file is JSON</returns>
        public static bool IsJsonFile(string filePath)
        {
            return Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies if a file is of a supported format.
        /// JSON or XML
        /// </summary>
        /// <param name="filepath">Full path to the file</param>
        /// <returns>True is the file type is supported</returns>
        public static bool IsSupportedFile(string filepath)
        {
            return IsJsonFile(filepath) || IsXmlFile(filepath);
        }
    }
}
