// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Class that contains base initialization methods for all the unit tests, such as creating and deleting temporary files.
    /// </summary>
    public class BaseTest : IDisposable
    {
        /// <summary>
        /// Gets the list of temporary files to delete after test
        /// </summary>
        protected IList<string> FilesToDeleteAfterTest { get; } = new List<string>();

        /// <summary>
        /// At the end of tests, attempts to delete all the files generated during the test.
        /// </summary>
        public void Dispose()
        {
            foreach (string filename in this.FilesToDeleteAfterTest)
            {
                if (File.Exists(filename))
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch (System.IO.IOException)
                    {
                        // some processes will hold onto the file until the AppDomain is unloaded
                    }
                }
            }

            this.FilesToDeleteAfterTest.Clear();
        }

        /// <summary>
        /// Writes a string to a temporary file.
        /// </summary>
        /// <param name="content">Content to be written.</param>
        /// <returns>The path of the created file.</returns>
        protected virtual string WriteTextToTempFile(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException("content");
            }

            string tempFile = this.GetTempFilename(true);
            File.WriteAllText(tempFile, content);
            return tempFile;
        }

        /// <summary>
        /// Creates a temporary file for testing.
        /// </summary>
        /// <param name="ensureFileDoesntExist">If it is ensured that a file with the same name doesn't already exist.</param>
        /// <returns>The path to the created file.</returns>
        protected virtual string GetTempFilename(bool ensureFileDoesntExist)
        {
            string path = Path.GetTempFileName();
            if (ensureFileDoesntExist && File.Exists(path))
            {
                File.Delete(path);
            }

            this.FilesToDeleteAfterTest.Add(path);
            return path;
        }
    }
}
