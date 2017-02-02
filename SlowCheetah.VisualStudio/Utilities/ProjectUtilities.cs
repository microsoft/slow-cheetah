using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SlowCheetah.VisualStudio
{
    public static class ProjectUtilities
    {
        private static string SupportedProjectExtensionsKey = @"XdtTransforms\SupportedProjectExtensions";
        private static string SupportedItemExtensionsKey = @"XdtTransforms\SupportedItemExtensions";

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
        public static string[] GetProjectConfigurations(EnvDTE.Project project)
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

            return configurations.ToArray();
        }

        /// <summary>
        /// Retrieves the supported project extensions from the package settings
        /// </summary>
        /// <param name="settingsManager"> </param>
        /// <returns>List of supported project extensions starting with '.'</returns>
        public static string[] GetSupportedProjectExtensions(IVsSettingsManager settingsManager)
        {
            return GetSupportedExtensions(settingsManager, SupportedProjectExtensionsKey);
        }

        /// <summary>
        /// Retrieves the supported item extensions from the package settings
        /// </summary>
        /// <param name="settingsManager"></param>
        /// <returns></returns>
        public static string[] GetSupportedItemExtensions(IVsSettingsManager settingsManager)
        {
            return GetSupportedExtensions(settingsManager, SupportedItemExtensionsKey);
        }

        private static string[] GetSupportedExtensions(IVsSettingsManager settingsManager, string rootKey)
        {
            ErrorHandler.ThrowOnFailure(settingsManager.GetReadOnlySettingsStore((uint)__VsSettingsScope.SettingsScope_Configuration, out var settings));
            ErrorHandler.ThrowOnFailure(settings.GetSubCollectionCount(rootKey, out uint count));

            string[] supportedExtensions = new string[count];

            for (uint i = 0; i != count; ++i)
            {
                ErrorHandler.ThrowOnFailure(settings.GetSubCollectionName(rootKey, i, out string keyName));
                supportedExtensions[i] = keyName;
            }

            return supportedExtensions;
        }

        
    }
}
