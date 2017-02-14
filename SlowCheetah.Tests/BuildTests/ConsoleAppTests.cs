// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah.Tests.BuildTests
{
    using System.IO;
    using Xunit;

    /// <summary>
    /// Tests build time transformations for a test console app
    /// </summary>
    [Collection("BuildTests")]
    public class ConsoleAppTests : ConfigTransformTestsBase
    {
        /// <summary>
        /// Tests if app.config is transformed on build
        /// </summary>
        [Fact]
        public void ConsoleApp_AppConfig_IsTransformed()
        {
            var projectName = "ConsoleApp";
            this.BuildProject(projectName);

            var configFilePath = Path.Combine(this.OutputPath, "ConsoleApp.exe.config");

            var testSetting = this.GetAppSettingValue(configFilePath, "TestSetting");

            Assert.Equal("Debug", testSetting);
        }

        /// <summary>
        /// Tests if other.config is transformed on build
        /// </summary>
        [Fact]
        public void ConsoleApp_OtherConfig_IsTransformed()
        {
            var projectName = "ConsoleApp";
            this.BuildProject(projectName);

            var configFilePath = Path.Combine(this.OutputPath, "Other.config");

            var testNodeValue = this.GetConfigNodeValue(configFilePath, "TestNode");

            Assert.Equal("Debug", testNodeValue);
        }
    }
}
