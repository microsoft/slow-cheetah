// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Utilities class for the Visual Studio Extension Package that deals specifically with projects
    /// </summary>
    public static class ProjectUtilities
    {
        private const string SupportedProjectExtensionsKey = @"XdtTransforms\SupportedProjectExtensions";
        private const string SupportedItemExtensionsKey = @"XdtTransforms\SupportedItemExtensions";

        private static IEnumerable<string> supportedProjectExtensions;
        private static IEnumerable<string> supportedItemExtensions;

        /// <summary>
        /// Gets the DTE from current context
        /// </summary>
        /// <returns>The Visual Studio DTE object</returns>
        public static DTE GetDTE()
        {
            return (DTE)Package.GetGlobalService(typeof(DTE));
        }

        /// <summary>
        /// Verifies if a single object is selected
        /// </summary>
        /// <param name="hierarchy">Current selected project hierarchy</param>
        /// <param name="itemid">ID of the selected item</param>
        /// <returns>True if a single item is selected</returns>
        public static bool IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out uint itemid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            hierarchy = null;
            itemid = VSConstants.VSITEMID_NIL;
            int hr = VSConstants.S_OK;

            IVsMonitorSelection monitorSelection = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            IVsSolution solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            if (monitorSelection == null || solution == null)
            {
                return false;
            }

            IVsMultiItemSelect multiItemSelect = null;
            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr selectionContainerPtr = IntPtr.Zero;

            try
            {
                hr = monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainerPtr);

                if (ErrorHandler.Failed(hr) || hierarchyPtr == IntPtr.Zero || itemid == VSConstants.VSITEMID_NIL)
                {
                    // there is no selection
                    return false;
                }

                if (multiItemSelect != null)
                {
                    // multiple items are selected
                    return false;
                }

                if (itemid == VSConstants.VSITEMID_ROOT)
                {
                    // there is a hierarchy root node selected, thus it is not a single item inside a project
                    return false;
                }

                hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
                if (hierarchy == null)
                {
                    return false;
                }

                Guid guidProjectID = Guid.Empty;

                if (ErrorHandler.Failed(solution.GetGuidOfProject(hierarchy, out guidProjectID)))
                {
                    return false; // hierarchy is not a project inside the Solution if it does not have a ProjectID Guid
                }

                // if we got this far then there is a single project item selected
                return true;
            }
            finally
            {
                if (selectionContainerPtr != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainerPtr);
                }

                if (hierarchyPtr != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPtr);
                }
            }
        }

        /// <summary>
        /// Gets all project configurations
        /// </summary>
        /// <param name="project">Current open project</param>
        /// <returns>List of configuration names for that project</returns>
        public static IEnumerable<string> GetProjectConfigurations(Project project)
        {
            List<string> configurations = new List<string>();

            if (project != null && project.ConfigurationManager != null && project.ConfigurationManager.ConfigurationRowNames != null)
            {
                foreach (object objConfigName in (object[])project.ConfigurationManager.ConfigurationRowNames)
                {
                    string configName = objConfigName as string;
                    if (!string.IsNullOrWhiteSpace(configName))
                    {
                        configurations.Add(configName);
                    }
                }
            }

            return configurations;
        }

        /// <summary>
        /// Gets all project configurations
        /// </summary>
        /// <param name="hierarchy">Current project hierarchy</param>
        /// <returns>List of configuration names for that project</returns>
        public static IEnumerable<string> GetProjectConfigurations(IVsHierarchy hierarchy)
        {
            Project project = PackageUtilities.GetAutomationFromHierarchy<Project>(hierarchy, (uint)VSConstants.VSITEMID.Root);
            return GetProjectConfigurations(project);
        }

        /// <summary>
        /// Retrieves the supported project extensions from the package settings
        /// </summary>
        /// <param name="settingsManager">The settings manager for the project</param>
        /// <returns>List of supported project extensions starting with '.'</returns>
        public static IEnumerable<string> GetSupportedProjectExtensions(IVsSettingsManager settingsManager)
        {
            if (supportedProjectExtensions == null)
            {
                supportedProjectExtensions = GetSupportedExtensions(settingsManager, SupportedProjectExtensionsKey);
            }

            return supportedProjectExtensions;
        }

        /// <summary>
        /// Retrieves the supported item extensions from the package settings
        /// </summary>
        /// <param name="settingsManager">The settings manager for the project</param>
        /// <returns>A list of supported item extensions</returns>
        public static IEnumerable<string> GetSupportedItemExtensions(IVsSettingsManager settingsManager)
        {
            if (supportedItemExtensions == null)
            {
                supportedItemExtensions = GetSupportedExtensions(settingsManager, SupportedProjectExtensionsKey);
            }

            return supportedItemExtensions;
        }

        /// <summary>
        /// Verifies if the given project is a Web Application.
        /// Checks the type GUIDs for that project.
        /// </summary>
        /// <param name="project">Project to verify</param>
        /// <returns>True if a subtype GUID matches the Web App Guid in Resources</returns>
        public static bool IsProjectWebApp(IVsProject project)
        {
            if (project is IVsAggregatableProject aggregatableProject)
            {
                aggregatableProject.GetAggregateProjectTypeGuids(out string projectTypeGuidStrings);
                var projectTypeGuids = new List<Guid>();
                foreach (string gs in projectTypeGuidStrings.Split(';'))
                {
                    try
                    {
                        var guid = new Guid(gs);

                        projectTypeGuids.Add(guid);
                    }
                    catch
                    {
                        // Don't add to list of guids
                    }
                }

                return projectTypeGuids.Contains(Guids.GuidWebApplication);
            }

            return false;
        }

        private static IEnumerable<string> GetSupportedExtensions(IVsSettingsManager settingsManager, string rootKey)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ErrorHandler.ThrowOnFailure(settingsManager.GetReadOnlySettingsStore((uint)__VsSettingsScope.SettingsScope_Configuration, out IVsSettingsStore settings));
            ErrorHandler.ThrowOnFailure(settings.GetSubCollectionCount(rootKey, out uint count));

            List<string> supportedExtensions = new List<string>();

            for (uint i = 0; i != count; ++i)
            {
                ErrorHandler.ThrowOnFailure(settings.GetSubCollectionName(rootKey, i, out string keyName));
                supportedExtensions.Add(keyName);
            }

            return supportedExtensions;
        }
    }
}
