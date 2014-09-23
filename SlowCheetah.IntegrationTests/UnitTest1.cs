using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Logging;
using Microsoft.Build.Framework;

namespace SlowCheetah.IntegrationTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void BuildConsoleApp()
        {
            var p = new Project(@"E:\Project\GitHub\cfbarbero\slow-cheetah\FunctionalTests\ConsoleApp\ConsoleApp.csproj");
            p.SetGlobalProperty("Configuration", "Release");
            var logger = new ConsoleLogger(LoggerVerbosity.Diagnostic);
            p.Build(logger);
        }
    }
}
