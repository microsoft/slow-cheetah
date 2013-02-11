namespace SlowCheetah.VisualStudio {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using SlowCheetah.VisualStudio.Properties;
    using System.Reflection;
    using SlowCheetah.VisualStudio.Exceptions;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Microsoft.Web.XmlTransform;

    public class Transformer : ITransformer {
        public void Transform(string source, string transform, string destination) {
            if (string.IsNullOrWhiteSpace(source)) { throw new ArgumentNullException("source"); }
            if (string.IsNullOrWhiteSpace(transform)) { throw new ArgumentNullException("transform"); }
            if (string.IsNullOrWhiteSpace(destination)) { throw new ArgumentNullException("destination"); }

            if (!File.Exists(source)) {
                throw new FileNotFoundException("File to transform not found", source);
            }
            if (!File.Exists(transform)) {
                throw new FileNotFoundException("Transform file not found", transform);
            }

            using (XmlTransformableDocument document = new XmlTransformableDocument())
            using (XmlTransformation transformation = new XmlTransformation(transform)) {
                document.PreserveWhitespace = true;
                document.Load(source);

                var success = transformation.Apply(document);
                if (!success) {
                    string message = string.Format(
                        "There was an unknown error trying while trying to apply the transform. Source file='{0}',Transform='{1}', Destination='{2}'",
                        source,transform,destination);
                    throw new TransformFailedException(message);
                }

                document.Save(destination);
            }
        }

        internal string InstallRoot {
            get {
                string installRoot = Path.Combine(
                    System.Environment.ExpandEnvironmentVariables(PackageInstaller.Consts.InstallUserFolder),
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
    }
}
