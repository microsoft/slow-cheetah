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
        private IXmlTransformationLogger logger = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTransformer"/> class.
        /// </summary>
        public XmlTransformer()
        {
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
                // First, copy the original file to preserve encoding and header
                File.Copy(sourcePath, transformPath, true);

                XmlDocument transformDoc = new XmlDocument();
                transformDoc.Load(transformPath);
                XmlComment xdtInfo = transformDoc.CreateComment(Resources.Resources.XmlTransform_ContentInfo);
                XmlElement root = transformDoc.DocumentElement;
                transformDoc.InsertBefore(xdtInfo, root);
                root.SetAttribute(Resources.Resources.XmlTransform_XdtAttributeName, Resources.Resources.XmlTransform_XdtAttributeValue);
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

            bool isXmlFile = true;
            try
            {
                using (XmlTextReader xmlTextReader = new XmlTextReader(filePath))
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

        /// <inheritdoc/>
        public void SetLogger(ITransformationLogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = new XmlShimLogger(logger, false);
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
    }
}
