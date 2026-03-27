// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.Jdt;

    /// <summary>
    /// Transforms Ini files
    /// </summary>
    public class IniTransformer : ITransformer
    {
        private IJsonTransformationLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IniTransformer"/> class.
        /// </summary>
        public IniTransformer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IniTransformer"/> class with an external logger
        /// </summary>
        /// <param name="logger">The external logger</param>
        public IniTransformer(ITransformationLogger logger)
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
                File.WriteAllText(transformPath, string.Empty, encoding);
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

            return Path.GetExtension(filePath).Equals(".ini", StringComparison.OrdinalIgnoreCase);
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

            try
            {
                Dictionary<string, string> transformDictionary = new Dictionary<string, string>();
                string currentRoot = null;
                string[] keyPair = null;

                if (!System.IO.File.Exists(sourcePath) || !System.IO.File.Exists(transformPath))
                {
                    return false;
                }

                // Collect what we need to replace
                using (StreamReader iniFile = new StreamReader(transformPath))
                {
                    string strLine = iniFile.ReadLine();
                    while (strLine != null)
                    {
                        if (!string.IsNullOrEmpty(strLine))
                        {
                            if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                            {
                                currentRoot = strLine.Substring(1, strLine.Length - 2);
                            }
                            else if (!strLine.StartsWith(";") && !strLine.StartsWith("#"))
                            {
                                keyPair = strLine.Split(new char[] { '=' }, 2);
                                if (keyPair.Length > 1 && !transformDictionary.ContainsKey(currentRoot + "-" + keyPair[0]))
                                {
                                    transformDictionary.Add(currentRoot + "-" + keyPair[0], keyPair[1]);
                                }
                            }
                        }
                        strLine = iniFile.ReadLine();
                    }
                }

                // read the source file and replace lines where is necessary
                using (StreamReader iniFile = new StreamReader(sourcePath))
                {
                    using (StreamWriter destFile = new StreamWriter(destinationPath))
                    {
                        string strLine = iniFile.ReadLine();
                        while (strLine != null)
                        {
                            if (!string.IsNullOrEmpty(strLine))
                            {
                                if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                                {
                                    currentRoot = strLine.Substring(1, strLine.Length - 2);
                                }
                                else if (!strLine.StartsWith(";") && !strLine.StartsWith("#"))
                                {
                                    keyPair = strLine.Split(new char[] { '=' }, 2);
                                    if (keyPair.Length > 1 && transformDictionary.ContainsKey(currentRoot + "-" + keyPair[0]))
                                    {
                                        strLine = keyPair[0] + "=" + transformDictionary[currentRoot + "-" + keyPair[0]];
                                    }
                                }
                                destFile.WriteLine(strLine);
                            }
                            else
                            {
                                destFile.WriteLine(strLine);
                            }

                            strLine = iniFile.ReadLine();
                        }
                    }
                }

                return true;
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
                return new IniTransformer();
            }
            else
            {
                return new IniTransformer(logger);
            }
        }
    }
}
