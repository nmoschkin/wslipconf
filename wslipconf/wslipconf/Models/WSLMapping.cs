using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
        private ProxyType proxyType = ProxyType.V4ToV4;
        private string distroName = null;
        private WSLDistribution distro;

        public WSLMapping()
        {
            distro = App.Current.SessionDefault;
        }

        private void ClearAddresses(bool src, bool dest)
        {
            if (src)
            {
                if ((proxyType & ProxyType.SourceV4) == ProxyType.SourceV4)
                {
                    SourceAddress = IPAddress.Parse("0.0.0.0");
                }
                else
                {
                    SourceAddress = IPAddress.Parse("0000:0000:0000:0000:0000:0000:0000:0000");
                }
            }
            if (dest)
            {
                if (AutoDestination)
                {
                    OnPropertyChanged(nameof(DestinationAddress));
                }
                else
                {
                    if ((proxyType & ProxyType.DestV4) == ProxyType.DestV4)
                    {
                        DestinationAddress = IPAddress.Parse("0.0.0.0");
                    }
                    else
                    {
                        DestinationAddress = IPAddress.Parse("0000:0000:0000:0000:0000:0000:0000:0000");
                    }
                }
            }
        }

        [JsonProperty("proxyType")]
        public ProxyType ProxyType
        {
            get
            {
                return proxyType;
            }
            set
            {
                if (value == 0) value = ProxyType.V4ToV4;

                var oldproxy = proxyType;
                if (SetProperty(ref proxyType, value))
                {
                    bool s = false,
                         d = false;

                    if (((int)oldproxy & 0x30) != ((int)proxyType & 0x30))
                    {
                        s = true;
                    }
                    if (((int)oldproxy & 0x3) != ((int)proxyType & 0x3))
                    {
                        d = true;
                    }

                    ClearAddresses(s, d);
                }
            }
        }

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

        [JsonIgnore]
        public bool UseDefaultDistro => distroName == null;

        [JsonIgnore]
        public WSLDistribution DistroInfo => distro;

        [JsonProperty("distro")]
        public string Distribution
        {
            get => distroName;
            set
            {
                if (SetProperty(ref distroName, value))
                {
                    TriggerUpdate();
                }
            }
        }

        public void TriggerUpdate()
        {
            distro = null;

            if (distroName != null)
            {
                distro = WSLDistribution.FindByName(distroName);
            }
            else
            {
                distro = App.Current.SessionDefault;
            }

            if (distro == null)
            {
                distro = App.Current.SessionDefault;
                distroName = distro.Name;
            }

            OnPropertyChanged(nameof(Distribution));
            OnPropertyChanged(nameof(UseDefaultDistro));
            OnPropertyChanged(nameof(DistroInfo));
            OnPropertyChanged(nameof(WorkingDistroName));

            if (AutoDestination)
            {
                OnPropertyChanged(nameof(DestinationAddress));
            }
        }

        [JsonIgnore]
        public string WorkingDistroName
        {
            get
            {
                if (distroName != null) return distroName;
                return $"{App.Current.SessionDefault.Name} ({AppResources.Default})";
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
                if (SetProperty(ref srcAddr, value))
                {
                    if ((proxyType & ProxyType.SourceV4) == ProxyType.SourceV4
                        && srcAddr.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        ProxyType = (ProxyType)((int)proxyType & 3) | ProxyType.SourceV6;
                    }
                    else if ((proxyType & ProxyType.SourceV6) == ProxyType.SourceV6
                        && srcAddr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ProxyType = (ProxyType)((int)proxyType & 3) | ProxyType.SourceV4;
                    }

                    Changed = true;
                }
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
                else
                {
                    if ((proxyType & ProxyType.DestV4) == ProxyType.DestV4)
                    {
                        return distro.GetWslIpAddress();
                    }
                    else
                    {
                        return distro.GetWslIpV6Address();
                    }
                }
            }
            set
            {
                if (SetProperty(ref destAddr, value))
                {
                    if ((proxyType & ProxyType.DestV4) == ProxyType.DestV4
                        && destAddr.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        ProxyType = (ProxyType)((int)proxyType & 30) | ProxyType.DestV6;
                    }
                    else if ((proxyType & ProxyType.DestV6) == ProxyType.DestV6
                        && destAddr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ProxyType = (ProxyType)((int)proxyType & 30) | ProxyType.DestV4;
                    }

                    Changed = true;
                }
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
                return (srcAddr == other.srcAddr) &&
                    (srcPort == other.srcPort) &&
                    ((autoDest == other.autoDest && autoDest == true) || (destAddr == other.destAddr)) &&
                    (proxyType == other.proxyType) &&
                    (destPort == other.destPort);
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