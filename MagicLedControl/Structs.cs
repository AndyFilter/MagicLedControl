using MagicLedControl.PluginLib;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MagicLedControl
{
    public class Structs
    {
        public class UserData
        {
            public List<MagicStructs.DeviceInfo> Devices { get; set; } = new List<MagicStructs.DeviceInfo>();
            public Dictionary<string, SimpleColor> SavedColors { get; set; } = new Dictionary<string, SimpleColor>();
            public List<PluginInfo> Plugins { get; set; } = new List<PluginInfo>();
        }

        public class PluginInfo
        {
            public string Name { get; set; }
            public bool IsEnabled { get; set; }
            [JsonIgnore]
            public string? Description { get; set; }
            [JsonIgnore]
            public IPlugin? Plugin { get; set; }

            public PluginInfo(string name, string description, bool isEnabled)
            {
                Name = name;
                Description = description;
                IsEnabled = isEnabled;
            }

            public PluginInfo(string name, bool isEnabled)
            {
                Name = name;
                IsEnabled = isEnabled;
            }

            public PluginInfo()
            {
                Name = "";
                IsEnabled = false;
            }
        }

        public class SimpleColor
        {
            public int R { get; set; } = 0;
            public int G { get; set; } = 0;
            public int B { get; set; } = 0;
            public int A { get; set; } = 0;

            public SimpleColor(int r, int g, int b, int a)
            {
                R = r; G = g; B = b; A = a;
            }

            public SimpleColor()
            {
                R = 0;
                G = 0;
                B = 0;
                A = 0;
            }
        }
    }
}
