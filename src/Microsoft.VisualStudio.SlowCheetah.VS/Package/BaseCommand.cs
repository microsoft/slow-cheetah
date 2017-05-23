// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.ComponentModel.Design;

    /// <summary>
    /// Base class for package commands
    /// </summary>
    public abstract class BaseCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCommand"/> class.
        /// </summary>
        /// <param name="package">The VSPackage</param>
        public BaseCommand(IServiceProvider package)
        {
            this.ServiceProvider = package ?? throw new ArgumentNullException(nameof(package));
        }

        /// <summary>
        /// Gets the ID of the command
        /// </summary>
        public abstract int CommandId { get; }

        /// <summary>
        /// Gets the service provider
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Registers the command in the command service
        /// </summary>
        public void RegisterCommand()
        {
            // Add our command handlers for menu (commands must exist in the .vsct file)
            if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                // create the command for the "Add Transform" query status menu item
                CommandID menuContextCommandID = new CommandID(Guids.GuidSlowCheetahCmdSet, this.CommandId);
                OleMenuCommand menuCommand = new OleMenuCommand(this.OnInvoke, this.OnChange, this.OnBeforeQueryStatus, menuContextCommandID);
                mcs.AddCommand(menuCommand);
            }
        }

        /// <summary>
        /// This event is called when the command status has changed
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Event arguments</param>
        public abstract void OnChange(object sender, EventArgs e);

        /// <summary>
        /// This event is fired when a user right-clicks on a menu, but prior to the menu showing.
        /// This function is used to set the visibility of the menu.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Event arguments</param>
        public abstract void OnBeforeQueryStatus(object sender, EventArgs e);

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Event arguments</param>
        public abstract void OnInvoke(object sender, EventArgs e);
    }
}
