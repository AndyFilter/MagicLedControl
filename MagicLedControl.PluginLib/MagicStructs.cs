using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace MagicLedControl.PluginLib
{
    public class MagicStructs
    {
        public class Controller
        {
            public Color currentColor;//Changes when selected to function mode
            public byte modelVersion;
            public bool powerState;
            public byte functionMode;
            public int functionSpeed;
            public byte firmwareVersion;
        }

        public enum FunctionType
        {
            Gradual,//Gradual
            Instant,//Jumping
            Strobe,//Strobe
        }

        public class Function
        {
            public List<Color> colors = new List<Color>(16);
            public FunctionType type;
            public int speed;
        }

        public class DeviceInfo
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public PingOutcome PingOutcome { get; set; }
            public string MacAddress { get; set; }

            [JsonIgnore]
            public Controller? lastConfiguration { get; set; }

            public DeviceInfo(string name, string address, PingOutcome pingOutcome, string macAddress)
            {
                Name = name;
                Address = address;
                PingOutcome = pingOutcome;
                MacAddress = macAddress;
            }

            public DeviceInfo()
            {
                Name = "Unknown";
                Address = "0.0.0.0";
                MacAddress = "00000";
                PingOutcome = 0;
            }
        }

        public enum PingOutcome
        {
            NoResponse,
            Device,
            Controller
        }
    }
}
