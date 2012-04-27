namespace SlowCheetah.VisualStudio.Extensions {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
using System.Reflection;
    using System.IO;
    using SlowCheetah.VisualStudio.Exceptions;

    public static class AssemblyExtensions {
        /// <summary>
        /// Gets a resource from an assembly as a string.
        /// Don't use this for big strings!!!
        /// </summary>m>
        /// <returns></returns>
        public static string GetResourceAsString(this Assembly assembly, string resourceName) {
            if (assembly == null) { throw new ArgumentNullException("assembly"); }
            if (resourceName == null) { throw new ArgumentNullException("resourceName"); }

            using (Stream resxStream = assembly.GetManifestResourceStream(resourceName)) {
                if (resxStream == null) {
                    string message = string.Format("Resource with name [{0}] from assembly [{1}] not found", resxStream, assembly.FullName);
                    throw new ResourceNotFoundException(message);
                }

                string result = null;
                using (StreamReader streamReader = new StreamReader(resxStream)) {
                    result = streamReader.ReadToEnd();
                }

                return result;
            }
        }

        // TODO: Any issues with this method?
        public static void WriteTextResourceToFile(this Assembly assembly, string resourceName, string filePath) {
            if (assembly == null) { throw new ArgumentNullException("assembly"); }
            if (string.IsNullOrWhiteSpace(resourceName)) { throw new ArgumentNullException("resourceName"); }
            if (string.IsNullOrWhiteSpace(filePath)) { throw new ArgumentNullException("filePath"); }

            using (Stream resxStream = assembly.GetManifestResourceStream(resourceName)) {
                if (resxStream == null) {
                    string message = string.Format("Resource with name [{0}] from assembly [{1}] not found", resxStream, assembly.FullName);
                    throw new ResourceNotFoundException(message);
                }

                using (StreamReader reader = new StreamReader(resxStream))
                using (StreamWriter writer = new StreamWriter(new FileStream(filePath, FileMode.Create))) {
                    int readValue = 0;
                    while ((readValue = reader.Read()) != -1) {
                        writer.Write((char)readValue);
                    }
                }
            }
        }

        // TODO: Any issues with this method?
        public static void WriteBinaryResourceToFile(this Assembly assembly, string resourceName, string filePath) {
            if (assembly == null) { throw new ArgumentNullException("assembly"); }
            if (string.IsNullOrWhiteSpace(resourceName)) { throw new ArgumentNullException("resourceName"); }
            if (string.IsNullOrWhiteSpace(filePath)) { throw new ArgumentNullException("filePath"); }

            using (Stream resxStream = assembly.GetManifestResourceStream(resourceName)) {
                if (resxStream == null) {
                    string message = string.Format("Resource with name [{0}] from assembly [{1}] not found", resxStream, assembly.FullName);
                    throw new ResourceNotFoundException(message);
                }

                using (BinaryReader reader = new BinaryReader(resxStream))
                using (BinaryWriter writer = new BinaryWriter(new FileStream(filePath, FileMode.Create))) {
                    reader.BaseStream.Position = 0;
                    for (int i = 0; i < reader.BaseStream.Length;i++ ) {
                        writer.Write(reader.ReadByte());
                    }
                }
            }
        }
    }
}
