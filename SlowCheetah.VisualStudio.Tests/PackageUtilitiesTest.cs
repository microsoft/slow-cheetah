// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace SlowCheetah.VisualStudio.Tests
{
    public class PackageUtilitiesTest
    {
        IEnumerable<string> testProjectConfigs = new List<string>(new string[] { "Debug", "Release", "Debug.Test", "" });

        [Fact]
        public void IsFileTransfromWithNullArguments()
        {
            Assert.False(PackageUtilities.IsFileTransfrom(null, null, testProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransfrom("", "", testProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransfrom("App.config", null, testProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransfrom("App.config", "", testProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransfrom(null, "App.Debug.config", testProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransfrom("", "App.Debug.config", testProjectConfigs));
        }

        [Fact]
        public void IsFileTransfromWithValidArguments()
        {
            Assert.True(PackageUtilities.IsFileTransfrom("App.config", "App.Debug.config", testProjectConfigs));
            Assert.True(PackageUtilities.IsFileTransfrom("App.config", "app.release.config", testProjectConfigs));
            Assert.True(PackageUtilities.IsFileTransfrom("APP.config", "App.Debug.config", testProjectConfigs));
            Assert.True(PackageUtilities.IsFileTransfrom("App.Test.config", "App.Test.Debug.config", testProjectConfigs));
        }

        [Fact]
        public void IsFileTransfromWithInvalidArguments()
        {
            Assert.False(PackageUtilities.IsFileTransfrom("App.config", "App.Test.Debug.config", testProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransfrom("App.Debug.config", "App.Debug.config", testProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransfrom("App.Debug.config", "App.Release.config", testProjectConfigs));
        }
    }
}
