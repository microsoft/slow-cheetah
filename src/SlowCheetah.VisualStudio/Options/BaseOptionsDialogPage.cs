namespace SlowCheetah.VisualStudio
{
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Base class for options page
    /// </summary>
    internal abstract class BaseOptionsDialogPage : DialogPage
    {
        /// <summary>
        /// Registry key for all options
        /// </summary>
        protected const string RegOptionsKey = "ConfigTransform";
    }
}
