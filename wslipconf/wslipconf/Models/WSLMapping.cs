
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using WSLIPConf.Converters;
using WSLIPConf.Helpers;
using WSLIPConf.Localization;
using WSLIPConf.ViewModels;

namespace WSLIPConf.Models
{
    /// <summary>
    /// Network Mapping
    /// </summary>
    public class WSLMapping : ObservableBase, ICloneable, IValidatableObject
    {
        private string name;

        private IPAddress srcAddr = IPAddress.Parse("0.0.0.0");
        private IPAddress destAddr = IPAddress.Parse("0.0.0.0");

        private int srcPort;
        private int destPort;

        private bool autoDest = true;
        private ProxyProtocol protocol = ProxyProtocol.Tcp;

        private bool changed;

        [JsonProperty("autoDest")]
        public bool AutoDestination
        {
            get => autoDest;
            set
            {
                if (SetProperty(ref autoDest, value))
                {
                    OnPropertyChanged(nameof(DestinationAddress));
                    Changed = true;
                }
            }
        }

        [JsonProperty("name")]
        public string Name
        {
            get => name;
            set
            {
                if (SetProperty(ref name, value)) Changed = true;
            }
        }

        [JsonProperty("protocol")]
        public ProxyProtocol Protocol
        {
            get => protocol;
            set
            {
                SetProperty(ref protocol, value);
            }
        }

        [JsonProperty("srcAddr")]
        [JsonConverter(typeof(IPAddressConverter))]
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
        [JsonConverter(typeof(IPAddressConverter))]
        public IPAddress DestinationAddress
        {
            get
            {
                if (!autoDest) return destAddr;
                else return App.Current.WSLAddress;
            }
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
                return (srcAddr == other.srcAddr) && (srcPort == other.srcPort) && ((autoDest == other.autoDest && autoDest == true) || (destAddr == other.destAddr)) && (destPort == other.destPort);
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var res = new List<ValidationResult>();

            if (validationContext.MemberName == nameof(Name))
            {
                if (string.IsNullOrEmpty(Name))
                {
                    res.Add(new ValidationResult(AppResources.ErrorFieldBlank, new string[] { nameof(Name) }));
                }
            }
            else if (validationContext.MemberName == nameof(SourcePort))
            {
                if (SourcePort <= 0)
                {
                    res.Add(new ValidationResult(AppResources.ErrorInvalidValue, new string[] { nameof(SourcePort) }));

                }
            }
            else if (validationContext.MemberName == nameof(SourceAddress))
            {
                if (SourceAddress == null)
                {
                    res.Add(new ValidationResult(AppResources.ErrorInvalidValue, new string[] { nameof(SourceAddress) }));

                }
            }
            else if (validationContext.MemberName == nameof(DestinationPort))
            {
                if (DestinationPort <= 0)
                {
                    res.Add(new ValidationResult(AppResources.ErrorInvalidValue, new string[] { nameof(DestinationPort) }));
                }
            }
            else if (validationContext.MemberName == nameof(DestinationAddress))
            {
                if (DestinationAddress == null)
                {
                    res.Add(new ValidationResult(AppResources.ErrorInvalidValue, new string[] { nameof(DestinationAddress) }));
                }
            }


            return res;
        }
    }

}
