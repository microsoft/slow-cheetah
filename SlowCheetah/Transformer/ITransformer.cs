// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah
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
