using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using WSLIPConf.Models;

namespace WSLIPConf.Helpers
{
    public static class NetshTool
    {

        public static List<WSLMapping> GetPortProxies()
        {

            // netsh interface portproxy show v4tov4

            var proc = new Process();
            var lOut = new List<WSLMapping>();

            proc.StartInfo = new ProcessStartInfo("netsh", "interface portproxy show v4tov4")
            {
                RedirectStandardOutput = true
            };

            proc.Start();
            proc.WaitForExit();

            var s = proc.StandardOutput.ReadToEnd();

            var lines = s.Replace("\r\n", "\n").Split("\n");

            int i, c = lines.Length;

            for (i = 0; i < c; i++)
            {
                if (lines[i].StartsWith("Address"))
                {
                    i += 2;
                    break;
                }

            }
            
            // Nothing here
            if (i >= c) return lOut;
            int d = 1;
            WSLMapping obj;

            for (; i < c; i++)
            {
                var line = lines[i].Trim();

                while (line.Contains("  "))
                {
                    line = line.Replace("  ", " ");
                }

                var split = line.Trim().Split(" ");
                if (split.Length != 4) continue;

                obj = new WSLMapping();

                obj.SourceAddress = IPAddress.Parse(split[0].Trim());
                obj.SourcePort = int.Parse(split[1].Trim());

                obj.DestinationAddress = IPAddress.Parse(split[2].Trim());
                obj.DestinationPort = int.Parse(split[3].Trim());

                obj.Name = "Rule #" + d++;
                lOut.Add(obj);


            }

            return lOut;
        }


    }
}
