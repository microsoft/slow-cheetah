// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.Jdt;

    /// <summary>
    /// Transforms JSON files using JSON Document Transformations
    /// </summary>
    public class JsonTransformer : TransformerBase
    {
        private readonly IJsonTransformationLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTransformer"/> class.
        /// </summary>
        public JsonTransformer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTransformer"/> class with a logger
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public JsonTransformer(ITransformationLogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = new JsonShimLogger(logger);
        }

        /// <inheritdoc/>
        public override bool Transform(string source, string transform, string destination)
        {
            this.ValidateArguments(source, transform, destination);

            JsonTransformation transformation = new JsonTransformation(transform, this.logger);

            bool success = true;

            try
            {
                using (Stream result = transformation.Apply(source))
                {
                    using (Stream destinationStream = File.Create(destination))
                    {
                        result.CopyTo(destinationStream);
                    }
                }
            }
            catch
            {
                success = false;
            }

            return success;
        }
    }
}
