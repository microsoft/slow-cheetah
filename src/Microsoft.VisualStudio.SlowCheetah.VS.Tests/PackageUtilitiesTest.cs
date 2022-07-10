// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS.Tests
{
    using System.Collections.Generic;
    using Xunit;

    /// <summary>
    /// Test class for <see cref="PackageUtilities"/>
    /// </summary>
    public class PackageUtilitiesTest
    {
        private IEnumerable<string> baseTestProjectConfigs = new List<string>(new string[] { "Debug", "Release" });
        private IEnumerable<string> testProjectConfigsWithDots = new List<string>(new string[] { "Debug", "Debug.Test", "Release", "Test.Release", "Test.Rel" });

        /// <summary>
        /// Tests <see cref="PackageUtilities.IsFileTransformForBuildConfiguration(string, string, IEnumerable{string})"/> returns on arguments that are null or empty strings
        /// </summary>
        /// <param name="docName">Document name</param>
        /// <param name="trnName">Transform file name</param>
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("App.config", null)]
        [InlineData("App.config", "")]
        [InlineData(null, "App.Debug.config")]
        [InlineData("", "App.Debug.config")]
        public void IsFileTransformWithNullArguments(string docName, string trnName)
        {
            Assert.False(PackageUtilities.IsFileTransformForBuildConfiguration(docName, trnName, this.baseTestProjectConfigs));
        }

        /// <summary>
        /// Tests <see cref="PackageUtilities.IsFileTransformForBuildConfiguration(string, string, IEnumerable{string})"/> with valid arguments normally found in projects.
        /// </summary>
        /// <param name="docName">Document name</param>
        /// <param name="trnName">Transform file name</param>
        [Theory]
        [InlineData("App.config", "App.Debug.config")]
        [InlineData("App.config", "app.release.config")]
        [InlineData("APP.config", "App.Debug.config")]
        [InlineData("App.Test.config", "App.Test.Debug.config")]
        public void IsFileTransformWithValidArguments(string docName, string trnName)
        {
            Assert.True(PackageUtilities.IsFileTransformForBuildConfiguration(docName, trnName, this.baseTestProjectConfigs));
        }

        /// <summary>
        /// Tests <see cref="PackageUtilities.IsFileTransformForBuildConfiguration(string, string, IEnumerable{string})"/> with invalid arguments
        /// </summary>
        /// <param name="docName">Document name</param>
        /// <param name="trnName">Transform file name</param>
        [Theory]
        [InlineData("App.config", "App.Test.Debug.config")]
        [InlineData("App.Debug.config", "App.Debug.config")]
        [InlineData("App.Debug.config", "App.Release.config")]
        public void IsFileTransformWithInvalidArguments(string docName, string trnName)
        {
            Assert.False(PackageUtilities.IsFileTransformForBuildConfiguration(docName, trnName, this.baseTestProjectConfigs));
        }

        /// <summary>
        /// Tests <see cref="PackageUtilities.IsFileTransformForBuildConfiguration(string, string, IEnumerable{string})"/> with project configurations containing dots
        /// and file names with similar structures. Tests valid names
        /// </summary>
        /// <param name="docName">Document name</param>
        /// <param name="trnName">Transform file name</param>
        [Theory]
        [InlineData("App.config", "App.Debug.Test.config")]
        [InlineData("App.System.config", "App.System.Debug.Test.config")]
        [InlineData("App.config", "App.Test.Release.config")]
        [InlineData("App.Test.config", "App.Test.Release.config")]
        [InlineData("App.Test.config", "App.Test.Test.Release.config")]
        [InlineData("App.config", "App.Test.Rel.config")]
        public void IsFileTransformWithDottedConfigsAndValidNames(string docName, string trnName)
        {
            Assert.True(PackageUtilities.IsFileTransformForBuildConfiguration(docName, trnName, this.testProjectConfigsWithDots));
        }

        /// <summary>
        /// Tests <see cref="PackageUtilities.IsFileTransformForBuildConfiguration(string, string, IEnumerable{string})"/> with project configurations containing dots
        /// and file names with similar structures. Tests invalid names
        /// </summary>
        /// <param name="docName">Document name</param>
        /// <param name="trnName">Transform file name</param>
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
            Assert.False(PackageUtilities.IsFileTransformForBuildConfiguration(docName, trnName, this.testProjectConfigsWithDots));
        }

        /// <summary>
        /// Tests <see cref="PackageUtilities.IsGenericFileTransform(string, string)"/> with invalid arguments
        /// </summary>
        /// <param name="docName">Document name</param>
        /// <param name="trnName">Transform file name</param>
        [Theory]
        [InlineData("App.config", "App.config")]
        [InlineData("App.Debug.config", "App.Debug.config")]
        [InlineData("App.config", "App..config")]
        [InlineData("App.Debug.config", "App.config")]
        [InlineData("App.config", "App.config.Debug")]
        public void IsFileGenericTransformWithInvalidArguments(string docName, string trnName)
        {
            Assert.False(PackageUtilities.IsGenericFileTransform(docName, trnName));
        }

        /// <summary>
        /// Tests <see cref="PackageUtilities.IsGenericFileTransform(string, string)"/> with valid arguments
        /// </summary>
        /// <param name="docName">Document name</param>
        /// <param name="trnName">Transform file name</param>
        [Theory]
        [InlineData("App.config", "App.Debug.config")]
        [InlineData("App.config", "App.Test.Debug.config")]
        [InlineData("App.Test.config", "App.Test.Debug.config")]
        public void IsFileGenericTransformWithValidArguments(string docName, string trnName)
        {
            Assert.True(PackageUtilities.IsGenericFileTransform(docName, trnName));
        }
    }
}
