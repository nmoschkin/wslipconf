using System;

namespace WSLIPConf.Helpers
{
    /// <summary>
    /// Specifies port proxy type as flags.
    /// </summary>
    /// <remarks>
    /// One of <see cref="SourceV4"/> or <see cref="ProxyType.SourceV6"/> is combined with <br />
    /// one of <see cref="DestV4"/> or <see cref="ProxyType.DestV6"/> to create the 4 legal <br />
    /// port proxy map types:<br /><br />
    /// <see cref="ProxyType.V4ToV4" />: IPv4 to IPv4 map<br />
    /// <see cref="ProxyType.V6ToV6" />: IPv6 to IPv6 map<br />
    /// <see cref="ProxyType.V4ToV6" />: IPv4 to IPv6 map<br />
    /// <see cref="ProxyType.V6ToV4" />: IPv6 to IPv4 map<br />
    /// </remarks>
    [Flags]
    public enum ProxyType
    {
        /// <summary>
        /// IPv4 to IPv4 map
        /// </summary>
        V4ToV4 = 0x11,

        /// <summary>
        /// IPv6 to IPv6 map
        /// </summary>
        V6ToV6 = 0x22,

        /// <summary>
        /// IPv4 to IPv6 map
        /// </summary>
        V4ToV6 = 0x12,

        /// <summary>
        /// IPv6 to IPv4 map
        /// </summary>
        V6ToV4 = 0x21,

        /// <summary>
        /// Source is IPv4 address
        /// </summary>
        SourceV4 = 0x10,

        /// <summary>
        /// Source is IPv6 address
        /// </summary>
        SourceV6 = 0x20,

        /// <summary>
        /// Destination is IPv4 address
        /// </summary>
        DestV4 = 0x01,

        /// <summary>
        /// Destination is IPv6 address
        /// </summary>
        DestV6 = 0x02
    }
}