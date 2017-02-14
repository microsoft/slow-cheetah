// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace SlowCheetah.VisualStudio.Tests
{
    /// <summary>
    /// Test class for <see cref="PackageUtilities"/>
    /// </summary>
    public class PackageUtilitiesTest
    {
        IEnumerable<string> baseTestProjectConfigs = new List<string>(new string[] { "Debug", "Release" });
        IEnumerable<string> testProjectConfigsWithDots = new List<string>(new string[] { "Debug", "Debug.Test", "Release", "Test.Release", "Test.Rel" });


        /// <summary>
        /// Tests <see cref="PackageUtilities.IsFileTransform(string, string, IEnumerable{string})"/> returns on arguments that are null or empty strings
        /// </summary>
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("App.config", null)]
        [InlineData("App.config", "")]
        [InlineData(null, "App.Debug.config")]
        [InlineData("", "App.Debug.config")]
        public void IsFileTransfromWithNullArguments(string docName, string trnName)
        {
            Assert.False(PackageUtilities.IsFileTransform(docName, trnName, baseTestProjectConfigs));
        }

        /// <summary>
        /// Tests <see cref="PackageUtilities.IsFileTransform(string, string, IEnumerable{string})"/> with valid arguments normally found in projects.
        /// </summary>
        [Theory]
        [InlineData("App.config", "App.Debug.config")]
        [InlineData("App.config", "app.release.config")]
        [InlineData("APP.config", "App.Debug.config")]
        [InlineData("App.Test.config", "App.Test.Debug.config")]
        public void IsFileTransfromWithValidArguments(string docName, string trnName)
        {
            Assert.True(PackageUtilities.IsFileTransform(docName, trnName, baseTestProjectConfigs));
        }

        /// <summary>
        /// Tests <see cref="PackageUtilities.IsFileTransform(string, string, IEnumerable{string})"/> with invalid arguments
        /// </summary>
        [Theory]
        [InlineData("App.config", "App.Test.Debug.config")]
        [InlineData("App.Debug.config", "App.Debug.config")]
        [InlineData("App.Debug.config", "App.Release.config")]
        public void IsFileTransfromWithInvalidArguments(string docName, string trnName)
        {
            Assert.False(PackageUtilities.IsFileTransform(docName, trnName, baseTestProjectConfigs));
        }

        /// <summary>
        /// Tests <see cref="PackageUtilities.IsFileTransform(string, string, IEnumerable{string})"/> with project configurations containing dots 
        /// and file names with similar structures. Tests valid names
        /// </summary>
        [Theory]
        [InlineData("App.config", "App.Debug.Test.config")]
        [InlineData("App.System.config", "App.System.Debug.Test.config")]
        [InlineData("App.config", "App.Test.Release.config")]
        [InlineData("App.Test.config", "App.Test.Release.config")]
        [InlineData("App.Test.config", "App.Test.Test.Release.config")]
        [InlineData("App.config", "App.Test.Rel.config")]
        public void IsFileTransformWithDottedConfigsAndValidNames(string docName, string trnName)
        {
            Assert.True(PackageUtilities.IsFileTransform(docName, trnName, testProjectConfigsWithDots));
        }

        /// <summary>
        /// Tests <see cref="PackageUtilities.IsFileTransform(string, string, IEnumerable{string})"/> with project configurations containing dots 
        /// and file names with similar structures. Tests invalid names
        /// </summary>
        [Theory]
        [InlineData("App.config", "App.Release.Test.config")]
        [InlineData("App.config", "App.Rel.Test.config")]
        [InlineData("App.Test.config", "App.Test.Rel.config")]
        [InlineData("App.Test.config", "App.Test.Test.config")]
        [InlineData("App.Test.config", "App.Debug.Test.config")]
        [InlineData("App.config", "Test.Rel.config")]
        [InlineData("App.Test.Rel.config", "App.Test.Rel.config")]
        public void IsFileTransformWithDottedConfigsAndInvalidNames(string docName, string trnName)
        {
            Assert.False(PackageUtilities.IsFileTransform(docName, trnName, testProjectConfigsWithDots));
        }
    }
}
