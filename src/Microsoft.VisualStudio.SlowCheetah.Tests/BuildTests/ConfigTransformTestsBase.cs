// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.Tests.BuildTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Logging;
    using Xunit;

    /// <summary>
    /// Base class for transformation tests.
    /// </summary>
    public abstract class ConfigTransformTestsBase : IDisposable
    {
        /// <summary>
        /// Gets the test solution directory
        /// </summary>
        public string SolutionDir
        {
            get { return Path.Combine(Environment.CurrentDirectory, @"..\..\..\src"); }
        }

        /// <summary>
        /// Gets the output path of the test project
        /// </summary>
        public string OutputPath
        {
            get { return Path.Combine(Environment.CurrentDirectory, @"ProjectOutput"); }
        }

        /// <summary>
        /// Gets the test projects directory
        /// </summary>
        public string TestProjectsDir
        {
            get { return Path.Combine(this.SolutionDir, @"Microsoft.VisualStudio.SlowCheetah.Tests\BuildTests\TestProjects"); }
        }

        /// <summary>
        /// Builds the project of the given name from the <see cref="TestProjectsDir"/>
        /// </summary>
        /// <param name="projectName">Name of the project to be built.
        /// Must correspond to a folder name in the test projects directory</param>
        public void BuildProject(string projectName)
        {
            var globalProperties = new Dictionary<string, string>()
            {
               { "Configuration", "Debug" },
               { "OutputPath", this.OutputPath },
               { "VSToolsPath", string.Empty }
            };

            var project = new Project(Path.Combine(this.TestProjectsDir, projectName, projectName + ".csproj"), globalProperties, "15.0");
            bool buildSuccess = project.Build();

            Assert.True(buildSuccess);
        }

        /// <summary>
        /// Gets a app setting from a configuration file
        /// </summary>
        /// <param name="configFilePath">Path to the configuration file</param>
        /// <param name="appSettingKey">Setting key</param>
        /// <returns>Value of the setting</returns>
        public string GetAppSettingValue(string configFilePath, string appSettingKey)
        {
            var configFile = XDocument.Load(configFilePath);
            var testSetting = (from settingEl in configFile.Descendants("appSettings").Elements()
                               where settingEl.Attribute("key").Value == appSettingKey
                               select settingEl.Attribute("value").Value).Single();
            return testSetting;
        }

        /// <summary>
        /// Gets the value of a node within a configuration file
        /// </summary>
        /// <param name="configFilePath">Path to the configuration file</param>
        /// <param name="nodeName">Name of the node</param>
        /// <returns>Value of the node</returns>
        public string GetConfigNodeValue(string configFilePath, string nodeName)
        {
            var configFile = XDocument.Load(configFilePath);
            return configFile.Descendants(nodeName).Single().Value;
        }

        /// <summary>
        /// At the end of tests, delete the output path for the tested projects
        /// </summary>
        public void Dispose()
        {
            ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
            if (Directory.Exists(this.OutputPath))
            {
                Directory.Delete(this.OutputPath, true);
            }
        }
    }
}
