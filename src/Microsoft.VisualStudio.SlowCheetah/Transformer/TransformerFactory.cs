// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Factory for <see cref="ITransformer"/>
    /// </summary>
    public static class TransformerFactory
    {
        private static readonly List<ITransformer> TransformerCatalog = new List<ITransformer>()
        {
            new JsonTransformer(),
            new XmlTransformer(),
            new IniTransformer()
        };

        /// <summary>
        /// Gets the appropriate <see cref="ITransformer"/> for the given transformation
        /// </summary>
        /// <param name="source">Path to the file to be transformed</param>
        /// <param name="logger">Logger to be used in the transformer</param>
        /// <returns>The appropriate transformer for the given file</returns>
        public static ITransformer GetTransformer(string source, ITransformationLogger logger)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException(nameof(source));
            }

            return TransformerCatalog.FirstOrDefault(tr => tr.IsFileSupported(source))?.WithLogger(logger)
                ?? throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.Resources.ErrorMessage_UnsupportedFile, source));
        }

        /// <summary>
        /// Verifies if a file is of a supported format.
        /// </summary>
        /// <param name="filePath">Full path to the file</param>
        /// <returns>True is the file type is supported</returns>
        public static bool IsSupportedFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            return TransformerCatalog.Any(tr => tr.IsFileSupported(filePath));
        }
    }
}
