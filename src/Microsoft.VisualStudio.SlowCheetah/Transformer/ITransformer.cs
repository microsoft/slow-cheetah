// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    /// <summary>
    /// Interface for file tranformers
    /// </summary>
    public interface ITransformer
    {
        /// <summary>
        /// Main method that tranforms a source file accoring to a transformation file and puts it in a destination file
        /// </summary>
        /// <param name="source">Path to source file</param>
        /// <param name="transform">Path to tranformation file</param>
        /// <param name="destination">Path to destination of transformed file</param>
        /// <returns>True if the transform succeeded</returns>
        bool Transform(string source, string transform, string destination);
    }
}
