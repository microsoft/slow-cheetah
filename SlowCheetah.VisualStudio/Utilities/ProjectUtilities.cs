// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System.Collections.Generic;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SlowCheetah.VisualStudio
{
    //Utilities class for the Visual Studio Extension Package that deals specifically with projects
    public static class ProjectUtilities
    {
        private const string SupportedProjectExtensionsKey = @"XdtTransforms\SupportedProjectExtensions";
        private const string SupportedItemExtensionsKey = @"XdtTransforms\SupportedItemExtensions";

        private static IEnumerable<string> s_supportedProjectExtensions;
        private static IEnumerable<string> s_supportedItemExtensions;

        /// <summary>
        /// Gets the DTE from current context
        /// </summary>
        public static DTE GetDTE()
        {
            return (DTE)Package.GetGlobalService(typeof(DTE));
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
        /// <param name="settingsManager"> </param>
        /// <returns>List of supported project extensions starting with '.'</returns>
        public static IEnumerable<string> GetSupportedProjectExtensions(IVsSettingsManager settingsManager)
        {
            if (s_supportedProjectExtensions == null)
            {
                s_supportedProjectExtensions = GetSupportedExtensions(settingsManager, SupportedProjectExtensionsKey);
            }

            return s_supportedProjectExtensions;
        }

        /// <summary>
        /// Retrieves the supported item extensions from the package settings
        /// </summary>
        /// <param name="settingsManager"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetSupportedItemExtensions(IVsSettingsManager settingsManager)
        {
            if (s_supportedItemExtensions == null)
            {
                s_supportedItemExtensions = GetSupportedExtensions(settingsManager, SupportedProjectExtensionsKey);
            }

            return s_supportedItemExtensions;
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

        /// <summary>
        /// Verifies if the given project is a Web Application.
        /// Checks the type GUIDs for that project.
        /// </summary>
        /// <param name="project">Project to verify</param>
        /// <returns>True if a subtype GUID matches the Web App Guid in Resources</returns>
        public static bool IsProjectWebApp(IVsProject project)
        {
            IVsAggregatableProject aggregatableProject = project as IVsAggregatableProject;
            if (aggregatableProject != null)
            {
                string projectTypeGuids;
                aggregatableProject.GetAggregateProjectTypeGuids(out projectTypeGuids);
                List<string> guids = new List<string>(projectTypeGuids.Split(';'));
                return guids.Contains(GuidList.guidWebApplicationString);
            }

            return false;
        }
    }
}
