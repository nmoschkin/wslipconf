using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Printing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WSLIPConf.Helpers
{
    public static class WSLTool
    {

        public static IPAddress GetWslIpAddress()
        {
            return WSLInterfaceInfo.GetFirstMulticastAddress().Address;
        }

    }

    public class WSLInterfaceInfo
    {
        private static List<WSLInterfaceInfo> interfaces = new List<WSLInterfaceInfo>();
        
        public static IReadOnlyList<WSLInterfaceInfo> Interfaces 
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

        private WSLInterfaceInfo()
        {
        }

        public static void Refresh()
        {
            interfaces = new List<WSLInterfaceInfo>(GetWSLInterfaces());
        }

        public static WSLInterfaceInfo GetFirstMulticastAddress(bool forceRefresh = true)
        {
            if (forceRefresh) Refresh();

            foreach (var iface in Interfaces)
            {
                if (iface.IsMulticast) return iface;
            }

            return null;
        }

        public static WSLInterfaceInfo[] GetWSLInterfaces()
        {
            var p = new Process();
            var wsl = Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\system32\\wsl.exe");

            p.StartInfo = new ProcessStartInfo(wsl, "ip -family inet address")
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
                    if (l2[0] == "inet")
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
