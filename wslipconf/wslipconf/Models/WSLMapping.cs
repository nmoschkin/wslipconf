using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using WSLIPConf.ViewModels;

namespace WSLIPConf.Models
{
    /// <summary>
    /// Network Mapping
    /// </summary>
    public class WSLMapping : ObservableBase, ICloneable
    {
        private string name;

        private IPAddress srcAddr;
        private IPAddress destAddr;

        private int srcPort;
        private int destPort;

        private bool changed;

        [JsonProperty("name")]
        public string Name
        {
            get => name;
            set
            {
                if (SetProperty(ref name, value)) Changed = true;
            }
        }

        [JsonProperty("srcAddr")]
        public IPAddress SourceAddress
        {
            get => srcAddr;
            set
            {
                if (SetProperty(ref srcAddr, value)) Changed = true;
            }
        }

        [JsonProperty("srcPort")]
        public int SourcePort
        {
            get => srcPort;
            set
            {
                if (SetProperty(ref srcPort, value)) Changed = true;
            }
        }

        [JsonProperty("destAddr")]
        public IPAddress DestinationAddress
        {
            get => destAddr;
            set
            {
                if (SetProperty(ref destAddr, value)) Changed = true;
            }
        }

        [JsonProperty("destPort")]
        public int DestinationPort
        {
            get => destPort;
            set
            {
                if (SetProperty(ref destPort, value)) Changed = true;
            }
        }

        [JsonIgnore]
        public bool Changed
        {
            get => changed;
            set
            {
                SetProperty(ref changed, value);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is WSLMapping other)
            {
                return (srcAddr == other.srcAddr) && (srcPort == other.srcPort) && (destAddr == other.destAddr) && (destPort == other.destPort);
            }
            else
            {
                return false;
            }

        }

        public override int GetHashCode()
        {
            int hc = 0;

            List<byte> bb = new List<byte>();
            
            bb.AddRange(srcAddr.GetAddressBytes());
            bb.AddRange(destAddr.GetAddressBytes());
            bb.AddRange(BitConverter.GetBytes(srcPort));
            bb.AddRange(BitConverter.GetBytes(destPort));

            foreach (byte b in bb)
            {
                hc = (hc << 3) | b;
            }

            return hc;
        }

        public override string ToString()
        {
            return $"{srcAddr}:{srcPort} => {destAddr}:{destPort}";
        }

        object ICloneable.Clone() => MemberwiseClone();

        public WSLMapping Clone() => (WSLMapping)MemberwiseClone();

    }

}
