// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    /// <summary>
    /// Transforms JSON files using JSON Document Transformations
    /// </summary>
    public class JsonTransformer : TransformerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTransformer"/> class.
        /// </summary>
        public JsonTransformer()
        {
        }

        /// <inheritdoc/>
        public override bool Transform(string source, string transform, string destination)
        {
            this.ValidateArguments(source, transform, destination);

            return false;
        }
    }
}
