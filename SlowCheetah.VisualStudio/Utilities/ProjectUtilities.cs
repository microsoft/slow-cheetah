// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah.VisualStudio
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
        public static IEnumerable<string> GetProjectConfigurations(EnvDTE.Project project)
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

        private static IEnumerable<string> GetSupportedExtensions(IVsSettingsManager settingsManager, string rootKey)
        {
            IVsSettingsStore settings;
            uint count;
            ErrorHandler.ThrowOnFailure(settingsManager.GetReadOnlySettingsStore((uint)__VsSettingsScope.SettingsScope_Configuration, out settings));
            ErrorHandler.ThrowOnFailure(settings.GetSubCollectionCount(rootKey, out count));

            List<string> supportedExtensions = new List<string>();

            for (uint i = 0; i != count; ++i)
            {
                string keyName;
                ErrorHandler.ThrowOnFailure(settings.GetSubCollectionName(rootKey, i, out keyName));
                supportedExtensions.Add(keyName);
            }

            return supportedExtensions;
        }
    }
}