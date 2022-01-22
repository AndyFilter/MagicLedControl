using System;
using System.Collections.Generic;
using System.Text;
using MagicLedControl;

namespace MagicLedControl.PluginLib
{
    public interface IPlugin
    {
        string Name { get; }
        string Description { get; }
        void Begin(string parameters, DeviceController deviceController);
        void ChangeState(bool isEnabled);
        void SettingsClicked();
    }
}
