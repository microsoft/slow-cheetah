// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System.IO;
using Xunit;

namespace SlowCheetah.Tests.BuildTests
{
    [Collection("BuildTests")]
    public class WebAppTests : ConfigTransformTestsBase
    {

        [Fact]
        public void WebApp_OtherConfig_IsTransformed()
        {
            var projectName = "WebApplication";
            BuildProject(projectName);

            var configFilePath = Path.Combine(OutputPath, "Other.config");

            var testNodeValue = GetConfigNodeValue(configFilePath, "TestNode");

            Assert.Equal("Debug", testNodeValue);
        }
    }
}
