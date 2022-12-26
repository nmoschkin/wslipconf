using Newtonsoft.Json;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

using WSLIPConf.Helpers;
using WSLIPConf.ViewModels;

namespace WSLIPConf.Models
{
    public class WSLConfig : ObservableBase
    {
        private string filename = GetConfigFile();
        private DateTime lastUpdated = DateTime.Now;
        private bool runAtStartup = true;
        private string prefdist = null;

        private ObservableCollection<WSLMapping> mappings = new ObservableCollection<WSLMapping>();

        [JsonIgnore]
        public string Filename
        {
            get => filename;
            set
            {
                SetProperty(ref filename, value);
            }
        }

        [JsonProperty("preferredDistribution")]
        public string PreferredDistribution
        {
            get => prefdist;
            set
            {
                SetProperty(ref prefdist, value);
            }
        }

        [JsonProperty("mappings")]
        public ObservableCollection<WSLMapping> Mappings
        {
            get => mappings;
            set
            {
                SetProperty(ref mappings, value);
            }
        }

        [JsonProperty("lastUpdated")]
        public DateTime LastUpdated
        {
            get => lastUpdated;
            set
            {
                SetProperty(ref lastUpdated, value);
            }
        }

        [JsonProperty("runAtStartup")]
        public bool RunAtStartup
        {
            get => runAtStartup;
            set
            {
                SetProperty(ref runAtStartup, value);
            }
        }

        public void SaveToDisk()
        {
            var json = JsonConvert.SerializeObject(this);
            var path = GetConfigFile();

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllText(path, json);
        }

        public void ReadFromDisk()
        {
            var path = GetConfigFile();
            mappings.Clear();

            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            JsonConvert.PopulateObject(json, this);

            var dist = WSLDistribution.FindByName(prefdist) ?? WSLDistribution.SessionDefault ?? WSLDistribution.SystemDefault;

            WSLDistribution.SessionDefault = dist;

            foreach (var rule in this.Mappings)
            {
                if (rule.AutoDestination)
                {
                    if ((rule.ProxyType & Helpers.ProxyType.DestV4) == Helpers.ProxyType.DestV4)
                    {
                        rule.DestinationAddress = dist.GetWslIpAddress();
                    }
                    else
                    {
                        rule.DestinationAddress = dist.GetWslIpV6Address();
                    }
                    rule.Changed = false;
                }
            }
        }

        public static WSLConfig Load()
        {
            var obj = new WSLConfig();
            obj.ReadFromDisk();

            return obj;
        }

        public static string GetConfigFile()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\config.json";
        }
    }
}