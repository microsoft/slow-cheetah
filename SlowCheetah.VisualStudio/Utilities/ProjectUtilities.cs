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
        private const string _supportedProjectExtensionsKey = @"XdtTransforms\SupportedProjectExtensions";
        private const string _supportedItemExtensionsKey = @"XdtTransforms\SupportedItemExtensions";

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
        /// <param name="settingsManager"> </param>
        /// <returns>List of supported project extensions starting with '.'</returns>
        public static IEnumerable<string> GetSupportedProjectExtensions(IVsSettingsManager settingsManager)
        {
            if (s_supportedProjectExtensions == null)
            {
                s_supportedProjectExtensions = GetSupportedExtensions(settingsManager, _supportedProjectExtensionsKey);
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
                s_supportedItemExtensions = GetSupportedExtensions(settingsManager, _supportedProjectExtensionsKey);
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
    }
}