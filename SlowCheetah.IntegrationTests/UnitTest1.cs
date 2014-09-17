using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Build.Evaluation;

namespace SlowCheetah.IntegrationTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void BuildConsoleApp()
        {
            var p = new Project(@"E:\Project\GitHub\cfbarbero\slow-cheetah\FunctionalTests\ConsoleApp\ConsoleApp.csproj");
            p.Build();
        }
    }
}
