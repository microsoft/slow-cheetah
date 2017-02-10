// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace SlowCheetah.VisualStudio.Tests
{
    public class PackageUtilitiesTest
    {
        IEnumerable<string> baseTestProjectConfigs = new List<string>(new string[] { "Debug", "Release"});

        [Fact]
        public void IsFileTransfromWithNullArguments()
        {
            Assert.False(PackageUtilities.IsFileTransform(null, null, baseTestProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransform("", "", baseTestProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransform("App.config", null, baseTestProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransform("App.config", "", baseTestProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransform(null, "App.Debug.config", baseTestProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransform("", "App.Debug.config", baseTestProjectConfigs));
        }

        [Fact]
        public void IsFileTransfromWithValidArguments()
        {
            Assert.True(PackageUtilities.IsFileTransform("App.config", "App.Debug.config", baseTestProjectConfigs));
            Assert.True(PackageUtilities.IsFileTransform("App.config", "app.release.config", baseTestProjectConfigs));
            Assert.True(PackageUtilities.IsFileTransform("APP.config", "App.Debug.config", baseTestProjectConfigs));
            Assert.True(PackageUtilities.IsFileTransform("App.Test.config", "App.Test.Debug.config", baseTestProjectConfigs));
        }

        [Fact]
        public void IsFileTransfromWithInvalidArguments()
        {
            Assert.False(PackageUtilities.IsFileTransform("App.config", "App.Test.Debug.config", baseTestProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransform("App.Debug.config", "App.Debug.config", baseTestProjectConfigs));
            Assert.False(PackageUtilities.IsFileTransform("App.Debug.config", "App.Release.config", baseTestProjectConfigs));
        }

        [Fact]
        public void IsFileTransformWithDottedConfigs()
        {
            IEnumerable<string> testProjectConfigsWithDots = new List<string>(new string[] { "Debug", "Debug.Test", "Release", "Test.Release", "Test.Rel"});

            Assert.True(PackageUtilities.IsFileTransform("App.config", "App.Debug.Test.config", testProjectConfigsWithDots));
            Assert.True(PackageUtilities.IsFileTransform("App.System.config", "App.System.Debug.Test.config", testProjectConfigsWithDots));
            Assert.True(PackageUtilities.IsFileTransform("App.config", "App.Test.Release.config", testProjectConfigsWithDots));
            Assert.True(PackageUtilities.IsFileTransform("App.Test.config", "App.Test.Release.config", testProjectConfigsWithDots));
            Assert.True(PackageUtilities.IsFileTransform("App.Test.config", "App.Test.Test.Release.config", testProjectConfigsWithDots));
            Assert.True(PackageUtilities.IsFileTransform("App.config", "App.Test.Rel.config", testProjectConfigsWithDots));

            Assert.False(PackageUtilities.IsFileTransform("App.config", "App.Release.Test.config", testProjectConfigsWithDots));
            Assert.False(PackageUtilities.IsFileTransform("App.config", "App.Rel.Test.config", testProjectConfigsWithDots));
            Assert.False(PackageUtilities.IsFileTransform("App.Test.config", "App.Test.Rel.config", testProjectConfigsWithDots));
            Assert.False(PackageUtilities.IsFileTransform("App.Test.config", "App.Test.Test.config", testProjectConfigsWithDots));
            Assert.False(PackageUtilities.IsFileTransform("App.Test.config", "App.Debug.Test.config", testProjectConfigsWithDots));
            Assert.False(PackageUtilities.IsFileTransform("App.config", "Test.Rel.config", testProjectConfigsWithDots));
            Assert.False(PackageUtilities.IsFileTransform("App.Test.Rel.config", "App.Test.Rel.config", testProjectConfigsWithDots));
        }
    }
}
