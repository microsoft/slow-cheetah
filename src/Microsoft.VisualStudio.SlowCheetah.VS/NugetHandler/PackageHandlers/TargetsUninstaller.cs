// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System.Linq;
    using EnvDTE;
    using Microsoft.Build.Construction;
    using Microsoft.VisualStudio.Shell;
    using TPL = System.Threading.Tasks;

    /// <summary>
    /// Uninstalls old SlowCheetah targets from the user's project file
    /// </summary>
    internal class TargetsUninstaller : BasePackageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TargetsUninstaller"/> class.
        /// </summary>
        /// <param name="successor">The successor with the same package</param>
        public TargetsUninstaller(IPackageHandler successor)
            : base(successor)
        {
        }

        /// <inheritdoc/>
        public override async TPL.Task Execute(Project project)
        {
            // We handle any NuGet package logic before editing the project file
            await this.Successor.Execute(project);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

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
