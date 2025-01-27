// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.Tests
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
