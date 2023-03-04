using MagicLedControl.PluginLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Windows.Media;

namespace MagicLedControl
{
    public static class Utils
    {
        public static Structs.UserData? GetUserData()
        {
            try
            {
                if (!Directory.Exists(Constants.UserDataPath.FullName))
                {
                    Directory.CreateDirectory(Constants.UserDataPath.FullName);
                }
                if (!File.Exists(Constants.UserDataFile))
                {
                    File.Create(Constants.UserDataFile);
                }
                var fileText = File.ReadAllText(Constants.UserDataFile, Encoding.UTF8);
                var data = JsonSerializer.Deserialize<Structs.UserData>(fileText);
                data.Devices.ForEach(d => d.PingOutcome = 0);
                return data;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception On Read: " + ex.Message);
                return null;
            }
            return null;
        }

        public static void SaveUserData(Structs.UserData userData)
        {
            Trace.WriteLine("Saving Data");
            try
            {
                if (!Directory.Exists(Constants.UserDataPath.FullName))
                {
                    Directory.CreateDirectory(Constants.UserDataPath.FullName);
                }
                if (!File.Exists(Constants.UserDataFile))
                {
                    File.Create(Constants.UserDataFile);
                }

                Trace.WriteLine("Deserialized user data: " + JsonSerializer.Serialize(userData));
                File.WriteAllText(Constants.UserDataFile, JsonSerializer.Serialize(userData));
                Trace.WriteLine("Data Saved");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception: " + ex.Message);
                return;
            }
        }

        public static Structs.SimpleColor ColorToSimpleColor(Color color)
        {
            return new Structs.SimpleColor(color.R, color.G, color.B, color.A);
        }

        public static Color SimpleColorToColor(Structs.SimpleColor color)
        {
            return Color.FromArgb(Convert.ToByte(color.A), Convert.ToByte(color.R), Convert.ToByte(color.G), Convert.ToByte(color.B));
        }

        public static Color ColorFromNotifColor(ColorPicker.Models.NotifyableColor color)
        {
            Color cleanColor;
            cleanColor.A = Convert.ToByte(Math.Floor(color.A));    //The data in NotifyableColor is stored as double, thus it need a conversion to int before going to byte.
            cleanColor.R = Convert.ToByte(Math.Floor(color.RGB_R));
            cleanColor.G = Convert.ToByte(Math.Floor(color.RGB_G));
            cleanColor.B = Convert.ToByte(Math.Floor(color.RGB_B));
            return cleanColor;
        }

        public static string GetNetworkGatewayAddress()
        {
            string ip = null;

            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (f.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
                    {
                        ip = d.Address.ToString();
                    }
                }
            }

            return ip;
        }
    }
}
