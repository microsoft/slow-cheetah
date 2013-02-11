namespace SlowCheetah_UnitTests {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SlowCheetah.VisualStudio;
    using System.IO;

    [TestClass]
    public class TransformTest : BaseTest {

        [TestMethod]
        public void TestTransform01() {
           string sourceFile = this.WriteTextToTempFile(Consts.Source01);
            string transformFile = this.WriteTextToTempFile(Consts.Transform01);
            string expectedResultFile = this.WriteTextToTempFile(Consts.Result01);

            string destFile = this.GetTempFilename(true);
            ITransformer transformer = new Transformer();
            transformer.Transform(sourceFile, transformFile, destFile);

            Assert.IsTrue(File.Exists(sourceFile));
            Assert.IsTrue(File.Exists(transformFile));
            Assert.IsTrue(File.Exists(destFile));

            string actualResult = File.ReadAllText(destFile);
            string expectedResult = File.ReadAllText(expectedResultFile);
            Assert.AreEqual(expectedResult.Trim(), actualResult.Trim());
        }

        private static class Consts {
            public const string Source01 =
@"<?xml version=""1.0""?>
<configuration>
  <appSettings>
    <add key=""setting01"" value=""default01""/> 
    <add key=""setting02"" value=""default02""/> 
  </appSettings>
</configuration>";

            public const string Transform01 =
@"<?xml version=""1.0""?>
<configuration xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
  <appSettings>
    <add key=""setting01"" value=""debug01""
         xdt:Locator=""Match(key)"" xdt:Transform=""Replace"" />
    
    <add key=""setting02"" value=""debug02""
         xdt:Locator=""Match(key)"" xdt:Transform=""Replace"" />
  </appSettings>

</configuration>";

            public const string Result01 =
@"<?xml version=""1.0""?>
<configuration>
  <appSettings>
    <add key=""setting01"" value=""debug01""/> 
    <add key=""setting02"" value=""debug02""/> 
  </appSettings>
</configuration>";
        }
    }
}
