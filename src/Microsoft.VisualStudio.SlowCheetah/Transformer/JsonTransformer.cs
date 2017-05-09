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
    public class JsonTransformer : ITransformer
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
        public bool Transform(string source, string transform, string destination)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrWhiteSpace(transform))
            {
                throw new ArgumentNullException(nameof(transform));
            }

            if (string.IsNullOrWhiteSpace(destination))
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (!File.Exists(source))
            {
                throw new FileNotFoundException("File to transform not found", source);
            }

            if (!File.Exists(transform))
            {
                throw new FileNotFoundException("Transform file not found", transform);
            }

            JsonTransformation transformation = new JsonTransformation(transform, this.logger);

            bool success = true;

            using (Stream result = GetResultStream(transformation, source))
            {
                if (result != null)
                {
                    try
                    {
                        using (Stream destinationStream = File.Create(destination))
                        {
                            result.CopyTo(destinationStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogErrorFromException(ex);
                        success = false;
                    }
                }
                else
                {
                    success = false;
                }
            }

            return success;
        }

        private static Stream GetResultStream(JsonTransformation transformation, string sourceFile)
        {
            Stream result = null;
            try
            {
                result = transformation.Apply(sourceFile);
            }
            catch
            {
                // JDT exceptions are handled by it's own logger
            }

            return result;
        }
    }
}
