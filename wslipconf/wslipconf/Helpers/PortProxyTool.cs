using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using WSLIPConf.Models;

namespace WSLIPConf.Helpers
{
    [Flags]
    public enum ProxyType
    {
        V4ToV4 = 0x11,
        V6ToV6 = 0x22,
        V4ToV6 = 0x12,
        V6ToV4 = 0x21,
        SourceV4 = 0x10,
        SourceV6 = 0x20,
        DestV4 = 0x01,
        DestV6 = 0x02
    }

    public enum ProxyProtocol
    {
        Tcp,
        Udp
    }

    public class PortProxyTool
    {
        public const string RegistryKey = "SYSTEM\\CurrentControlSet\\Services\\PortProxy";

        public static ProxyType GetProxyType(WSLMapping mapping)
        {
            if (mapping.SourceAddress.AddressFamily == AddressFamily.InterNetwork
                && mapping.DestinationAddress.AddressFamily == AddressFamily.InterNetwork)
                return ProxyType.V4ToV4;
            else if (mapping.SourceAddress.AddressFamily == AddressFamily.InterNetworkV6
                && mapping.DestinationAddress.AddressFamily == AddressFamily.InterNetworkV6)
                return ProxyType.V6ToV6;
            else if (mapping.SourceAddress.AddressFamily == AddressFamily.InterNetworkV6
                && mapping.DestinationAddress.AddressFamily == AddressFamily.InterNetwork)
                return ProxyType.V6ToV4;
            else if (mapping.SourceAddress.AddressFamily == AddressFamily.InterNetwork
                && mapping.DestinationAddress.AddressFamily == AddressFamily.InterNetworkV6)
                return ProxyType.V4ToV6;
            else
                return ProxyType.V4ToV4;
        }

        public static string GetProxyTypeString(ProxyType pt)
        {
            switch (pt)
            {
                case ProxyType.V6ToV4:
                    return "v6tov4";

                case ProxyType.V4ToV6:
                    return "v4tov6";

                case ProxyType.V6ToV6:
                    return "v6tov6";

                case ProxyType.V4ToV4:
                default:
                    return "v4tov4";
            }
        }

        public static bool ApplyMapping(WSLMapping mapping)
        {
            return ApplyMapping(mapping, true);
        }

        private static bool ApplyMapping(WSLMapping mapping, bool triggerUpdate)
        {
            var pp = mapping.Protocol;
            var pt = GetProxyType(mapping);

            string skey = $"{RegistryKey}\\{GetProxyTypeString(pt)}\\{pp.ToString().ToLower()}";

            using (var key = Registry.LocalMachine.CreateSubKey(skey, true))
            {
                if (key == null) return false;

                string kname = $"{mapping.SourceAddress}/{mapping.SourcePort}";
                string kvalue = $"{mapping.DestinationAddress}/{mapping.DestinationPort}";

                key.SetValue(kname, kvalue);
            }

            if (triggerUpdate)
            {
                return ServiceControl.SendRefreshSignal();
            }
            else
            {
                return true;
            }
        }

        public static void ClearMappings()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(RegistryKey, true))
            {
                if (key == null) return;

                var subkeys = key.GetSubKeyNames();
                foreach (var k in subkeys)
                {
                    key.DeleteSubKeyTree(k);
                }
            }

            ServiceControl.SendRefreshSignal();
        }

        public static IList<WSLMapping> GetProxyMappingByType(ProxyType pt = ProxyType.V4ToV4, ProxyProtocol pp = ProxyProtocol.Tcp)
        {
            string skey = $"{RegistryKey}\\{GetProxyTypeString(pt)}\\{pp.ToString().ToLower()}";
            List<WSLMapping> map = new List<WSLMapping>();
            int i = 0;

            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(skey))
                {
                    if (key == null) return null;

                    var sources = key.GetValueNames();
                    if (sources == null || sources.Length == 0) return null;

                    foreach (var src in sources)
                    {
                        var pv = src.Split("/");
                        IPAddress sip, dip;
                        int sport, dport;

                        if (pv != null && pv.Length == 2)
                        {
                            sip = IPAddress.Parse(pv[0]);
                            sport = int.Parse(pv[1]);

                            pv = ((string)key.GetValue(src)).Split("/");

                            if (pv != null && pv.Length == 2)
                            {
                                dip = IPAddress.Parse(pv[0]);
                                dport = int.Parse(pv[1]);

                                var newMap = new WSLMapping
                                {
                                    SourceAddress = sip,
                                    SourcePort = sport,
                                    DestinationAddress = dip,
                                    DestinationPort = dport,
                                    Name = "New Map #" + ++i
                                };

                                map.Add(newMap);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            catch
            {
            }

            return map;
        }

        public static bool SetPortProxies(IEnumerable<WSLMapping> mappings, bool clearFirst = true)
        {
            if (clearFirst) ClearMappings();

            foreach (var mapping in mappings)
            {
                ApplyMapping(mapping, false);
            }

            return ServiceControl.SendRefreshSignal();
        }

        public static IList<WSLMapping> GetPortProxies()
        {
            var map = new List<WSLMapping>();
            IList<WSLMapping> res;

            res = GetProxyMappingByType(ProxyType.V4ToV4);
            if (res != null) map.AddRange(res);

            res = GetProxyMappingByType(ProxyType.V6ToV6);
            if (res != null) map.AddRange(res);

            res = GetProxyMappingByType(ProxyType.V4ToV6);
            if (res != null) map.AddRange(res);

            res = GetProxyMappingByType(ProxyType.V6ToV4);
            if (res != null) map.AddRange(res);

            res = GetProxyMappingByType(ProxyType.V4ToV4, ProxyProtocol.Udp);
            if (res != null) map.AddRange(res);

            res = GetProxyMappingByType(ProxyType.V6ToV6, ProxyProtocol.Udp);
            if (res != null) map.AddRange(res);

            res = GetProxyMappingByType(ProxyType.V4ToV6, ProxyProtocol.Udp);
            if (res != null) map.AddRange(res);

            res = GetProxyMappingByType(ProxyType.V6ToV4, ProxyProtocol.Udp);
            if (res != null) map.AddRange(res);

            return map;
        }
    }
}