namespace SlowCheetah_UnitTests {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.IO;

    [TestClass]
    public class BaseTest {
        protected IList<string> FilesToDeleteAfterTest { get; set; }

        [TestInitialize]
        public virtual void SetupFilesToDeleteList() {
            this.FilesToDeleteAfterTest = new List<string>();
        }

        [TestCleanup]
        public virtual void CleanUpFilesToDeleteList() {
            if (this.FilesToDeleteAfterTest != null && this.FilesToDeleteAfterTest.Count > 0) {
                foreach (string filename in this.FilesToDeleteAfterTest) {
                    if (File.Exists(filename)) {
                        try {
                            File.Delete(filename);
                        }
                        catch (System.IO.IOException) {
                            // some processes will hold onto the file until the AppDomain is unloaded
                        }
                    }
                }
            }

            this.FilesToDeleteAfterTest = null;
        }

        protected virtual string WriteTextToTempFile(string content) {
            if (string.IsNullOrEmpty(content)) { throw new ArgumentNullException("content"); }

            string tempFile = this.GetTempFilename(true);
            File.WriteAllText(tempFile, content);
            return tempFile;
        }

        protected virtual string GetTempFilename(bool ensureFileDoesntExist) {
            string path = Path.GetTempFileName();
            if (ensureFileDoesntExist && File.Exists(path)) {
                File.Delete(path);
            }
            this.FilesToDeleteAfterTest.Add(path);
            return path;
        }

    }
}
