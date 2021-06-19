using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WSLIPConf.Helpers
{
    public static class WSLTool
    {

        public static IPAddress GetWslIpAddress()
        {
            var p = new Process();

            p.StartInfo = new ProcessStartInfo("wsl", "/usr/sbin/ifconfig")
            {
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };


            p.Start();
            p.WaitForExit();

            var contents = p.StandardOutput.ReadToEnd();
            var l = contents.Replace("\r\n", "\n").Split("\n");
            var lt = l[1].Trim();
            var parts = lt.Split(" ");
            return IPAddress.Parse(parts[1]);
        }


    }
}
