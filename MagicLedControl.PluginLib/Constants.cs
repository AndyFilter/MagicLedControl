using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLedControl.PluginLib
{
    public static class Constants
    {
        //The folder name which contains the plugin DLLs
        public const string FolderName = "Plugins";
        public static DirectoryInfo UserDataPath = new DirectoryInfo(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MagicLedControl"));
        public static string UserDataFile = System.IO.Path.Combine(UserDataPath.FullName, "UserData.json");
    }
}
