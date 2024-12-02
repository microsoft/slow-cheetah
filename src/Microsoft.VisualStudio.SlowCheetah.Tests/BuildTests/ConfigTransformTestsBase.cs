// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License license. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.Tests.BuildTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Build.Utilities;
    using Xunit;

    /// <summary>
    /// Base class for transformation tests.
    /// </summary>
    public abstract class ConfigTransformTestsBase : IDisposable
    {
        /// <summary>
        /// Gets the test solution directory.
        /// </summary>
        public string SolutionDir
        {
            get { return Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\src"); }
        }

        /// <summary>
        /// Gets the output path of the test project.
        /// </summary>
        public string OutputPath
        {
            get { return Path.Combine(Environment.CurrentDirectory, @"ProjectOutput"); }
        }

        /// <summary>
        /// Gets the test projects directory.
        /// </summary>
        public string TestProjectsDir
        {
            get { return Path.Combine(this.SolutionDir, @"Microsoft.VisualStudio.SlowCheetah.Tests\BuildTests\TestProjects"); }
        }

        /// <summary>
        /// Gets the msbuild exe path that was cached during build.
        /// </summary>
        private static string MSBuildExePath
        {
            get
            {
                string msbuildPathCache = Path.Combine(Environment.CurrentDirectory, "msbuildPath.txt");
                return Path.Combine(File.ReadAllLines(msbuildPathCache).First(), "msbuild.exe");
            }
        }

        /// <summary>
        /// Builds the project of the given name from the <see cref="TestProjectsDir"/>.
        /// </summary>
        /// <param name="projectName">Name of the project to be built.
        /// Must correspond to a folder name in the test projects directory.</param>
        public void BuildProject(string projectName)
        {
            var globalProperties = new Dictionary<string, string>()
            {
               { "Configuration", "Debug" },
               { "OutputPath", this.OutputPath },
            };

            var msbuildPath = ToolLocationHelper.GetPathToBuildToolsFile("msbuild.exe", ToolLocationHelper.CurrentToolsVersion);

            // We use an external process to run msbuild, because XUnit test discovery breaks
            // when using <Reference Include="$(MSBuildToolsPath)\Microsoft.Build.dll" />.
            // MSBuild NuGet packages proved to be difficult in getting in-proc test builds to run.
            string projectPath = Path.Combine(this.TestProjectsDir, projectName, projectName + ".csproj");
            //string msbuildPath = MSBuildExePath;
            string properties = "/p:" + string.Join(",", globalProperties.Select(x => $"{x.Key}={x.Value}"));

            var startInfo = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = msbuildPath,
                Arguments = $"{projectPath} {properties}",
                CreateNoWindow = false,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized,
            };

            string path = "C:\\src\\libtempslowcheetah\\src\\Microsoft.VisualStudio.SlowCheetah.Tests\\example.txt";

            // Open the file for reading
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Write))
            {
                // Read from the file
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.WriteLine($"Running msbuild.exe {startInfo.Arguments}");
                    writer.WriteLine($"Running msbuild.exe filename {startInfo.FileName}");
                }
            }

            try
            {

                using (var process = System.Diagnostics.Process.Start(startInfo))
                {

                    process.WaitForExit();
                    Assert.Equal(0, process.ExitCode);
                    process.Close();
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Error running msbuild: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a app setting from a configuration file.
        /// </summary>
        /// <param name="configFilePath">Path to the configuration file.</param>
        /// <param name="appSettingKey">Setting key.</param>
        /// <returns>Value of the setting.</returns>
        public string GetAppSettingValue(string configFilePath, string appSettingKey)
        {
            var configFile = XDocument.Load(configFilePath);
            var testSetting = (from settingEl in configFile.Descendants("appSettings").Elements()
                               where settingEl.Attribute("key").Value == appSettingKey
                               select settingEl.Attribute("value").Value).Single();
            return testSetting;
        }

        /// <summary>
        /// Gets the value of a node within a configuration file.
        /// </summary>
        /// <param name="configFilePath">Path to the configuration file.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns>Value of the node.</returns>
        public string GetConfigNodeValue(string configFilePath, string nodeName)
        {
            var configFile = XDocument.Load(configFilePath);
            return configFile.Descendants(nodeName).Single().Value;
        }

        /// <summary>
        /// At the end of tests, delete the output path for the tested projects.
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(this.OutputPath))
            {
                Directory.Delete(this.OutputPath, recursive: true);
            }
        }
    }
}
