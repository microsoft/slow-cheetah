// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System;
    using System.IO;

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
        /// <returns>The instance of </returns>
        public static ITransformer GetTransformer(string source, ITransformationLogger logger, bool useSections)
        {
            if (IsJsonFile(source))
            {
                return new JsonTransformer();
            }
            else
            {
                return new XmlTransformer(logger, useSections);
            }
        }

        private static bool IsJsonFile(string filePath)
        {
            return Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase);
        }
    }

}
