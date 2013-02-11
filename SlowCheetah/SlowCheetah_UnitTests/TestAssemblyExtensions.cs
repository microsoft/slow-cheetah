namespace SlowCheetah_UnitTests {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SlowCheetah.VisualStudio.Extensions;
    using System.IO;

    [TestClass]
    public class TestAssemblyExtensions : BaseTest {

        [TestMethod]
        public void WriteTextResourceToFileTest01() {
            string tempFilePath = this.GetTempFilename(true);

            this.GetType().Assembly.WriteTextResourceToFile(Consts.ResxInstalManifestName, tempFilePath);
            Assert.IsTrue(File.Exists(tempFilePath));
        }

        [TestMethod]
        public void WriteBinaryResouceToFileTest01() {
            string tempFilePath = this.GetTempFilename(true);

            this.GetType().Assembly.WriteBinaryResourceToFile(Consts.ResxWebPubDllName, tempFilePath);
            Assert.IsTrue(File.Exists(tempFilePath));
        }

        private static class Consts {
            public static string ResxInstalManifestName = @"SlowCheetah_UnitTests.Resources.Install.Install-Manifest.xml";
            public static string ResxWebPubDllName = @"SlowCheetah_UnitTests.Resources.Install.SlowCheetah.Xdt.dll";
            public static string ResxTransformsTargetsName = @"SlowCheetah_UnitTests.Resources.Install.SlowCheetah.Transforms.targets";

        }
    }
}
