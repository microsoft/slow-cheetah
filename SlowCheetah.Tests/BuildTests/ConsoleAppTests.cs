// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System.IO;
using Xunit;

namespace SlowCheetah.Tests.BuildTests
{
    [Collection("BuildTests")]
    public class ConsoleAppTests : ConfigTransformTestsBase
    {
        [Fact]
        public void ConsoleApp_AppConfig_IsTransformed()
        {
            var projectName = "ConsoleApp";
            BuildProject(projectName);

            var configFilePath = Path.Combine(OutputPath, "ConsoleApp.exe.config");

            var testSetting = GetAppSettingValue(configFilePath, "TestSetting");

            Assert.Equal("Debug", testSetting);
        }

        [Fact]
        public void ConsoleApp_OtherConfig_IsTransformed()
        {
            var projectName = "ConsoleApp";
            BuildProject(projectName);

            var configFilePath = Path.Combine(OutputPath, "Other.config");

            var testNodeValue = GetConfigNodeValue(configFilePath, "TestNode");

            Assert.Equal("Debug", testNodeValue);
        }
    }
}
