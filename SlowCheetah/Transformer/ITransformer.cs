namespace SlowCheetah
{
    /// <summary>
    /// Interface for file tranformers
    /// </summary>
    public interface ITransformer
    {
        /// <summary>
        /// Main method that tranforms a source file accoring to a transformation file and puts it in a destination file
        /// </summary>
        /// <param name="source">Path to source file</param>
        /// <param name="transform">Path to tranformation file</param>
        /// <param name="destination">Path to destination of transformed file</param>
        void Transform(string source, string transform, string destination);
    }
}
