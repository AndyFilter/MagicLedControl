using System;
using System.Collections.Generic;
using System.IO;


namespace MagicLedControl
{
    public class Structs
    {
        public static DirectoryInfo UserDataPath = new DirectoryInfo(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MagicLedControl"));
        public static string UserDataFile = System.IO.Path.Combine(UserDataPath.FullName, "UserData.json");
        public class UserData
        {
            public List<MagicStructs.DeviceInfo> Devices { get; set; } = new List<MagicStructs.DeviceInfo>();
            public Dictionary<string, SimpleColor> SavedColor { get; set; } = new Dictionary<string, SimpleColor>();
        }

        public class SimpleColor
        {
            public int R { get; set; }
            public int G { get; set; }
            public int B { get; set; }
            public int A { get; set; }

            public SimpleColor(int r, int g, int b, int a)
            {
                R = r; G = g; B = b; A = a;
            }
        }
    }
}
