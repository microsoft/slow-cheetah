namespace SlowCheetah.VisualStudio {
    using System;
    using Microsoft.Build.Framework;

    public class MockBuildEngine : IBuildEngine {
        #region IBuildEngine Members

        public int ColumnNumberOfTaskNode {
            get { return 0; }
        }

        public bool ContinueOnError {
            get { return true; }
        }

        public int LineNumberOfTaskNode {
            get { return 0; }
        }

        public string ProjectFileOfTaskNode {
            get { return String.Empty; }
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs) {
            return true;
        }

        public void LogCustomEvent(CustomBuildEventArgs e) {
        }

        public void LogErrorEvent(BuildErrorEventArgs e) {
        }

        public void LogMessageEvent(BuildMessageEventArgs e) {
        }

        public void LogWarningEvent(BuildWarningEventArgs e) {
        }
        #endregion
    }
}
