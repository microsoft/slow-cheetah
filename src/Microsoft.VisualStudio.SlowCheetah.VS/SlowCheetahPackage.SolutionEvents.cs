// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Contains Solution Events
    /// </summary>
    public sealed partial class SlowCheetahPackage : Package, IVsUpdateSolutionEvents
    {
        /// <inheritdoc/>
        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        /// <inheritdoc/>
        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            return VSConstants.S_OK;
        }

        /// <inheritdoc/>
        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            // On solution update, clear all errors generated
            this.errorListProvider.Tasks.Clear();
            return VSConstants.S_OK;
        }

        /// <inheritdoc/>
        public int UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        /// <inheritdoc/>
        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }
    }
}
