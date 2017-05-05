// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System.Diagnostics.Contracts;
    using System.IO;

    /// <summary>
    /// Contains base methods for <see cref="ITransformer"/>
    /// </summary>
    public abstract class TransformerBase : ITransformer
    {
        /// <inheritdoc/>
        public abstract bool Transform(string source, string transform, string destination);

        /// <summary>
        /// Validates the given string for transformation
        /// </summary>
        /// <param name="source">Path to source file</param>
        /// <param name="transform">Path to tranformation file</param>
        /// <param name="destination">Path to destination of transformed file</param>
        protected void ValidateArguments(string source, string transform, string destination)
        {
            // Parameter validation
            Contract.Requires(!string.IsNullOrWhiteSpace(source));
            Contract.Requires(!string.IsNullOrWhiteSpace(transform));
            Contract.Requires(!string.IsNullOrWhiteSpace(destination));

            // File validation
            if (!File.Exists(source))
            {
                throw new FileNotFoundException("File to transform not found", source);
            }

            if (!File.Exists(transform))
            {
                throw new FileNotFoundException("Transform file not found", transform);
            }
        }
    }
}
