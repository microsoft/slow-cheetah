// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;

    /// <summary>
    /// List of Guids necessary for the SlowCheetah extension.
    /// </summary>
    public static class Guids
    {
        /// <summary>
        /// Guid string of a Web Application project.
        /// </summary>
        public const string GuidWebApplicationString = "{349c5851-65df-11da-9384-00065b846f21}";

        /// <summary>
        /// Guid string for the SlowCheetah Visual Studio Package.
        /// </summary>
        public const string GuidSlowCheetahPkgString = "9eb9f150-fcc9-4db8-9e97-6aef2011017c";

        /// <summary>
        /// Guid string for the SlowCheetah commands.
        /// </summary>
        public const string GuidSlowCheetahCmdSetString = "eab4615a-3384-42bd-9589-e2df97a783ee";

        /// <summary>
        /// Guid for the SlowCheetah commands.
        /// </summary>
        public static readonly Guid GuidSlowCheetahCmdSet = new Guid(GuidSlowCheetahCmdSetString);

        /// <summary>
        /// Guid of a Web Application project.
        /// </summary>
        public static readonly Guid GuidWebApplication = new Guid(GuidWebApplicationString);
    }
}
