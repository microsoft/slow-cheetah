// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Factory for <see cref="ITransformer"/>
    /// </summary>
    public static class TransformerFactory
    {
        private static readonly List<ITransformer> TransformerCatalog = new List<ITransformer>()
        {
            new JsonTransformer(),
            new XmlTransformer(),
        };

        /// <summary>
        /// Gets the appropriate <see cref="ITransformer"/> for the given transformation
        /// </summary>
        /// <param name="source">Path to the file to be transformed</param>
        /// <param name="logger">Logger to be used in the transformer</param>
        /// <returns>The appropriate transformer for the given file</returns>
        public static ITransformer GetTransformer(string source, ITransformationLogger logger)
        {
            foreach (ITransformer transformer in TransformerCatalog)
            {
                if (transformer.IsFileSupported(source))
                {
                    if (logger != null)
                    {
                        transformer.SetLogger(logger);
                    }

                    return transformer;
                }
            }

            throw new NotSupportedException(string.Format(Resources.Resources.ErrorMessage_UnsupportedFile, source));
        }

        /// <summary>
        /// Verifies if a file is of a supported format.
        /// JSON or XML
        /// </summary>
        /// <param name="filepath">Full path to the file</param>
        /// <returns>True is the file type is supported</returns>
        public static bool IsSupportedFile(string filepath)
        {
            foreach (ITransformer transformer in TransformerCatalog)
            {
                if (transformer.IsFileSupported(filepath))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
