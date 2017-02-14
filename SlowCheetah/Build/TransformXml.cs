// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah
{
    using System;
    using Microsoft.Build.Framework;

    /// <summary>
    /// Task that performs the transformation of the XML file
    /// </summary>
    public class TransformXml : Microsoft.Build.Utilities.Task
    {
        /// <summary>
        /// Gets or sets the source file path for the transformation
        /// </summary>
        [Required]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the transformation file path
        /// </summary>
        [Required]
        public string Transform { get; set; }

        /// <summary>
        /// Gets or sets the destination path for the transformation
        /// </summary>
        [Required]
        public string Destination { get; set; }

        /// <inheritdoc/>
        public override bool Execute()
        {
            ITransformer transformer = new XmlTransformer();

            try
            {
                this.Log.LogMessage("Beginning transformation.");
                transformer.Transform(this.Source, this.Transform, this.Destination);
            }
            catch (Exception e)
            {
                this.Log.LogErrorFromException(e);
            }
            finally
            {
                this.Log.LogMessage(this.Log.HasLoggedErrors ?
                    "Transformation failed." :
                    "Transformation succeeded");
            }

            // TO DO: Transforms for different file types
            // TO DO: Logging level: catch exceptions or log inside transformer?
            return !this.Log.HasLoggedErrors;
        }
    }
}
