// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah.Tests.BuildTests
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
