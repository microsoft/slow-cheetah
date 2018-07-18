// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System;
    using System.IO;
    using System.Xml;
    using Microsoft.Web.XmlTransform;

    /// <summary>
    /// Transforms XML files utilizing Microsoft Web XmlTransform library
    /// </summary>
    public class XmlTransformer : ITransformer
    {
        private const string XdtAttributeName = "xmlns:xdt";
        private const string XdtAttributeValue = "http://schemas.microsoft.com/XML-Document-Transform";

        private IXmlTransformationLogger logger = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTransformer"/> class.
        /// </summary>
        public XmlTransformer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTransformer"/> class with an external logger
        /// </summary>
        /// <param name="logger">The external logger</param>
        public XmlTransformer(ITransformationLogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = new XmlShimLogger(logger, useSections: false);
        }

        /// <inheritdoc/>
        public void CreateTransformFile(string sourcePath, string transformPath, bool overwrite)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                throw new ArgumentNullException(nameof(sourcePath));
            }

            if (string.IsNullOrWhiteSpace(transformPath))
            {
                throw new ArgumentNullException(nameof(transformPath));
            }

            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException(Resources.Resources.ErrorMessage_SourceFileNotFound, sourcePath);
            }

            // If the file should be overwritten or if it doesn't exist, we create it
            if (overwrite || !File.Exists(transformPath))
            {
                XmlDocument transformDoc = new XmlDocument();
                using (XmlTextReader reader = new XmlTextReader(sourcePath))
                {
                    reader.DtdProcessing = DtdProcessing.Ignore;
                    transformDoc.Load(reader);
                }

                XmlComment xdtInfo = transformDoc.CreateComment(Resources.Resources.XmlTransform_ContentInfo);
                XmlElement root = transformDoc.DocumentElement;
                transformDoc.InsertBefore(xdtInfo, root);
                root.SetAttribute(XdtAttributeName, XdtAttributeValue);
                root.InnerXml = string.Empty;
                transformDoc.Save(transformPath);
            }
        }

        /// <inheritdoc/>
        public bool IsFileSupported(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(Resources.Resources.ErrorMessage_FileNotFound, filePath);
            }

            try
            {
                using (XmlTextReader xmlTextReader = new XmlTextReader(filePath))
                {
                    // This is required because if the XML file has a DTD then it will try and download the DTD!
                    xmlTextReader.DtdProcessing = DtdProcessing.Ignore;
                    xmlTextReader.Read();
                    return true;
                }
            }
            catch (XmlException)
            {
            }

            return false;
        }

        /// <inheritdoc/>
        public bool Transform(string sourcePath, string transformPath, string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                throw new ArgumentNullException(nameof(sourcePath));
            }

            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                throw new ArgumentNullException(nameof(transformPath));
            }

            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                throw new ArgumentNullException(nameof(destinationPath));
            }

            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException(Resources.Resources.ErrorMessage_SourceFileNotFound, sourcePath);
            }

            if (!File.Exists(transformPath))
            {
                throw new FileNotFoundException(Resources.Resources.ErrorMessage_TransformFileNotFound, transformPath);
            }

            using (XmlTransformableDocument document = new XmlTransformableDocument())
            using (XmlTransformation transformation = new XmlTransformation(transformPath, this.logger))
            {
                document.PreserveWhitespace = true;
                document.Load(sourcePath);

                var success = transformation.Apply(document);

                if (success)
                {
                    document.Save(destinationPath);
                }

                return success;
            }
        }

        /// <inheritdoc/>
        public ITransformer WithLogger(ITransformationLogger logger)
        {
            if (logger == this.logger)
            {
                return this;
            }
            else if (logger == null)
            {
                return new XmlTransformer();
            }
            else
            {
                return new XmlTransformer(logger);
            }
        }
    }
}
