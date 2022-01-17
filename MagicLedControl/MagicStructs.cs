using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace MagicLedControl
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

            public DeviceInfo(string name, string address, PingOutcome pingOutcome)
            {
                Name = name;
                Address = address;
                PingOutcome = pingOutcome;
            }

            public static DeviceInfo Clone(DeviceInfo deviceInfo)
            {
                var dev = new DeviceInfo(deviceInfo.Name, deviceInfo.Address, deviceInfo.PingOutcome);
                return dev;
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
