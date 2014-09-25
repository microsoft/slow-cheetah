using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Logging;
using Microsoft.Build.Framework;
using System.IO;

namespace SlowCheetah.IntegrationTests
{
    [TestClass]
    public class UnitTest1
    {
        public TestContext TestContext { get; set; }
        [TestMethod]
        public void BuildConsoleApp()
        {
            var outputPath = Path.Combine(TestContext.TestRunDirectory, @"ProjectOutput\");
            var p = new Project(@"E:\Project\GitHub\cfbarbero\slow-cheetah\FunctionalTests\ConsoleApp\ConsoleApp.csproj");
            p.SetGlobalProperty("Configuration", "Debug");
            p.SetGlobalProperty("SolutionDir", @"E:\Project\GitHub\cfbarbero\slow-cheetah");
            p.SetGlobalProperty("OutputPath", outputPath);
            var logger = new ConsoleLogger(LoggerVerbosity.Diagnostic);
            Assert.IsTrue(p.Build(logger));

            //ToDo: Assert that config is transformed
        }
    }
}
