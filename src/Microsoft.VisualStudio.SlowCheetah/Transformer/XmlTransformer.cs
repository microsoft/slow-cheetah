// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System;
    using System.IO;
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
        public bool Transform(string source, string transform, string destination)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentNullException(nameof(transform));
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (!File.Exists(source))
            {
                throw new FileNotFoundException(Resources.Resources.ErrorMessage_SourceFileNotFound, source);
            }

            if (!File.Exists(transform))
            {
                throw new FileNotFoundException(Resources.Resources.ErrorMessage_TransformFileNotFound, transform);
            }

            using (XmlTransformableDocument document = new XmlTransformableDocument())
            using (XmlTransformation transformation = new XmlTransformation(transform, this.logger))
            {
                document.PreserveWhitespace = true;
                document.Load(source);

                var success = transformation.Apply(document);
                if (success)
                {
                    document.Save(destination);
                }

                return success;
            }
        }
    }
}
