// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah.VisualStudio
{
    using System;

    /// <summary>
    /// List of Guids necessary for the SlowCheetah extension
    /// </summary>
    public static class Guids
    {
        /// <summary>
        /// Guid string for the SlowCheetah Visual Studio Package
        /// </summary>
        public const string GuidSlowCheetahPkgString = "9eb9f150-fcc9-4db8-9e97-6aef2011017c";

        /// <summary>
        /// Guid string for the SlowCheetah commands
        /// </summary>
        public const string GuidSlowCheetahCmdSetString = "eab4615a-3384-42bd-9589-e2df97a783ee";

        /// <summary>
        /// Guid for the SlowCheetah commands
        /// </summary>
        public static readonly Guid GuidSlowCheetahCmdSet = new Guid(GuidSlowCheetahCmdSetString);
    }
}
