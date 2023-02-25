using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using WSLIPConf.Models;

namespace WSLIPConf.Helpers
{
    public class MappingChangedEventArgs : EventArgs
    {
        public virtual IReadOnlyList<WSLMapping> ActiveMappings { get; }

        public virtual IReadOnlyList<WSLMapping> MappingsAdded { get; }

        public virtual IReadOnlyList<WSLMapping> MappingsRemoved { get; }

        public MappingChangedEventArgs(IEnumerable<WSLMapping> mappings = null, IEnumerable<WSLMapping> mappingsAdded = null, IEnumerable<WSLMapping> mappingsRemoved = null)
        {
            ActiveMappings = mappings != null ? new List<WSLMapping>(mappings).ToArray() : null;
            MappingsAdded = mappingsAdded != null ? new List<WSLMapping>(mappingsAdded).ToArray() : null;
            MappingsRemoved = mappingsRemoved != null ? new List<WSLMapping>(mappingsRemoved).ToArray() : null;
        }
    }

    /// <summary>
    /// Windows System Port Proxy Configuration Tool
    /// </summary>
    /// <remarks>
    /// This class manipulates the Windows operating system on the local machine in real time and requires elevated privileges to run.
    /// </remarks>
    public static class PortProxyTool
    {
        public static event EventHandler<MappingChangedEventArgs> SystemMappingChanged;

        /// <summary>
        /// Port proxy windows registry home key.
        /// </summary>
        public const string RegistryKey = "SYSTEM\\CurrentControlSet\\Services\\PortProxy";

        /// <summary>
        /// Return the proxy type of the specified mapping object.
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get the registry key name for the specified proxy type
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Apply the specified mapping object to the local machine and trigger a system update
        /// </summary>
        /// <param name="mapping">The mapping object to apply</param>
        /// <returns>True if completely successful.</returns>
        /// <remarks>
        /// If <see cref="WSLMapping.IsSuspended"/> is true, this method will remove that entry from the local machine.
        /// </remarks>
        public static bool SetPortProxy(WSLMapping mapping)
        {
            return SetPortProxy(mapping, true);
        }

        /// <summary>
        /// Remove the proxy rule matching the specified mapping object from the local machine.
        /// </summary>
        /// <param name="mapping">The mapping object to remove</param>
        /// <returns></returns>
        public static bool RemovePortProxy(WSLMapping mapping)
        {
            return RemovePortProxy(mapping, false);
        }

        /// <summary>
        /// Apply the specified mapping object to the local machine
        /// </summary>
        /// <param name="mapping">The mapping object to apply</param>
        /// <param name="triggerUpdate">True to send the service refresh signal.</param>
        /// <returns></returns>
        private static bool SetPortProxy(WSLMapping mapping, bool triggerUpdate)
        {
            if (mapping.IsSuspended) return RemovePortProxy(mapping, triggerUpdate);

            var pp = mapping.Protocol;
            var pt = GetProxyType(mapping);

            string skey = $"{RegistryKey}\\{GetProxyTypeString(pt)}\\{pp.ToString().ToLower()}";

            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(skey, true))
                {
                    if (key == null) return false;

                    string kname = $"{mapping.SourceAddress}/{mapping.SourcePort}";
                    string kvalue = $"{mapping.DestinationAddress}/{mapping.DestinationPort}";

                    key.SetValue(kname, kvalue);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex);
#endif
                return false;
            }

            if (triggerUpdate)
            {
                var ret = ServiceControl.SendRefreshSignal();
                if (ret)
                {
                    mapping.OnPropertyChanged(nameof(WSLMapping.IsOnSystem));
                    SystemMappingChanged?.Invoke(App.Current, new MappingChangedEventArgs(mappingsAdded: new[] { mapping }));
                }
                return ret;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Remove the proxy rule matching the specified mapping object from the local machine.
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="triggerUpdate">True to send the service refresh signal.</param>
        /// <returns></returns>
        private static bool RemovePortProxy(WSLMapping mapping, bool triggerUpdate)
        {
            var pp = mapping.Protocol;
            var pt = GetProxyType(mapping);

            string skey = $"{RegistryKey}\\{GetProxyTypeString(pt)}\\{pp.ToString().ToLower()}";

            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(skey, true))
                {
                    if (key == null) return false;

                    string kname = $"{mapping.SourceAddress}/{mapping.SourcePort}";
                    string kvalue = $"{mapping.DestinationAddress}/{mapping.DestinationPort}";

                    var val = (string)key.GetValue(kname);

                    if (val == kvalue)
                    {
                        key.DeleteValue(kname);
                    }
                    else if (val == null)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex);
#endif
                return false;
            }

            if (triggerUpdate)
            {
                var ret = ServiceControl.SendRefreshSignal();
                if (ret)
                {
                    mapping.OnPropertyChanged(nameof(WSLMapping.IsOnSystem));
                    SystemMappingChanged?.Invoke(App.Current, new MappingChangedEventArgs(mappingsRemoved: new[] { mapping }));
                }
                return ret;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Clear all current port proxy mappings on the local machine (sight-unseen) and send a system refresh signal.
        /// </summary>
        /// <param name="raiseEvent">True to raise the <see cref="SystemMappingChanged"/> event.</param>
        public static void ClearPortProxies(bool raiseEvent = true)
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

            if (ServiceControl.SendRefreshSignal() && raiseEvent)
            {
                SystemMappingChanged?.Invoke(App.Current, new MappingChangedEventArgs());
            }
        }

        /// <summary>
        /// Returns true if the specified mapping was found on the local machine.
        /// </summary>
        /// <param name="mapping">The mapping to test.</param>
        /// <param name="matchSourceOnly">True to only require that the source address/port match.</param>
        /// <returns></returns>
        public static bool GetSystemHasProxyMapping(WSLMapping mapping, bool matchSourceOnly = false)
        {
            var pp = mapping.Protocol;
            var pt = GetProxyType(mapping);

            string skey = $"{RegistryKey}\\{GetProxyTypeString(pt)}\\{pp.ToString().ToLower()}";

            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(skey, false))
                {
                    if (key == null) return false;

                    string kname = $"{mapping.SourceAddress}/{mapping.SourcePort}";
                    string kvalue = $"{mapping.DestinationAddress}/{mapping.DestinationPort}";

                    if (key.GetValueNames().Contains(kname))
                    {
                        if (matchSourceOnly) return true;

                        var val = (string)key.GetValue(kname);

                        if (val == kvalue)
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        /// Enumerate all currently active port mappings with the specified <paramref name="proxyProtocol"/> and <paramref name="proxyProtocol"/> on the local machine.
        /// </summary>
        /// <param name="proxyType">The proxy type.</param>
        /// <param name="proxyProtocol">The proxy protocol.</param>
        /// <returns></returns>
        public static IList<WSLMapping> GetProxyMappingByType(ProxyType proxyType = ProxyType.V4ToV4, ProxyProtocol proxyProtocol = ProxyProtocol.Tcp)
        {
            string skey = $"{RegistryKey}\\{GetProxyTypeString(proxyType)}\\{proxyProtocol.ToString().ToLower()}";
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
                                    Name = "Port " + sport.ToString() + " Rule",
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

        /// <summary>
        /// Enable the specified port mappings on the local machine.
        /// </summary>
        /// <param name="mappings">The mappings to enable.</param>
        /// <param name="clearFirst">Clear the current port mappings from the local machine, first.</param>
        /// <returns></returns>
        public static bool SetPortProxies(IEnumerable<WSLMapping> mappings, bool clearFirst = true)
        {
            if (clearFirst) ClearPortProxies(false);

            var added = new List<WSLMapping>();
            var removed = new List<WSLMapping>();

            foreach (var mapping in mappings)
            {
                SetPortProxy(mapping, false);
                if (mapping.IsSuspended)
                {
                    removed.Add(mapping);
                }
                else
                {
                    added.Add(mapping);
                }
            }

            var ret = ServiceControl.SendRefreshSignal();

            if (ret)
            {
                foreach (var mapping in mappings)
                {
                    mapping.OnPropertyChanged(nameof(WSLMapping.IsOnSystem));
                }

                SystemMappingChanged?.Invoke(App.Current, new MappingChangedEventArgs(mappings, added, removed));
            }

            return ret;
        }

        public static bool RemovePortProxies(IEnumerable<WSLMapping> mappings)
        {
            foreach (var mapping in mappings)
            {
                RemovePortProxy(mapping, false);
            }

            var ret = ServiceControl.SendRefreshSignal();

            if (ret)
            {
                foreach (var mapping in mappings)
                {
                    mapping.OnPropertyChanged(nameof(WSLMapping.IsOnSystem));
                }

                SystemMappingChanged?.Invoke(App.Current, new MappingChangedEventArgs(GetPortProxies(), mappingsRemoved: mappings));
            }

            return ret;
        }

        /// <summary>
        /// Enumerate all currently active port proxy mappings on the local machine.
        /// </summary>
        /// <returns></returns>
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