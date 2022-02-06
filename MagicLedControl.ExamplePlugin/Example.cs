using MagicLedControl.PluginLib;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MagicLedControl.ExamplePlugin
{
    public class Example : IPlugin
    {
        public DeviceController deviceController;
        public bool isEnabled = true;
        public bool isSettingsWindowOpen = false;
        public Control.ExampleUserControl userSettings = null;
        public string Description
        {
            get
            {
                return "Set a Red or Green color of your leds with just ONE button press!";
            }
        }

        public string Name
        {
            get
            {
                return "Example Plugin";
            }
        }

        public void Begin(string parameters, DeviceController deviceController) //With the deviceController you can controll the leds however you want to
        {
            this.deviceController = deviceController; //DO NOT create a new instance of DeviceController, this will cause a lot of issues with the connectino to the led controller!
        }

        public void ChangeState(bool isEnabled)
        {
            this.isEnabled = isEnabled;
        }

        public void SettingsClicked()
        {
            Trace.WriteLine("Settings Opened");
            isSettingsWindowOpen = !isSettingsWindowOpen;
            if (userSettings == null || !userSettings.IsLoaded)
            {
                isSettingsWindowOpen = true;
                userSettings = new Control.ExampleUserControl(deviceController);
            }
            if (isSettingsWindowOpen)
            {
                userSettings.Show();
            }
            else
            {
                userSettings.Hide();
            }
        }
    }
}
