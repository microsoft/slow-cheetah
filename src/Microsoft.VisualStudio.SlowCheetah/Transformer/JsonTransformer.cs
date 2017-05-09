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
                throw new ArgumentException($"{nameof(source)} cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(transform))
            {
                throw new ArgumentException($"{nameof(transform)} cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(destination))
            {
                throw new ArgumentException($"{nameof(destination)} cannot be null or whitespace");
            }

            if (!File.Exists(source))
            {
                throw new FileNotFoundException(Resources.Resources.ErrorMessage_SourceFileNotFound, source);
            }

            if (!File.Exists(transform))
            {
                throw new FileNotFoundException(Resources.Resources.ErrorMessage_TransformFileNotFound, transform);
            }

            var transformation = new JsonTransformation(transform, this.logger);

            try
            {
                using (Stream result = transformation.Apply(source))
                {
                    return this.TrySaveToFile(result, destination);
                }
            }
            catch
            {
                // JDT exceptions are handled by it's own logger
                return false;
            }
        }

        private bool TrySaveToFile(Stream result, string destinationFile)
        {
            try
            {
                System.Text.Encoding encoding;
                string contents;
                using (StreamReader reader = new StreamReader(result, true))
                {
                    // Get the contents and the encoding of the result stram
                    reader.Peek();
                    encoding = reader.CurrentEncoding;
                    contents = reader.ReadToEnd();
                }

                // Make sure to save it in the encoding of the result stream
                File.WriteAllText(destinationFile, contents, encoding);

                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogErrorFromException(ex);
                return false;
            }
        }
    }
}
