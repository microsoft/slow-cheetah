using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SlowCheetah.UnitTests.BuildTests
{
    public abstract class ConfigTransformTestsBase
    {
        public TestContext TestContext { get; set; }
        public string SolutionDir { get { return Path.Combine(TestContext.TestRunDirectory, @"..\.."); } }
        public string OutputPath { get { return Path.Combine(TestContext.TestRunDirectory, @"ProjectOutput"); } }
        public string TestProjectsDir { get { return Path.Combine(SolutionDir, @"SlowCheetah.UnitTests\BuildTests\TestProjects"); } }

        public void BuildProject(string projectName)
        {
            var globalProperties = new Dictionary<string, string>(){
               {"Configuration", "Debug"},
               {"SolutionDir", SolutionDir},
               {"OutputPath", OutputPath},
               {"VSToolsPath", ""}
            };
            var project = new Project(Path.Combine(TestProjectsDir, projectName, projectName + ".csproj"),
                globalProperties, "4.0");
            var logger = new ConsoleLogger(LoggerVerbosity.Diagnostic);
            bool buildSuccess = project.Build(logger);
            Assert.IsTrue(buildSuccess);
            ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
        }

        public string GetAppSettingValue(string configFilePath, string appSettingKey)
        {
            var configFile = XDocument.Load(configFilePath);
            var testSetting = (from settingEl in configFile.Descendants("appSettings").Elements()
                               where settingEl.Attribute("key").Value == appSettingKey
                               select settingEl.Attribute("value").Value).Single();
            return testSetting;
        }

        public string GetConfigNodeValue(string configFilePath, string nodeName)
        {
            var configFile = XDocument.Load(configFilePath);
            return configFile.Descendants(nodeName).Single().Value;
        }
    }
}
