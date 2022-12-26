using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace WSLIPConf.Helpers
{
    public class WSLDistribution
    {
        public static event EventHandler<WSLDistribution> SysDistroChanged;

        public static event EventHandler<WSLDistribution> SessionDistroChanged;

        private static ObservableCollection<WSLDistribution> distros = new ObservableCollection<WSLDistribution>();

        public static ObservableCollection<WSLDistribution> Distributions => distros;

        /// <summary>
        /// The current distribution that is the default for the system
        /// </summary>
        public static WSLDistribution SystemDefault { get; private set; }

        /// <summary>
        /// Gets or sets the session default WSL distribution
        /// </summary>
        public static WSLDistribution SessionDefault { get; set; }

        public static WSLDistribution FindByName(string name)
        {
            return distros.FirstOrDefault(x => x.Name?.ToLower() == name?.ToLower());
        }

        public static WSLDistribution RefreshDistributions()
        {
            var lastDefault = SystemDefault?.Name;
            var lastSession = SessionDefault?.Name;

            QueryDistributions(distros);

            SystemDefault = distros.Where(x => x.IsDefault).FirstOrDefault() ?? distros.FirstOrDefault();

            if (lastSession != null)
            {
                var sess = distros.Where(x => x.Name == lastSession).FirstOrDefault();
                SessionDefault = sess;
            }
            else
            {
                SessionDefault = SystemDefault;
            }

            SessionDistroChanged?.Invoke(App.Current, SessionDefault);
            SysDistroChanged?.Invoke(App.Current, SystemDefault);

            return SessionDefault;
        }

        public static TCol QueryDistributions<TCol>(TCol reuseCol = null) where TCol : class, ICollection<WSLDistribution>, new()
        {
            var distros = reuseCol ?? new TCol();
            distros.Clear();

            var p = new Process();
            var wsl = Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\system32\\wsl.exe");

            string cmd = "--list --all --verbose";

            p.StartInfo = new ProcessStartInfo(wsl, cmd)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            p.Start();
            p.WaitForExit();

            var contents = p.StandardOutput.ReadToEnd().Replace("\x0", "");

            var lines = contents.Replace("\r\n", "\n").Split('\n');

            var top = lines[0];
            int c = top.Length;
            int lidx = 2;

            int lc = lines.Length;
            int li = 0;

            for (li = 1; li <= lc; li++)
            {
                var line = lines[li];
                if (string.IsNullOrEmpty(line)) break;

                var def = line[0] == '*';

                var sb = new StringBuilder();
                var cols = new List<string>();

                c = line.Length;

                for (int i = 2; i < c; i++)
                {
                    var ch = line[i];
                    if (!char.IsWhiteSpace(ch))
                    {
                        sb.Append(ch);
                        if (lidx == -1) lidx = i;
                    }
                    else if (sb.Length > 0)
                    {
                        cols.Add(sb.ToString());
                        lidx = -1;

                        sb.Clear();
                    }
                }
                if (sb.Length > 0 && lidx != -1)
                {
                    cols.Add(sb.ToString());
                }

                distros.Add(new WSLDistribution()
                {
                    Name = cols[0],
                    IsDefault = def,
                    IsRunning = cols[1] == "Running",
                    Version = int.Parse(cols[2]),
                });
            }

            if (distros.Count > 0)
            {
                if (distros.Count(x => x.IsDefault) == 0)
                {
                    distros.First().IsDefault = true;
                }
            }

            return distros;
        }

        public override string ToString()
        {
            return $"{(IsDefault ? "[Default] " : "")}{Name} ({(IsRunning ? "Running" : "Stopped")})";
        }

        public string Name { get; private set; }

        public bool IsRunning { get; private set; }
        public bool IsDefault { get; private set; }
        public int Version { get; private set; }

        private WSLDistribution()
        { }

        public static WSLDistribution DefaultDistribution { get; set; }

        public IPAddress GetWslIpAddress()
        {
            return Interfaces.Where(x =>
                x.Address.AddressFamily == AddressFamily.InterNetwork
                && x.IsMulticast
            )?.FirstOrDefault()?.Address;
        }

        public IPAddress GetWslIpV6Address()
        {
            return Interfaces.Where(x =>
                x.Address.AddressFamily == AddressFamily.InterNetworkV6
                && x.IsMulticast
            )?.FirstOrDefault()?.Address;
        }

        private List<WSLInterfaceInfo> interfaces = new List<WSLInterfaceInfo>();

        public IReadOnlyList<WSLInterfaceInfo> Interfaces
        {
            get
            {
                if (interfaces.Count == 0)
                {
                    Refresh();
                }

                return new ReadOnlyCollection<WSLInterfaceInfo>(interfaces);
            }
        }

        public void Refresh()
        {
            interfaces = new List<WSLInterfaceInfo>(WSLInterfaceInfo.GetWSLInterfaces(Name));
        }
    }

    public class WSLInterfaceInfo
    {
        private WSLInterfaceInfo()
        {
        }

        public static WSLInterfaceInfo[] GetWSLInterfaces(string distribution = null)
        {
            var p = new Process();
            var wsl = Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\system32\\wsl.exe");

            string cmd = "ip -family inet address && ip -family inet6 address";

            if (distribution != null)
            {
                cmd = $"-d {distribution} {cmd}";
            }

            p.StartInfo = new ProcessStartInfo(wsl, cmd)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            p.Start();
            p.WaitForExit();

            var contents = p.StandardOutput.ReadToEnd().Replace("\x0", "");
            return ParseText(contents);
        }

        private static WSLInterfaceInfo[] ParseText(string contents)
        {
            List<WSLInterfaceInfo> list = new List<WSLInterfaceInfo>();

            var l = new List<string>(contents.Replace("\r\n", "\n").Split("\n"));
            int i, c = l.Count;

            for (i = c - 1; i >= 0; i--)
            {
                if (l[i].Trim() == "")
                {
                    l.RemoveAt(i);
                }
            }

            if (l.Count % 3 != 0) throw new InvalidOperationException("Bad Data.");

            c = l.Count;

            for (i = 0; i < c; i += 3)
            {
                var newinst = new WSLInterfaceInfo();

                var regparts = Regex.Match(l[i], @"(\d+): ([A-Za-z0-9_-]+): <(.+)> mtu (\d+)");
                if (regparts.Success)
                {
                    newinst.Id = int.Parse(regparts.Groups[1].Value);
                    newinst.Name = regparts.Groups[2].Value;

                    var parms = regparts.Groups[3].Value.Split(",");

                    newinst.IsBroadcast = parms.Contains("BROADCAST");
                    newinst.Up = parms.Contains("UP");
                    newinst.LowerUp = parms.Contains("LOWER_UP");
                    newinst.IsMulticast = parms.Contains("MULTICAST");
                    newinst.IsLoopback = parms.Contains("LOOPBACK");

                    newinst.MTU = int.Parse(regparts.Groups[4].Value);

                    var l2 = l[i + 1].Trim().Split(" ");
                    if (l2[0] == "inet" || l2[0] == "inet6")
                    {
                        var ip = l2[1].Split("/")[0];
                        newinst.Address = IPAddress.Parse(ip);
                    }
                }

                list.Add(newinst);
            }

            return list.ToArray();
        }

        public IPAddress Address { get; private set; }

        public bool IsBroadcast { get; private set; }

        public bool IsMulticast { get; private set; }

        public bool IsLoopback { get; private set; }

        public bool Up { get; private set; }
        public bool LowerUp { get; private set; }

        public int Id { get; private set; }

        public int MTU { get; private set; }

        public string Name { get; private set; }

        /* 1: lo: <LOOPBACK,UP,LOWER_UP> mtu 65536 qdisc noqueue state UNKNOWN group default qlen 1000
            inet 127.0.0.1/8 scope host lo
               valid_lft forever preferred_lft forever
           6: eth0: <BROADCAST,MULTICAST,UP,LOWER_UP> mtu 1500 qdisc mq state UP group default qlen 1000
            inet 172.20.182.177/20 brd 172.20.191.255 scope global eth0
               valid_lft forever preferred_lft forever

         */
    }
}