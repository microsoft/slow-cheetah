using System;
using System.IO;
using SlowCheetah;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SlowCheetah.UnitTests
{
    [TestClass]
    public class TransformTest : BaseTest
    {
        [TestMethod]
        public void TestXmlTransform()
        {
            string sourceFile = this.WriteTextToTempFile(TestUtilities.Source01);
            string transformFile = this.WriteTextToTempFile(TestUtilities.Transform01);
            string expectedResultFile = this.WriteTextToTempFile(TestUtilities.Result01);

            string destFile = this.GetTempFilename(true);
            ITransformer transformer = new XmlTransformer();
            transformer.Transform(sourceFile, transformFile, destFile);

            Assert.IsTrue(File.Exists(sourceFile));
            Assert.IsTrue(File.Exists(transformFile));
            Assert.IsTrue(File.Exists(destFile));

            string actualResult = File.ReadAllText(destFile);
            string expectedResult = File.ReadAllText(expectedResultFile);
            Assert.AreEqual(expectedResult.Trim(), actualResult.Trim());
        }
    }
}
