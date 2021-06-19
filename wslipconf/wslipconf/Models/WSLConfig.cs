using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WSLIPConf.ViewModels;

namespace WSLIPConf.Models
{
    public class WSLConfig : ObservableBase
    {
        private string filename = GetConfigFile();
        private DateTime lastUpdated = DateTime.Now;
        private bool runAtStartup = true;

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

            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            JsonConvert.PopulateObject(json, this);

            foreach (var rule in this.Mappings)
            {
                if (rule.AutoDestination)
                {
                    rule.DestinationAddress = App.Current.WSLAddress;
                    rule.Changed = false;
                }
            }
        }

        public static WSLConfig Load()
        {
            var path = GetConfigFile();

            var obj = new WSLConfig();

            if (!File.Exists(path)) return obj;
            
            var json = File.ReadAllText(path);

            JsonConvert.PopulateObject(json, obj);

            foreach (var rule in obj.Mappings)
            {
                if (rule.AutoDestination)
                {
                    rule.DestinationAddress = App.Current.WSLAddress;
                    rule.Changed = false;
                }
            }

            return obj;
        }

        public static string GetConfigFile()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\config.json";

        }

    }
}
