namespace SlowCheetah_UnitTests {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using SlowCheetah.VisualStudio;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class CustomAssert {
        internal static void AssertAreEqual(IInstallSettings expected, IInstallSettings actual) {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            Assert.IsNotNull(expected.FilesToInstall);
            Assert.AreEqual(expected.FilesToInstall.Count, actual.FilesToInstall.Count);

            for (int i = 0; i < expected.FilesToInstall.Count; i++) {
                IInstallFile expectedFile = expected.FilesToInstall[i];
                IInstallFile actualFile = actual.FilesToInstall[i];
                AssertAreEqual(expectedFile, actualFile);
            }
        }

        internal static void AssertAreEqual(IInstallFile expected, IInstallFile actual) {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            Assert.IsNotNull(expected.Path);
            Assert.AreEqual(expected.Path, actual.Path);
            Assert.AreEqual(expected.FileType, actual.FileType);
        }
    }
}
