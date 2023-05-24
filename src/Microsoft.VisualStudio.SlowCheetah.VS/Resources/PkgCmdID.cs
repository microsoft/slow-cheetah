// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    /// <summary>
    /// List of the Command IDs for SlowCheetah.
    /// </summary>
    public static class PkgCmdID
    {
        /// <summary>
        /// ID for the "Add Transform" command.
        /// </summary>
        public const uint CmdIdAddTransform = 0x100;

        /// <summary>
        /// ID for the "Preview Transform" command.
        /// </summary>
        public const uint CmdIdPreviewTransform = 0x101;
    }
}
