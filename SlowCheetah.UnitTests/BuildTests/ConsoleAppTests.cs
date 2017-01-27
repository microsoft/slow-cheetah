using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace SlowCheetah.UnitTests.BuildTests
{
    [TestClass]
    public class ConsoleAppTests : ConfigTransformTestsBase
    {
        [TestMethod]
        [TestCategory("BuildTests")]
        public void ConsoleApp_AppConfig_IsTransformed()
        {
            var projectName = "ConsoleApp";
            BuildProject(projectName);

            var configFilePath = Path.Combine(OutputPath, "ConsoleApp.exe.config");

            var testSetting = GetAppSettingValue(configFilePath, "TestSetting");

            Assert.AreEqual("Debug", testSetting);
        }

        [TestMethod]
        [TestCategory("BuildTests")]
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
