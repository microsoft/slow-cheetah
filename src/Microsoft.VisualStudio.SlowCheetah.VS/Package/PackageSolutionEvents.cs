﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Contains solution events for <see cref="SlowCheetahPackage"/>.
    /// </summary>
    public class PackageSolutionEvents : IVsUpdateSolutionEvents, IDisposable
    {
        private uint solutionUpdateCookie = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageSolutionEvents"/> class.
        /// </summary>
        /// <param name="asyncPackage">The VSPackage.</param>
        /// <param name="errorListProvider">The error list provider.</param>
        public PackageSolutionEvents(AsyncPackage asyncPackage, ErrorListProvider errorListProvider)
        {
            this.Package = asyncPackage ?? throw new ArgumentNullException(nameof(asyncPackage));
            this.ErrorListProvider = errorListProvider ?? throw new ArgumentNullException(nameof(errorListProvider));
        }

        private ErrorListProvider ErrorListProvider { get; }

        private AsyncPackage Package { get; }

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
            this.ErrorListProvider.Tasks.Clear();
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

        /// <summary>
        /// Asynchronously registers the solution eve.
        /// </summary>
        /// <returns>Async task.</returns>
        public async System.Threading.Tasks.Task RegisterEventsAsync()
        {
            await this.Package.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (await this.Package.GetServiceAsync(typeof(SVsSolutionBuildManager)) is IVsSolutionBuildManager solutionBuildManager)
            {
                solutionBuildManager.AdviseUpdateSolutionEvents(this, out this.solutionUpdateCookie);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.solutionUpdateCookie > 0)
            {
                this.Package.JoinableTaskFactory.Run(async () =>
                {
                    await this.Package.JoinableTaskFactory.SwitchToMainThreadAsync();

                    if (await this.Package.GetServiceAsync(typeof(SVsSolutionBuildManager)) is IVsSolutionBuildManager solutionBuildManager)
                    {
                        solutionBuildManager.UnadviseUpdateSolutionEvents(this.solutionUpdateCookie);
                    }
                });
            }
        }
    }
}
