using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowCheetah.UnitTests
{
    /// <summary>
    /// Utilities class for SlowCheetah tests.
    /// </summary>
    public static class TestUtilities
    {
        /// <summary>
        /// Example source file for transform testing.
        /// </summary>
        public const string Source01 =
            @"<?xml version=""1.0""?>
            <configuration>
                <appSettings>
                    <add key=""setting01"" value=""default01""/> 
                    <add key=""setting02"" value=""default02""/> 
                </appSettings>
            </configuration>";

        /// <summary>
        /// Example transform file for transform testing.
        /// </summary>
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

        /// <summary>
        /// Example result file for transform testing.
        /// </summary>
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
