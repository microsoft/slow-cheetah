// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.Jdt;

    /// <summary>
    /// Transforms JSON files using JSON Document Transformations.
    /// </summary>
    public class JsonTransformer : ITransformer
    {
        // Contents of a newly created transform file
        private static readonly string TransformFileContents = "{" + Environment.NewLine + "}" + Environment.NewLine;

        private IJsonTransformationLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTransformer"/> class.
        /// </summary>
        public JsonTransformer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTransformer"/> class with an external logger.
        /// </summary>
        /// <param name="logger">The external logger.</param>
        public JsonTransformer(ITransformationLogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = new JsonShimLogger(logger);
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
                var encoding = TransformUtilities.GetEncoding(sourcePath);
                File.WriteAllText(transformPath, Resources.Resources.JsonTransform_TransformFileContents, encoding);
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

            return Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public bool Transform(string sourcePath, string transformPath, string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                throw new ArgumentException($"{nameof(sourcePath)} cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(transformPath))
            {
                throw new ArgumentException($"{nameof(transformPath)} cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(destinationPath))
            {
                throw new ArgumentException($"{nameof(destinationPath)} cannot be null or whitespace");
            }

            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException(Resources.Resources.ErrorMessage_SourceFileNotFound, sourcePath);
            }

            if (!File.Exists(transformPath))
            {
                throw new FileNotFoundException(Resources.Resources.ErrorMessage_TransformFileNotFound, transformPath);
            }

            var transformation = new JsonTransformation(transformPath, this.logger);

            try
            {
                using (Stream result = transformation.Apply(sourcePath))
                {
                    return this.TrySaveToFile(result, sourcePath, destinationPath);
                }
            }
            catch
            {
                // JDT exceptions are handled by it's own logger
                return false;
            }
        }

        /// <inheritdoc/>
        public ITransformer WithLogger(ITransformationLogger logger)
        {
            if (logger == this.logger)
            {
                return this;
            }
            else if (logger == null)
            {
                return new JsonTransformer();
            }
            else
            {
                return new JsonTransformer(logger);
            }
        }

        private bool TrySaveToFile(Stream result, string sourceFile, string destinationFile)
        {
            try
            {
                string contents;
                var encoding = TransformUtilities.GetEncoding(sourceFile);
                using (StreamReader reader = new StreamReader(result, true))
                {
                    // Get the contents of the result stram
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
