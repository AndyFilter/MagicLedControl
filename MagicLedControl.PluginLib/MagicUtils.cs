using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MagicLedControl.PluginLib
{
    public class MagicUtils
    {
        public static byte CalculateChecksum(byte[] bytes) //Thanks to iUltimateLP for this function and the Controller Emulator that he made! https://github.com/iUltimateLP/MagicHomeController
        {
            // The checksum algorithm works as follows:
            // All bytes are summed together
            int sum = 0;
            foreach (byte b in bytes)
            {
                sum += b;
            }

            // Then the resulting number gets converted to a hex string
            string str = sum.ToString("X");

            // Of that string, the last two digits are taken
            str = str.Substring(str.Length - 2, 2);

            // And directly converted into a byte (without encoding)
            return Convert.ToByte(str, 16);
        }

        public static Color ApplyBrightnessToColor(Color color)
        {
            var brightness = color.A;
            var newColor = color;
            newColor = Color.Multiply(newColor, brightness / 255f); //multiplies Color by the Alpha channel, this is just how the how the controller works.
            return newColor;
        }

        public static async Task<(MagicStructs.PingOutcome, string)> FullPingDeviceAsync(string address, int timeOut = 1000)
        {
            //(MagicStructs.PingOutcome, string) result = await Task.Run(async () =>
            //{
            (bool, string) controllerPing;
            var ping = new Ping();
            //ping.PingCompleted += new PingCompletedEventHandler(OnPingCompleted);
            var reply = await ping.SendPingAsync(address, timeOut);
            if (reply != null && reply.Status == IPStatus.Success)
            {
                controllerPing = DeviceController.PingDevice(address).GetAwaiter().GetResult();
            }
            else
                return (MagicStructs.PingOutcome.NoResponse, "");

            return ((controllerPing.Item1 ? MagicStructs.PingOutcome.Controller : MagicStructs.PingOutcome.Device), controllerPing.Item2);
            //});
            //return result;
        }
    }
}
