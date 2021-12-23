using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            public string Name { get; set; } = "Led Controller";
            public string Address { get; set; } = "0.0.0.0";

            public DeviceInfo(string name, string address)
            {
                Name = name;
                Address = address;
            }
        }
    }
}
