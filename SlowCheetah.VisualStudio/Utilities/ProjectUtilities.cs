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

        public static string[] GetSupportedProjectExtensions(IVsSettingsManager settingsManager)
        {
            return GetSupportedExtensions(settingsManager, @"XdtTransforms\SupportedProjectExtensions");
        }

        public static string[] GetSupportedItemExtensions(IVsSettingsManager settingsManager)
        {
            return GetSupportedExtensions(settingsManager, @"XdtTransforms\SupportedItemExtensions");
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
