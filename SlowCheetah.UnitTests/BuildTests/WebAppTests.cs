using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowCheetah.UnitTests.BuildTests
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
