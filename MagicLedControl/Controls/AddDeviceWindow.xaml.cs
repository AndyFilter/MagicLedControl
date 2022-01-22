using MagicLedControl.PluginLib;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MagicLedControl.Controls
{
    /// <summary>
    /// Interaction logic for AddDeviceWindow.xaml
    /// </summary>
    public partial class AddDeviceWindow : Window
    {
        private static string GatewayAddress = Utils.GetNetworkGatewayAddress();
        private string addressBase = GatewayAddress.Remove(GatewayAddress.Length - 1);
        private int devicesFound = 0, controllersFound = 0, devicesScanned = 0;
        private bool isScanning = false;
        public AddDeviceWindow()
        {
            InitializeComponent();

            topTabGrid.MouseLeftButtonDown += MouseTabDrag;

            if (GatewayAddress == null)
                MessageBox.Show("There was an error! Make sure you are connected to the internet");
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
            if (isScanning || GatewayAddress.Length < 1) return;
            isScanning = true;
            controllersFound = devicesFound = devicesScanned = 0;
            devsFoundLab.Content = $"Devices found: {devicesFound}";
            controllersFoundLab.Content = $"Controllers found: {controllersFound}";
            controllersbox.Items.Clear();
            DoubleAnimation animation = new DoubleAnimation((devicesScanned / 255d), TimeSpan.FromMilliseconds(20));
            ipScannedProgressSeparator.BeginAnimation(ProgressBar.ValueProperty, animation);
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Discover();
            }).Start();
        }

        private async void Discover()
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            Task[] tasks = new Task[256];
            for (int i = 0; i <= 255; i++) //The async here is really bad, I had some other ideas that worked a bit better, but they looked much worse
            {
                tasks[i] = ScanIp(i);
            }
            await Task.WhenAll(tasks);
            isScanning = false;
            //stopwatch.Stop();
            //Trace.WriteLine($"Elapsed Discovery Time is {stopwatch.ElapsedMilliseconds} ms");
        }

        private async Task ScanIp(int i)
        {
            string address = $"{addressBase}{i}";
            try
            {
                Trace.WriteLine($"Scanning address {address}");
                //var ping = new Ping();
                //ping.PingCompleted += new PingCompletedEventHandler(OnPingCompleted);
                //ping.SendAsync(address, 500, address);
                var pingOut = await MagicUtils.FullPingDeviceAsync(address);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    devicesScanned++;
                    if (pingOut.Item1 == MagicStructs.PingOutcome.Controller)
                    {
                        //Trace.WriteLine($"Adding: {address} message: {System.Text.Encoding.Default.GetString(e.Reply.Buffer)}");
                        devicesFound++;
                        devsFoundLab.Content = $"Devices found: {devicesFound}";
                        controllersFound++;
                        controllersFoundLab.Content = $"Controllers found: {controllersFound}";
                        var label = new Label();
                        label.Content = $"{address}";
                        label.DataContext = new MagicStructs.DeviceInfo($"Device ({pingOut.Item2})", address, pingOut.Item1, pingOut.Item2);
                        if ((Application.Current.MainWindow as MainWindow).currentUserData.Devices.Any(d => d.Address == address))
                            label.Foreground = Resources["GradientStartButton"] as Brush;
                        controllersbox.Items.Add(label);
                    }
                    else if (pingOut.Item1 == MagicStructs.PingOutcome.Device)
                    {
                        devicesFound++;
                        devsFoundLab.Content = $"Devices found: {devicesFound}";
                    }
                    //if (address.EndsWith("255"))
                    //{
                    //    stopwatch.Stop();
                    //    Trace.WriteLine($"Elapsed Discovery Time is {stopwatch.ElapsedMilliseconds} ms");
                    //}
                    //Pings.Add(ping);
                    DoubleAnimation animation = new DoubleAnimation((devicesScanned / 255d), TimeSpan.FromMilliseconds(200));
                    ipScannedProgressSeparator.BeginAnimation(ProgressBar.ValueProperty, animation);
                    //progressScale.ScaleX = (devicesScanned / 255d);//Math.Clamp(devicesFound / ((255d - devicesScanned) + devicesFound), 0, 1);
                });
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error while pinging {ex.Message} IP: {address}");
                //Clipboard.SetText(ex.Message);
                //MessageBox.Show(ex.Message);
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
                Trace.WriteLine("Device added");
                var fullPing = await MagicUtils.FullPingDeviceAsync(selectedDevIpBox.Text);
                var device = new MagicStructs.DeviceInfo(selectedDevNameBox.Text, selectedDevIpBox.Text, fullPing.Item1, fullPing.Item2);
                (Application.Current.MainWindow as MainWindow).SaveDevice(device);
            }
        }
    }
}
