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
    public class WebAppTests : ConfigTransformTestsBase
    {
        
        [TestMethod]
        [TestCategory("FunctionalTests")]
        public void WebApp_OtherConfig_IsTransformed()
        {
            var projectName = "WebApplication";
            BuildProject(projectName);

            var configFilePath = Path.Combine(OutputPath, "Other.config");

            var testNodeValue = GetConfigNodeValue(configFilePath, "TestNode");

            Assert.AreEqual("Debug", testNodeValue);
        }
    }
}
