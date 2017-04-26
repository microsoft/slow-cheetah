// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EnvDTE;
    using Microsoft.Build.Construction;

    /// <summary>
    /// Uninstalls old SlowCheetah targets from the user's project file
    /// </summary>
    internal class TargetsUninstaller : PackageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TargetsUninstaller"/> class.
        /// </summary>
        /// <param name="package">VS package</param>
        public TargetsUninstaller(IServiceProvider package)
            : base(package)
        {
        }

        /// <inheritdoc/>
        internal override void Execute(Project project)
        {
            // We handle any NuGet package logic before editing the project file
            if (this.Successor != null)
            {
                this.Successor.Execute(project);
            }

            project.Save();
            ProjectRootElement projectRoot = ProjectRootElement.Open(project.FullName);
            foreach (ProjectPropertyGroupElement propertyGroup in projectRoot.PropertyGroups.Where(pg => pg.Label.Equals("SlowCheetah")))
            {
                projectRoot.RemoveChild(propertyGroup);
            }

            foreach (ProjectImportElement import in projectRoot.Imports.Where(i => i.Label == "SlowCheetah" || i.Project == "$(SlowCheetahTargets)"))
            {
                projectRoot.RemoveChild(import);
            }

            projectRoot.Save();
        }
    }
}
