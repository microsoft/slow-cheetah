namespace SlowCheetah.VisualStudio {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using SlowCheetah.VisualStudio.Properties;
    using System.Reflection;
    using SlowCheetah.VisualStudio.Exceptions;

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

            // get the path to the assmebly which has the TransformXml task inside of it
            // Settings.Default.InstallPath; => MSBuild\SlowCheetah\v1\
            // Settings.Default.TransformAssemblyPath; => SlowCheetah.Tasks.dll
            // System.Environment.ExpandEnvironmentVariables(Consts.PublicFolder)

            string assemblyPath = Path.Combine(
                this.InstallRoot,
                Settings.Default.TransformAssemblyPath);



            if (!File.Exists(assemblyPath)) {
                throw new FileNotFoundException("Transform assembly not found", assemblyPath);
            }

            // load the assembly
            Assembly assembly = Assembly.LoadFile(assemblyPath);
            // find the class
            Type type = assembly.GetType(Settings.Default.TransformXmlTaskName, true, true);

            // create a new instance of it
            dynamic transformTask = Activator.CreateInstance(type);
            // set the properties on it
            transformTask.BuildEngine = new MockBuildEngine();
            transformTask.Source = source;
            transformTask.Transform = transform;
            transformTask.Destination = destination;

            bool succeeded = transformTask.Execute();

            if (!succeeded) {
                string message = string.Format("There was an error processing the transformation.");
                throw new TransformFailedException(message);
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
