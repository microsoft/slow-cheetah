// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using Microsoft.Build.Framework;

    /// <summary>
    /// Task that performs the transformation of the XML file.
    /// </summary>
    public class TransformTask : Microsoft.Build.Utilities.Task
    {
        /// <summary>
        /// Gets or sets the source file path for the transformation.
        /// </summary>
        [Required]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the transformation file path.
        /// </summary>
        [Required]
        public string Transform { get; set; }

        /// <summary>
        /// Gets or sets the destination path for the transformation.
        /// </summary>
        [Required]
        public string Destination { get; set; }

        /// <inheritdoc/>
        public override bool Execute()
        {
            TransformationTaskLogger logger = new TransformationTaskLogger(this.Log);

            ITransformer transformer = TransformerFactory.GetTransformer(this.Source, logger);

            this.Log.LogMessage("Beginning transformation.");

            bool success = transformer.Transform(this.Source, this.Transform, this.Destination);
            success = success && !this.Log.HasLoggedErrors;

            this.Log.LogMessage(success ?
                    "Transformation succeeded." :
                    "Transformation failed.");

            return success;
        }
    }
}
