// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Xunit;

namespace SlowCheetah.Tests.BuildTests
{
    public abstract class ConfigTransformTestsBase : IDisposable
    {
        public string SolutionDir { get { return Path.Combine(Environment.CurrentDirectory, @"..\.."); } }
        public string OutputPath { get { return Path.Combine(Environment.CurrentDirectory, @"ProjectOutput"); } }
        public string TestProjectsDir { get { return Path.Combine(SolutionDir, @"SlowCheetah.Tests\BuildTests\TestProjects"); } }

        public void BuildProject(string projectName)
        {
            var globalProperties = new Dictionary<string, string>(){
               {"Configuration", "Debug"},
               {"OutputPath", OutputPath},
               {"VSToolsPath", ""}
            };
            var project = new Project(Path.Combine(TestProjectsDir, projectName, projectName + ".csproj"),
                globalProperties, "4.0");
            var logger = new ConsoleLogger(LoggerVerbosity.Quiet);
            bool buildSuccess = project.Build(logger);
            Assert.True(buildSuccess);
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

        public void Dispose()
        {
            Directory.Delete(OutputPath, true);
        }
    }
}
