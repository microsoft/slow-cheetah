namespace SlowCheetah.VisualStudio {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using System.IO;
    using SlowCheetah.VisualStudio.Properties;
    using SlowCheetah.VisualStudio.Extensions;
    using System.Reflection;
    using SlowCheetah.VisualStudio.Exceptions;

    /// <summary>
    /// This will install files which the package needs to run.
    /// It will install them under %LocalAppData%\public
    /// </summary>
    internal sealed class PackageInstaller : IPackageInstaller {
        public void Install() {
            // read the settings file
            IInstallSettings currentSettings = this.ReadInstallSettings();
            // determine if files on disk are out of date (or missing)
            if (this.AreExistingFilesOutOfDate(currentSettings)) {
                // Update the files which reside localy
                this.InstallFiles(currentSettings);
            }
        }

        internal bool AreExistingFilesOutOfDate(IInstallSettings installSettings) {
            if (installSettings == null) { throw new ArgumentNullException("installSettings"); }

            string localPath = Path.Combine(this.InstallRoot, Consts.InstallManifestFileName);

            return this.AreExistingFilesOutOfDate(installSettings, localPath);
        }

        internal bool AreExistingFilesOutOfDate(IInstallSettings currentInstallSettings, string localFilePath) {
            if (currentInstallSettings == null) { throw new ArgumentNullException("currentInstallSettings"); }
            if (string.IsNullOrWhiteSpace(localFilePath)) { throw new ArgumentNullException("localFilePath"); }

            bool isOutOfDate = true;

            FileInfo localInstallFile = new FileInfo(localFilePath);
            if (localInstallFile.Exists) {
                // See if the version is up to date 
                IInstallSettings localSettings = this.ReadInstallSettings(localInstallFile);

                if (localSettings.Version >= currentInstallSettings.Version) {
                    isOutOfDate = false;
                }
            }

            return isOutOfDate;
        }

        /// <summary>
        /// This will read the settings from the embedded resoruces and retun the settings
        /// </summary>
        internal IInstallSettings ReadInstallSettings() {
            string installManifestResxName = this.GetFullResourceNamefor(Consts.InstallManifestResxName);
            string installXml = this.GetType().Assembly.GetResourceAsString(installManifestResxName);
            return this.ReadInstallSettings(installXml);
        }

        internal IInstallSettings ReadInstallSettings(string xmlContents) {
            try {
                XNamespace xmlNamespace = XmlConsts.XmlNamespace;
                XDocument doc = XDocument.Parse(xmlContents);
                XElement rootElement = doc.Element(xmlNamespace + XmlConsts.InstallManifest);

                var r = from e in rootElement.Elements(xmlNamespace + XmlConsts.FilesToInstall)
                        from f in e.Elements(xmlNamespace + XmlConsts.File)
                        select new InstallFile(
                            f.Attribute(XmlConsts.Path).Value,
                            f.Attribute(XmlConsts.Name).Value,
                            (FileType)Enum.Parse(typeof(FileType), f.Attribute(XmlConsts.Type).Value));

                IInstallSettings settings = new InstallSettings {
                    Version = Convert.ToDouble(rootElement.Attribute(XmlConsts.Version).Value, System.Globalization.CultureInfo.InvariantCulture)
                };
                r.ToList().ForEach(file => {
                    settings.FilesToInstall.Add(file);
                });

                return settings;
            }
            catch (FormatException fe) {
                throw new SlowCheetahCustomException("Format exception trying to parse Install Settings", fe,xmlContents);
            }
        }

        internal IInstallSettings ReadInstallSettings(FileInfo filePath) {
            if (filePath == null) { throw new ArgumentNullException("filePath"); }
            if (!filePath.Exists) {
                throw new FileNotFoundException("Install settings file not found", filePath.FullName);
            }

            // TODO: Any issues w/ this? The file should be really small so non mem issues here
            string xmlStr = File.ReadAllText(filePath.FullName);

            return this.ReadInstallSettings(xmlStr);
        }

        /// <summary>
        /// This method is responsible for dropping the files locally
        /// </summary>
        internal void InstallFiles(IInstallSettings settings) {
            Assembly assembly = this.GetType().Assembly;
            Type resxType = typeof(Resources);

            string installManifestResxName = this.GetFullResourceNamefor(Consts.InstallManifestResxName);
            string installXml = assembly.GetResourceAsString(installManifestResxName);

            settings.FilesToInstall.ToList().ForEach(file => {
                // write the file to disk from the resx
                string fullResxName = this.GetFullResourceNamefor(file.ResxName);
                string fullFilePath = Path.Combine(this.InstallRoot, file.Path);

                FileInfo fileInfo = new FileInfo(fullFilePath);
                if (!fileInfo.Directory.Exists) {
                    fileInfo.Directory.Create();
                }

                switch (file.FileType) {
                    case FileType.Text:
                        assembly.WriteTextResourceToFile(fullResxName, fullFilePath);
                        break;

                    case FileType.Binary:
                        assembly.WriteBinaryResourceToFile(fullResxName, fullFilePath);
                        break;

                    default:
                        string message = string.Format("Invalid value [{0}] for FileType enum", file.FileType);
                        throw new UnknownValueException("");
                }

            });
        }

        internal string InstallRoot {
            get {
                string installRoot = Path.Combine(System.Environment.ExpandEnvironmentVariables(Consts.InstallUserFolder),
                    Settings.Default.InstallPath);

                // on XP/Server 2003 we cannont rely on the LocalAppData folder must user UserProfile instead
                if (Environment.GetEnvironmentVariable("LocalAppData") == null) {
                    // TODO: Any localization issues here
                    // C:\Documents and Settings\sayedha\Local Settings\Application Data
                    string newPath = @"%UserProfile%\Local Settings\Application Data\";
                    installRoot = Path.Combine(System.Environment.ExpandEnvironmentVariables(newPath), Settings.Default.InstallPath);
                }

                return installRoot;
            }
        }

        internal string GetResourceAsString(Assembly assembly, string resourceName) {
            if (assembly == null) { throw new ArgumentNullException("assembly"); }
            if (string.IsNullOrWhiteSpace(resourceName)) { throw new ArgumentNullException("resourceName"); }

            throw new NotImplementedException();
        }

        private string GetFullResourceNamefor(string resxName) {
            if (string.IsNullOrWhiteSpace(resxName)) { throw new ArgumentNullException("resxName"); }

            string result = string.Format(
                "{0}.{1}",
                typeof(Resources).FullName,
                resxName);
            return result;
        }

        internal static class Consts {
            public static string InstallUserFolder = Settings.Default.InstallUserFolder;
            public static string InstallManifestFileName = @"Install-Manifest.xml";
            public static string InstallManifestResxName = @"Install.Install-Manifest.xml";
        }
        internal static class XmlConsts {
            public static string XmlNamespace = @"http://schemas.microsoft.com/transforms/2011/02/install-manifest";
            public const string InstallManifest = @"InstallManifest";
            public static string Version = @"version";
            public static string FilesToInstall = @"FilesToInstall";
            public static string Path = @"path";
            public static string Name = @"name";
            public static string File = @"File";
            public static string Type = @"type";
        }
    }

    internal interface IInstallSettings {
        double Version { get; }
        IList<IInstallFile> FilesToInstall { get; }
    }

    internal class InstallSettings : IInstallSettings {
        public InstallSettings() {
            this.FilesToInstall = new List<IInstallFile>();
        }

        public double Version { get; set; }

        public IList<IInstallFile> FilesToInstall { get; private set; }
    }

    internal interface IInstallFile {
        string Path { get; }
        string ResxName { get; }
        FileType FileType { get; }
    }

    internal class InstallFile : IInstallFile {
        public InstallFile(string path, string resxName, FileType fileType) {
            this.Path = path;
            this.ResxName = resxName;
            this.FileType = fileType;
        }

        public string Path { get; private set; }
        public string ResxName { get; private set; }
        public FileType FileType { get; private set; }
    }

    internal enum FileType {
        Text,
        Binary
    }
}
