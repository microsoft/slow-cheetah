// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System.IO;
using Xunit;

namespace SlowCheetah.Tests
{
    public class TransformTest : BaseTest
    {
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
