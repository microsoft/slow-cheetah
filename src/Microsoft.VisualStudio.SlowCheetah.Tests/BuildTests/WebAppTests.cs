// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.Tests.BuildTests
{
    using System.IO;
    using Xunit;

    /// <summary>
    /// Tests build time transformations for a test web app
    /// </summary>
    [Collection("BuildTests")]
    public class WebAppTests : ConfigTransformTestsBase
    {
        /// <summary>
        /// Tests if other.config is transformed on build
        /// </summary>
        [Fact]
        public void WebApp_OtherConfig_IsTransformed()
        {
            var projectName = "WebApplication";
            this.BuildProject(projectName);

            var configFilePath = Path.Combine(this.OutputPath, "Other.config");

            var testNodeValue = this.GetConfigNodeValue(configFilePath, "TestNode");

            Assert.Equal("Debug", testNodeValue);
        }
    }
}
