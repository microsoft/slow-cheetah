// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.Tests
{
    using System.IO;
    using Xunit;

    /// <summary>
    /// Tests for <see cref="ITransformer"/>.
    /// </summary>
    public class TransformTest : BaseTest
    {
        /// <summary>
        /// Tests for <see cref="XmlTransformer"/>.
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
