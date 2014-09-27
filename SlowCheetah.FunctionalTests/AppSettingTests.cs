using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Logging;
using Microsoft.Build.Framework;
using System.IO;
using System.Xml.Linq;
using System.Linq;


namespace SlowCheetah.IntegrationTests
{
    [TestClass]
    public class AppSettingTests : ConfigTransformTestsBase
    {
        
        [TestMethod]
        [TestCategory("FunctionalTests")]
        public void ConsoleApp_AppConfig_IsTransformed()
        {
            var projectName = "ConsoleApp";
            BuildProject(projectName);

            var configFilePath = Path.Combine(OutputPath, "ConsoleApp.exe.config");

            var testSetting = GetAppSettingValue(configFilePath, "TestSetting");

            Assert.AreEqual("Debug", testSetting);
        }

        [TestMethod]
        [TestCategory("FunctionalTests")]
        public void ConsoleApp_OtherConfig_IsTransformed()
        {
            var projectName = "ConsoleApp";
            BuildProject(projectName);

            var configFilePath = Path.Combine(OutputPath, "Other.config");

            var testNodeValue = GetConfigNodeValue(configFilePath, "TestNode");

            Assert.AreEqual("Debug", testNodeValue);
        }
    }
}
