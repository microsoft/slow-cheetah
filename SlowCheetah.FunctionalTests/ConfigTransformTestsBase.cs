using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SlowCheetah.IntegrationTests
{
    public abstract class ConfigTransformTestsBase
    {
        public TestContext TestContext { get; set; }
        public string SolutionDir { get { return Path.Combine(TestContext.TestRunDirectory, @"..\.."); } }
        public string OutputPath { get { return Path.Combine(TestContext.TestRunDirectory, @"ProjectOutput"); } }
        public string TestProjectsDir { get { return Path.Combine(SolutionDir, @"SlowCheetah.FunctionalTests\TestProjects"); } }




        public void BuildProject(string projectName)
        {
            var globalProperties = new Dictionary<string, string>(){
               {"Configuration", "Debug"},
               {"SolutionDir", SolutionDir},
               {"OutputPath", OutputPath},
               //{"VSToolsPath", @"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v12.0"}
               //{"VSToolsPath", @"c:\program files (x86)MSBuild\Microsoft\Microsoft\VisualStudio\v12.0"}
            };
            var project = new Project(Path.Combine(TestProjectsDir, projectName, projectName + ".csproj"),
                globalProperties, "12.0");
            var logger = new ConsoleLogger(LoggerVerbosity.Diagnostic);

            Assert.IsTrue(project.Build(logger));
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
