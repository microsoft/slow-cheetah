// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System;
using Xunit;

namespace SlowCheetah.VisualStudio.Tests
{
    public class PackageUtilitiesTest
    {
        [Fact]
        public void IsFileTransfromWithNullArguments()
        {
            Assert.False(PackageUtilities.IsFileTransfrom(null, null));
            Assert.False(PackageUtilities.IsFileTransfrom("", ""));
            Assert.False(PackageUtilities.IsFileTransfrom("App.config", null));
            Assert.False(PackageUtilities.IsFileTransfrom("App.config", ""));
            Assert.False(PackageUtilities.IsFileTransfrom(null, "App.Debug.config"));
            Assert.False(PackageUtilities.IsFileTransfrom("", "App.Debug.config"));
        }

        [Fact]
        public void IsFileTransfromWithValidArguments()
        {
            Assert.True(PackageUtilities.IsFileTransfrom("App.config", "App.Debug.config"));
            Assert.True(PackageUtilities.IsFileTransfrom("App.config", "app.release.config"));
            Assert.True(PackageUtilities.IsFileTransfrom("APP.config", "App.Debug.config"));
            Assert.True(PackageUtilities.IsFileTransfrom("App.Test.config", "App.Test.Debug.config"));
        }

        [Fact]
        public void IsFileTransfromWithInvalidArguments()
        {
            Assert.False(PackageUtilities.IsFileTransfrom("App.config", "App.Test.Debug.config"));
            Assert.False(PackageUtilities.IsFileTransfrom("App.Debug.config", "App.Debug.config"));
            Assert.False(PackageUtilities.IsFileTransfrom("App.Debug.config", "App.Release.config"));
        }
    }
}
