// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Wrapper for <see cref="TransformerFactory"/>
    /// </summary>
    public static class PackageTransformerFactory
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
            return TransformerFactory.GetTransformer(source, logger, useSections);
        }

        /// <summary>
        /// Verifies if a file is in XML format.
        /// Attempts to open a file using an XML Reader.
        /// </summary>
        /// <param name="filepath">Full path to the file</param>
        /// <returns>True is the file is XML</returns>
        public static bool IsXmlFile(string filepath)
        {
            return TransformerFactory.IsXmlFile(filepath);
        }

        /// <summary>
        /// Verifies if a file is in JSON format.
        /// Checks for the appropriate extension
        /// </summary>
        /// <param name="filepath">Full path to the file</param>
        /// <returns>True is the file is JSON</returns>
        public static bool IsJsonFile(string filepath)
        {
            return TransformerFactory.IsJsonFile(filepath);
        }

        /// <summary>
        /// Verifies if a file is of a supported format.
        /// JSON or XML
        /// </summary>
        /// <param name="filepath">Full path to the file</param>
        /// <returns>True is the file type is supported</returns>
        public static bool IsSupportedFile(string filepath)
        {
            return TransformerFactory.IsSupportedFile(filepath);
        }

        /// <summary>
        /// Builds the contents of a transformation file for a given source file
        /// </summary>
        /// <param name="sourceItemPath">Full path to the file to be transformed</param>
        /// <returns>Contents of the transform file</returns>
        public static string BuildTransformContent(string sourceItemPath)
        {
            if (IsJsonFile(sourceItemPath))
            {
                return Resources.Resources.JsonTransformContents;
            }
            else if (IsXmlFile(sourceItemPath))
            {
                return BuildXdtContent(sourceItemPath);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Builds the contents of an XDT transformation file for a given XML source file
        /// </summary>
        /// <param name="sourceItemPath">Full path to the file to be transformed</param>
        /// <returns>Contents of the XML transform file.</returns>
        private static string BuildXdtContent(string sourceItemPath)
        {
            string content = string.Empty;

            using (MemoryStream contentStream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    OmitXmlDeclaration = false,
                    NewLineOnAttributes = true
                };

                XmlWriter contentWriter = XmlWriter.Create(contentStream, settings);

                using (XmlReader reader = XmlReader.Create(sourceItemPath))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.XmlDeclaration)
                        {
                            contentWriter.WriteNode(reader, false);
                            contentWriter.WriteWhitespace(Environment.NewLine);
                            contentWriter.WriteComment(Resources.Resources.XmlTransform_ContentInfo);
                            contentWriter.WriteWhitespace(Environment.NewLine);
                        }
                        else if (reader.NodeType == XmlNodeType.Element)
                        {
                            contentWriter.WriteStartElement(reader.Name, reader.NamespaceURI);
                            for (int index = 0; index < reader.AttributeCount; index++)
                            {
                                reader.MoveToAttribute(index);
                                if (reader.Prefix == "xmlns" && reader.Name != "xmlns:xdt")
                                {
                                    string nsName = reader.LocalName;
                                    string nsValue = reader.GetAttribute(index);
                                    contentWriter.WriteAttributeString("xmlns", nsName, null, nsValue);
                                }
                            }

                            contentWriter.WriteAttributeString("xmlns", "xdt", null, "http://schemas.microsoft.com/XML-Document-Transform");
                            contentWriter.WriteWhitespace(Environment.NewLine);
                            contentWriter.WriteEndElement();
                            contentWriter.WriteEndDocument();
                            break;
                        }
                    }

                    contentWriter.Flush();

                    contentStream.Seek(0, SeekOrigin.Begin);
                    using (StreamReader contentReader = new StreamReader(contentStream))
                    {
                        content += contentReader.ReadToEnd();
                    }
                }
            }

            return content;
        }
    }
}
