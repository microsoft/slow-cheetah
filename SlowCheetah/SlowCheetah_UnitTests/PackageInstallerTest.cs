namespace SlowCheetah_UnitTests {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SlowCheetah.VisualStudio;
    using System.IO;

    [TestClass]
    public class PackageInstallerTest : BaseTest {
        
        [TestMethod]
        public void TestGetInstallerSettingsFromXml() {
            IInstallSettings settings = new PackageInstaller().ReadInstallSettings(Consts.settingsXml01);
            Assert.IsNotNull(settings);

            InstallSettings expected = new InstallSettings {
                Version =1.0
            };
            expected.FilesToInstall.Add(new InstallFile (@"Install-Manifest.xml","Install.Install-Manifest.xml",FileType.Text));
            expected.FilesToInstall.Add(new InstallFile(@"SlowCheetah.Transforms.targets", "Install.SlowCheetah.Transforms.targets", FileType.Text));
            expected.FilesToInstall.Add(new InstallFile(@"Microsoft.Web.XmlTransform.dll", "Install.Microsoft.Web.XmlTransform.dll", FileType.Binary));
            expected.FilesToInstall.Add(new InstallFile(@"SlowCheetah.Xdt.dll", "Install.SlowCheetah.Xdt.dll", FileType.Binary));

            CustomAssert.AssertAreEqual(expected, settings);
        }

        [TestMethod]
        public void TestGetInstallerSettingsFromResx() {
            IInstallSettings settings = new PackageInstaller().ReadInstallSettings();
            Assert.IsNotNull(settings);

            InstallSettings expected = new InstallSettings {
                Version = 1.5
            };
            expected.FilesToInstall.Add(new InstallFile(@"Install-Manifest.xml", "Install.Install-Manifest.xml", FileType.Text));
            expected.FilesToInstall.Add(new InstallFile(@"SlowCheetah.Transforms.targets", "Install.SlowCheetah.Transforms.targets", FileType.Text));
            expected.FilesToInstall.Add(new InstallFile(@"Microsoft.Web.XmlTransform.dll", "Install.Microsoft.Web.XmlTransform.dll", FileType.Binary));
            expected.FilesToInstall.Add(new InstallFile(@"SlowCheetah.Xdt.dll", "Install.SlowCheetah.Xdt.dll", FileType.Binary));

            CustomAssert.AssertAreEqual(expected, settings);
        }

        [TestMethod]
        public void TestReadInstallSettingsFromFile() {
            string filePath = this.WriteTextToTempFile(Consts.settingsXml01);
            IInstallSettings settings = new PackageInstaller().ReadInstallSettings(new FileInfo(filePath));

            InstallSettings expected = new InstallSettings {
                Version = 1.0
            };
            expected.FilesToInstall.Add(new InstallFile(@"Install-Manifest.xml", "Install.Install-Manifest.xml", FileType.Text));
            expected.FilesToInstall.Add(new InstallFile(@"SlowCheetah.Transforms.targets", "Install.SlowCheetah.Transforms.targets", FileType.Text));
            expected.FilesToInstall.Add(new InstallFile(@"Microsoft.Web.XmlTransform.dll", "Install.Microsoft.Web.XmlTransform.dll", FileType.Binary));
            expected.FilesToInstall.Add(new InstallFile(@"SlowCheetah.Xdt.dll", "Install.SlowCheetah.Xdt.dll", FileType.Binary));

            CustomAssert.AssertAreEqual(expected, settings);
        }

        [TestMethod]
        public void AreFilesOutOfDate_LocalGreaterThan_Current() {
            PackageInstaller pkgInstaller = new PackageInstaller();
            IInstallSettings currentSettings = pkgInstaller.ReadInstallSettings(Consts.settingsXml01);
            string filePath = this.WriteTextToTempFile(Consts.settingsXml_Version15);
            bool result = pkgInstaller.AreExistingFilesOutOfDate(currentSettings, filePath);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AreFilesOutOfDate_EqualTo_Current() {
            PackageInstaller pkgInstaller = new PackageInstaller();
            IInstallSettings currentSettings = pkgInstaller.ReadInstallSettings(Consts.settingsXml01);
            string filePath = this.WriteTextToTempFile(Consts.settingsXml01);
            bool result = pkgInstaller.AreExistingFilesOutOfDate(currentSettings, filePath);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AreFilesOutOfDate_LocalLessThan_Current() {
            PackageInstaller pkgInstaller = new PackageInstaller();
            IInstallSettings currentSettings = pkgInstaller.ReadInstallSettings(Consts.settingsXml01);
            string filePath = this.WriteTextToTempFile(Consts.settingsXml_Version05);
            bool result = pkgInstaller.AreExistingFilesOutOfDate(currentSettings, filePath);

            Assert.IsTrue(result);
        }

        [Ignore]
        [TestMethod]
        public void InstallFilesTest01() {
            // TODO: delete files which reside in the install location locally

            try {
                PackageInstaller pkgInstaller = new PackageInstaller();

                IInstallSettings settings = pkgInstaller.ReadInstallSettings(Consts.settingsXml01);
                pkgInstaller.InstallFiles(settings);
            }
            catch (Exception ex) {
                throw ex;
            }
            // TODO: Some assertions here

        }

        private static class Consts {
            public static string settingsXml01 =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<InstallManifest xmlns=""http://schemas.microsoft.com/transforms/2011/02/install-manifest"" version=""1.0"">
  
  <FilesToInstall>
    <File path=""Install-Manifest.xml"" name=""Install.Install-Manifest.xml"" type=""Text""/>
    <File path=""SlowCheetah.Transforms.targets"" name=""Install.SlowCheetah.Transforms.targets"" type=""Text""/>
    <File path=""Microsoft.Web.XmlTransform.dll"" name=""Install.Microsoft.Web.XmlTransform.dll"" type=""Binary""/>
    <File path=""SlowCheetah.Xdt.dll"" name=""Install.SlowCheetah.Xdt.dll"" type=""Binary""/>
  </FilesToInstall>

</InstallManifest>";

            public static string settingsXml_Version15 =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<InstallManifest xmlns=""http://schemas.microsoft.com/transforms/2011/02/install-manifest"" version=""1.5"">
  
  <FilesToInstall>
    <File path=""Install-Manifest.xml"" name=""Install.Install-Manifest.xml"" type=""Text""/>
    <File path=""SlowCheetah.Transforms.targets"" name=""Install.SlowCheetah.Transforms.targets"" type=""Text""/>
    <File path=""Microsoft.Web.XmlTransform.dll"" name=""Install.Microsoft.Web.XmlTransform.dll"" type=""Binary""/>
    <File path=""SlowCheetah.Xdt.dll"" name=""Install.SlowCheetah.Xdt.dll"" type=""Binary""/>
  </FilesToInstall>
  
</InstallManifest>";

            public static string settingsXml_Version05 =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<InstallManifest xmlns=""http://schemas.microsoft.com/transforms/2011/02/install-manifest"" version=""0.5"">
  
  <FilesToInstall>
    <File path=""Install-Manifest.xml"" name=""Install.Install-Manifest.xml"" type=""Text""/>
    <File path=""SlowCheetah.Transforms.targets"" name=""Install.SlowCheetah.Transforms.targets"" type=""Text""/>
    <File path=""Microsoft.Web.XmlTransform.dll"" name=""Install.Microsoft.Web.XmlTransform.dll"" type=""Binary""/>
    <File path=""SlowCheetah.Xdt.dll"" name=""Install.SlowCheetah.Xdt.dll"" type=""Binary""/>
  </FilesToInstall>
  
</InstallManifest>";
        }
    }
}
