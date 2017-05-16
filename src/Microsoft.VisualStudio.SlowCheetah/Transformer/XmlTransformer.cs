// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System;
    using System.IO;
    using Microsoft.Web.XmlTransform;
    using System.Xml;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTransformer"/> class with an external logger.
        /// </summary>
        /// <param name="logger">External logger. Passed into the transformation</param>
        /// <param name="useSections">Wheter or not to use sections while logging</param>
        public XmlTransformer(ITransformationLogger logger, bool useSections)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = new XmlShimLogger(logger, useSections);
        }

        /// <inheritdoc/>
        public void CreateTransformFile(string sourcePath, string transformPath)
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
                throw new FileNotFoundException("File not found", sourcePath);
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
                throw new FileNotFoundException("File not found", filePath);
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
