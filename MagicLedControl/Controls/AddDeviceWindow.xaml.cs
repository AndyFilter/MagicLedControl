using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MagicLedControl.Controls
{
    /// <summary>
    /// Interaction logic for AddDeviceWindow.xaml
    /// </summary>
    public partial class AddDeviceWindow : Window
    {
        private List<Ping> Pings = new();
        private string GatewayAddress = Utils.GetNetworkGatewayAddress();
        private int devicesFound = 0, controllersFound = 0;
        public AddDeviceWindow()
        {
            InitializeComponent();

            if (GatewayAddress == null)
                MessageBox.Show("There was an error! Make sure you are connected to a internet");
        }

        private void MouseTabDrag(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ExitClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DiscoverDevicesClicked(object sender, RoutedEventArgs e)
        {
            controllersFound = devicesFound = 0;
            controllersbox.Items.Clear();

            var addressBase = GatewayAddress.Remove(GatewayAddress.Length - 1);
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            for (int i = 0; i <= 255; i++)
            {
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    string address = $"{addressBase}{i}";
                    try
                    {
                        Trace.WriteLine($"Scanning addres {address}");
                        //var ping = new Ping();
                        //ping.PingCompleted += new PingCompletedEventHandler(OnPingCompleted);
                        //ping.SendAsync(address, 500, address);
                        var pingOut = await MagicUtils.FullPingDeviceAsync(address);
                        if (pingOut == MagicStructs.PingOutcome.Controller)
                        {
                            //Trace.WriteLine($"Adding: {address} message: {System.Text.Encoding.Default.GetString(e.Reply.Buffer)}");
                            devicesFound++;
                            devsFoundLab.Content = $"Devices found: {devicesFound}"; //Might add a Mac address to the name: easy and looks cool.
                            controllersFound++;
                            controllersFoundLab.Content = $"Controllers found: {controllersFound}";
                            var label = new Label();
                            label.Content = $"{address}";
                            label.DataContext = new MagicStructs.DeviceInfo($"Device{controllersFound}", address, pingOut);
                            if ((Application.Current.MainWindow as MainWindow).currentUserData.Devices.Any(d => d.Address == address))
                                label.Foreground = Resources["GradientStartButton"] as Brush;
                            controllersbox.Items.Add(label);
                        }
                        else if (pingOut == MagicStructs.PingOutcome.Device)
                        {
                            devicesFound++;
                            devsFoundLab.Content = $"Devices found: {devicesFound}";
                        }
                        //if(address.EndsWith("255"))
                        //{
                        //    stopwatch.Stop();
                        //    Trace.WriteLine($"Elapsed Discovery Time is {stopwatch.ElapsedMilliseconds} ms");
                        //}
                        //Pings.Add(ping);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Error while pinging {ex.Message} IP: {address}");
                        //Clipboard.SetText(ex.Message);
                        //MessageBox.Show(ex.Message);
                    }
                });

            }
        }

        private void ControllerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (controllersbox.SelectedItem == null) return;
            MagicStructs.DeviceInfo selectedContext = (MagicStructs.DeviceInfo)((controllersbox.SelectedItem as Label).DataContext);
            selectedDevNameBox.Text = selectedContext.Name;
            selectedDevIpBox.Text = selectedContext.Address;
        }

        private async void DeviceAddClicked(object sender, RoutedEventArgs e)
        {
            if (selectedDevIpBox.Text.Length > 0 && selectedDevNameBox.Text.Length > 0)
            {
                var device = new MagicStructs.DeviceInfo(selectedDevNameBox.Text, selectedDevIpBox.Text, await MagicUtils.FullPingDeviceAsync(selectedDevIpBox.Text));

                (Application.Current.MainWindow as MainWindow).SaveDevice(device);
            }
        }
    }
}
