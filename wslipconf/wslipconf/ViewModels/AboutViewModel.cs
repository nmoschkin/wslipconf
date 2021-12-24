using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WSLIPConf.ViewModels
{
    public class AboutViewModel : ObservableBase
    {
        public FileVersionInfo VersionInfo { get; }

        public string Version => VersionInfo?.FileVersion;

        public string ProductName => VersionInfo?.ProductName;

        public string Copyright => VersionInfo?.LegalCopyright;
        
        public string CompanyName => VersionInfo?.CompanyName;

        public AboutViewModel()
        {
            var info = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            VersionInfo = info;
        }

    }
}
