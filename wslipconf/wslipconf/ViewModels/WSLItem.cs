using WSLIPConf.Helpers;

namespace WSLIPConf.ViewModels
{
    /// <summary>
    /// Encapsulates a WSL distribution with a name/title.
    /// </summary>
    public class WSLItem
    {
        /// <summary>
        /// The WSL distribution
        /// </summary>
        public WSLDistribution Distribution { get; set; }

        /// <summary>
        /// The name/title
        /// </summary>
        public string Name { get; set; }
    }
}