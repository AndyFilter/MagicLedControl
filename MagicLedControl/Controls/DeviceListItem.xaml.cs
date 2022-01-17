using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MagicLedControl.Controls
{
    /// <summary>
    /// Interaction logic for DeviceListItem.xaml
    /// </summary>
    public partial class DeviceListItem : UserControl
    {
        private MagicStructs.DeviceInfo _deviceInfo;
        public MagicStructs.DeviceInfo deviceInfo { 
            get { return _deviceInfo; }
            set {
                _deviceInfo = value;
                deviceName.Content = value.Name;
                UpdateDeviceLabelColor(deviceName, value.PingOutcome);
            } }
        public DeviceListItem(MagicStructs.DeviceInfo device)
        {
            InitializeComponent();

            deviceInfo = device;
        }

        private void UpdateDeviceLabelColor(Label label, MagicStructs.PingOutcome ping)
        {
            if (ping == MagicStructs.PingOutcome.NoResponse)
                label.Foreground = (Brush)(FindResource("DeleteButtonColor"));
            if (ping == MagicStructs.PingOutcome.Device)
                label.Foreground = (Brush)(FindResource("FontColor"));
            if (ping == MagicStructs.PingOutcome.Controller)
                label.Foreground = (Brush)(FindResource("SecondaryLight"));
        }

        public void UpdateDeviceLabelColor()
        {
            var label = deviceName;
            var ping = deviceInfo.PingOutcome;
            if (ping == MagicStructs.PingOutcome.NoResponse)
                label.Foreground = (Brush)(FindResource("DeleteButtonColor"));
            if (ping == MagicStructs.PingOutcome.Device)
                label.Foreground = (Brush)(FindResource("FontColor"));
            if (ping == MagicStructs.PingOutcome.Controller)
                label.Foreground = (Brush)(FindResource("SecondaryLight"));
        }
    }
}
