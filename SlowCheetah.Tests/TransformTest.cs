// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah.Tests
{
    using System.IO;
    using Xunit;

    /// <summary>
    /// Tests for <see cref="ITransformer"/>
    /// </summary>
    public class TransformTest : BaseTest
    {
        /// <summary>
        /// Tests for <see cref="XmlTransformer"/>
        /// </summary>
        [Fact]
        public void TestXmlTransform()
        {
            string sourceFile = this.WriteTextToTempFile(TestUtilities.Source01);
            string transformFile = this.WriteTextToTempFile(TestUtilities.Transform01);
            string expectedResultFile = this.WriteTextToTempFile(TestUtilities.Result01);

            string destFile = this.GetTempFilename(true);
            ITransformer transformer = new XmlTransformer();
            transformer.Transform(sourceFile, transformFile, destFile);

            Assert.True(File.Exists(sourceFile));
            Assert.True(File.Exists(transformFile));
            Assert.True(File.Exists(destFile));

            string actualResult = File.ReadAllText(destFile);
            string expectedResult = File.ReadAllText(expectedResultFile);
            Assert.Equal(expectedResult.Trim(), actualResult.Trim());
        }
    }
}
