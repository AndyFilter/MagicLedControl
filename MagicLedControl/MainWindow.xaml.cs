using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MagicLedControl
{
    public partial class MainWindow : Window
    {
        public DeviceController deviceController = new DeviceController();
        private string selectedDeviceIp = "";
        private Color lastSelectedColor;
        private bool wasInitialized = false;
        private Timer? UpdateDataMethod;//Not really used, and I dont really see a use for it either, but better to have it than sorry... right?
        private bool isDisco = false, isCustomFunction = false;
        public Structs.UserData currentUserData = new Structs.UserData();
        private Controls.AddDeviceWindow? newDeviceWindow;
        public long lastSuccessfulMessageTime = 0;

        public MainWindow()
        {
            InitializeComponent();
            var _tempUserData = Utils.GetUserData();
            if (_tempUserData != null)
            {
                currentUserData = _tempUserData;
                if (currentUserData.Devices.Count <= 0 || currentUserData.Devices[0] == null)
                {
                    //No devices found... Do something funny.
                }
                else
                {
                    selectedDeviceIp = currentUserData.Devices[0].Address;
                    foreach (var device in currentUserData.Devices)
                    {
                        var devItem = new Controls.DeviceListItem(device);

                        devicesBox.Items.Add(devItem);
                    }
                    devicesBox.SelectedIndex = 0;
                    PingAllDevices();
                }
                if (currentUserData.SavedColor.Count > 0)
                {
                    ReloadSavedColorsBox();
                }
            }
            InitializeDevice();
        }

        private async void PingAllDevices()
        {
            for (int i = 0; i < devicesBox.Items.Count; i++)
            {
                var device = devicesBox.Items.GetItemAt(i) as Controls.DeviceListItem;
                var ping = await MagicUtils.FullPingDeviceAsync(device.deviceInfo.Address);
                device.deviceInfo.PingOutcome = ping.Item1;
                device.UpdateDeviceLabelColor();
            }
        }

        private void ReloadSavedColorsBox()
        {
            colorSelectBox.Items.Clear();
            colorSelectBox.ItemsSource = null;
            //Trace.WriteLine("Children in CB: " + colorSelectBox.Items.Count);
            foreach (var color in currentUserData.SavedColor)
            {
                var label = new Label();
                label.Content = color.Key.ToString();
                label.DataContext = color;
                var useColor = Utils.SimpleColorToColor(color.Value);
                useColor.A = 255;
                useColor = Color.Add(useColor, Color.FromRgb(50, 50, 50));
                useColor = Color.Multiply(useColor, 1.3f);
                label.Foreground = new SolidColorBrush(useColor);
                //var cleanColor = new KeyValuePair<string, Brush>(color.Key, new SolidColorBrush(useColor));
                colorSelectBox.Items.Add(label);
            }
        }

        private void PowerOnChecked(object sender, RoutedEventArgs e)
        {
            //Trace.WriteLine("Led ON checked");
            if (!wasInitialized) return;
            SetLedPowerState(true);
        }

        private void PowerOffChecked(object sender, RoutedEventArgs e)
        {
            //Trace.WriteLine("Led OFF checked");
            if (!wasInitialized) return;
            SetLedPowerState(false);
        }

        public async void SetLedPowerState(bool newState)
        {
            Trace.WriteLine("the leds are now: " + (newState ? "On" : "Off"));
            try
            {
                await deviceController.SetPowerState(newState);

                //await Task.Delay(500);
                //if (ct.CanBeCanceled)
                //    ct.WaitHandle.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void OnColorChanged(object sender, RoutedEventArgs e)
        {
            var cp = (sender as ColorPicker.StandardColorPicker);
            if (cp == null)
                return;
            var currentColor = Utils.ColorFromNotifColor(cp.Color);
            currentColor = MagicUtils.ApplyBrightnessToColor(currentColor);//This is trhe way the device sends and receives colors, just multiplies them by brightness (in this case by Alpha);
            if (lastSelectedColor != currentColor)
            {
                Trace.WriteLine("Color changed to:" + currentColor.ToString());
                lastSelectedColor = currentColor;
                deviceController.SetColor(currentColor);
                isDisco = false;
                isCustomFunction = false;
                DiscoButton.Style = Resources["NormalButton"] as Style; //This code might be a bit higher, but it doesnt really matter, and I'm already high enough;
            }
        }

        protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (deviceController != null)
            {
                deviceController.Disconnect();
            }
            Application.Current.Shutdown();
        }

        private async void DebugButtonClicked(object sender, RoutedEventArgs e)
        {
            var data = await deviceController.RequestControllerData();

            Trace.WriteLine("Current Color:" + data?.currentColor.ToString());

            SetControllerData();
        }

        private async void InitializeDevice()
        {
            if (selectedDeviceIp.Length < 1) return;
            await deviceController.Connect(selectedDeviceIp);
            //SetControllerData();
            UpdateDataMethod = new System.Threading.Timer((e) =>
            {
                Trace.WriteLine("Retrying connection");
                SetControllerData();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            await Task.Delay(1000);
        }

        private async void SetControllerData()
        {
            //Trace.WriteLine($"Message info lsm: {lastSuccessfulMessageTime}  time now: {DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            if (lastSuccessfulMessageTime + 400 > DateTimeOffset.Now.ToUnixTimeMilliseconds())
            {
                Trace.WriteLine("Recent message");
                return;
            }

            var data = await deviceController.RequestControllerData();

            //var colorBackup = lastSelectedColor;

            if (data == null)
            {
                Application.Current.Dispatcher.Invoke(new Action(async () =>
                {
                    if (devicesBox.SelectedItem != null && (devicesBox.SelectedItem as Controls.DeviceListItem) != null)
                    {
                        var device = (devicesBox.SelectedItem as Controls.DeviceListItem);
                        var ping = await MagicUtils.FullPingDeviceAsync(device.deviceInfo.Address);
                        device.deviceInfo.PingOutcome = ping.Item1;
                        device.UpdateDeviceLabelColor();
                    }
                    ConnectionStateLabel.Content = "Device Disconnected";
                }
                ));
                return;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (devicesBox.SelectedItem != null && (devicesBox.SelectedItem as Controls.DeviceListItem) != null)
                    {
                        var device = (devicesBox.SelectedItem as Controls.DeviceListItem);
                        device.deviceInfo.PingOutcome = MagicStructs.PingOutcome.Controller;
                        device.deviceInfo.lastConfiguration = data;
                        device.UpdateDeviceLabelColor();
                    }
                }));
                UpdateDeviceConfigUI(data);
            }
        }

        public void UpdateDeviceConfigUI(MagicStructs.Controller data)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Trace.WriteLine("The device power is: " + data.powerState);
                //if (OnRB.IsChecked == null || OffRB.IsChecked == null) return;
                OnRB.Checked -= PowerOnChecked;
                OffRB.Checked -= PowerOffChecked;
                if (data.powerState && !OnRB.IsChecked.Value)
                    OnRB.IsChecked = true;
                else if (!data.powerState && !OffRB.IsChecked.Value)
                    OffRB.IsChecked = true;
                OnRB.Checked += PowerOnChecked;
                OffRB.Checked += PowerOffChecked;
                //OnRB.IsChecked = data.powerState;
                isCustomFunction = data.functionMode == 0x61 ? false : true;
                DiscoButton.Style = (isCustomFunction ? Resources["DeleteButton"] : Resources["NormalButton"]) as Style;
                isDisco = isCustomFunction;

                //If any custom function is running then make Disco button red

                //if (!isCustomFunction)
                //{
                //    isDisco = false;
                //    DiscoButton.Style = Resources["NormalButton"] as Style;
                //}
            }
            ));

            var currentColor = data.currentColor;

            if (isCustomFunction)
                Trace.WriteLine("Is running a custom func");

            if (lastSelectedColor != currentColor && !isCustomFunction)
            {
                Trace.WriteLine("Selected Color changed to:" + currentColor.ToString());
                //lastSelectedColor = lastSelectedColor == colorBackup ? currentColor : lastSelectedColor;
                lastSelectedColor = currentColor;
                currentColor.A = 255;
                //MainColorPicker.SecondaryColor = currentColor;
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MainColorPicker.ColorChanged -= OnColorChanged;
                    MainColorPicker.SelectedColor = currentColor;
                    MainColorPicker.ColorChanged += OnColorChanged;
                }
                ));
            }
            wasInitialized = true;
            Application.Current.Dispatcher.Invoke(new Action(() =>
                ConnectionStateLabel.Content = "Device Connected" //Now that I think about I probably should place this hole method in one of these fancy "Application.Current.Dispatcher.Invoke" things...
            ));
        }

        private void MouseTabDrag(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MinimalizeClicked(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ExitClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveColorClicked(object sender, RoutedEventArgs e)
        {
            //(Color, string) newColor = (lastSelectedColor, colorNameBox.Text);
            if (colorNameBox.Text.Length < 1) return;
            var currentColor = Utils.ColorToSimpleColor(lastSelectedColor);
            currentColor.A = 255;
            currentUserData.SavedColor[colorNameBox.Text] = currentColor;

            Utils.SaveUserData(currentUserData);

            ReloadSavedColorsBox();
            colorSelectBox.SelectedIndex = currentUserData.SavedColor.ToList().IndexOf(new KeyValuePair<string, Structs.SimpleColor>(colorNameBox.Text, currentColor));
        }

        private void DeleteColorClicked(object sender, RoutedEventArgs e)
        {
            var selectedLabel = (colorSelectBox.SelectedItem as Label);
            if (selectedLabel == null || selectedLabel.DataContext == null) return;
            currentUserData.SavedColor.Remove(((selectedLabel.DataContext) as KeyValuePair<string, Structs.SimpleColor>?).Value.Key);

            Utils.SaveUserData(currentUserData);
            ReloadSavedColorsBox();
            if (colorSelectBox.Items.Count > 0)
            {
                colorSelectBox.SelectedIndex = 0;
            }
        }

        private void SavedColorsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Earlier Used for changing the color, now useles because there is "Apply Color" button
        }

        private void DeviceAddClicked(object sender, RoutedEventArgs e)
        {
            if (newDeviceWindow == null || !newDeviceWindow.IsLoaded)
            {
                newDeviceWindow = new Controls.AddDeviceWindow();
                newDeviceWindow.Show();
            }
        }

        private async void OnDiscoButtonClicked(object sender, RoutedEventArgs e)
        {
            isDisco = !isDisco;
            isCustomFunction = isDisco;
            DiscoButton.Style = (isDisco ? Resources["DeleteButton"] : Resources["NormalButton"]) as Style;
            if (isDisco)
            {
                await deviceController.Send(Commands.SET_DISCO_FUNCTION);
                //await Task.Delay(200);
                //await deviceController.Send(Commands.SET_DISCO_FUNCTION);
            }
            if (!isDisco) deviceController.SetColor(lastSelectedColor);
        }

        public void SaveDevice(MagicStructs.DeviceInfo device)
        {
            if (device == null) return;


            if (currentUserData.Devices.Any(d => d.Address == device.Address)) //This device is already added, so just rename it
            {
                var duplicate = currentUserData.Devices.First(d => d.Address == device.Address);
                duplicate.Name = device.Name;
            }
            else if (currentUserData.Devices.Any(d => d.Name == device.Name)) //This device is already added, so just re-ip it
            {
                var duplicate = currentUserData.Devices.First(d => d.Name == device.Name);
                duplicate.Address = device.Address;
            }
            else
            {
                currentUserData.Devices.Add(device);
            }
            Utils.SaveUserData(currentUserData);

            AddDevice(device);
        }

        private async void SelectedDeviceChanged(object sender, SelectionChangedEventArgs e)
        {
            if (devicesBox.SelectedItem == null || (devicesBox.SelectedItem as Controls.DeviceListItem) == null)
                return;
            wasInitialized = true;
            deviceController.Disconnect();
            var selectedDevice = (devicesBox.SelectedItem as Controls.DeviceListItem).deviceInfo;
            if (selectedDevice == null) return;
            selectedDeviceIp = selectedDevice.Address;
            if (selectedDevice.PingOutcome == MagicStructs.PingOutcome.Controller)
            {
                var connectionStatus = await deviceController.Connect(selectedDeviceIp);
                if (connectionStatus)
                {
                    if (selectedDevice.lastConfiguration != null)
                        UpdateDeviceConfigUI(selectedDevice.lastConfiguration);
                    SetControllerData();
                }
            }
            else
            {
                var pingOutcome = await MagicUtils.FullPingDeviceAsync(selectedDeviceIp);
                selectedDevice.PingOutcome = pingOutcome.Item1;
                if (pingOutcome.Item1 == MagicStructs.PingOutcome.Controller)
                {
                    var connectionStatus = await deviceController.Connect(selectedDeviceIp);
                    if (connectionStatus)
                    {
                        if (selectedDevice.lastConfiguration != null)
                            UpdateDeviceConfigUI(selectedDevice.lastConfiguration);
                        SetControllerData();
                    }
                }
                else if (pingOutcome.Item1 == MagicStructs.PingOutcome.Device)
                    deviceController.Disconnect();
            }
            (devicesBox.SelectedItem as Controls.DeviceListItem).UpdateDeviceLabelColor();
        }

        private void removeDeviceClicked(object sender, RoutedEventArgs e)
        {
            if (devicesBox.SelectedItem != null)
            {
                if (MessageBox.Show("Are you sure you want to remove currently selected Controller?", "Waring!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    currentUserData.Devices.Remove((devicesBox.SelectedItem as Controls.DeviceListItem).deviceInfo);
                    Utils.SaveUserData(currentUserData);
                    devicesBox.Items.RemoveAt(devicesBox.SelectedIndex);
                    if (devicesBox.Items.Count > 0)
                    {
                        devicesBox.SelectedIndex = 0;
                    }
                    devicesBox.Items.Refresh();
                }
            }
        }

        private void applyColorClicked(object sender, RoutedEventArgs e)
        {
            var selectedLabel = (colorSelectBox.SelectedItem as Label);
            if (selectedLabel == null) return;
            var kvp = ((selectedLabel.DataContext) as KeyValuePair<string, Structs.SimpleColor>?);
            if (selectedLabel.DataContext == null || kvp == null) return;
            var selectedColor = Utils.SimpleColorToColor(kvp.Value.Value);
            selectedColor.A = 255;
            colorNameBox.Text = kvp.Value.Key;
            MainColorPicker.SelectedColor = selectedColor;
        }

        public async void AddDevice(MagicStructs.DeviceInfo device)
        {
            bool controllerPing;
            var ping = new Ping();
            //ping.PingCompleted += new PingCompletedEventHandler(OnPingCompleted);
            //var reply = await ping.SendPingAsync(device.Address, 200);
            //if(reply != null && reply.Status == IPStatus.Success)
            //    controllerPing = await DeviceController.PingDevice(device.Address);
            //else
            //    controllerPing = false;
            var reply = await MagicUtils.FullPingDeviceAsync(device.Address);

            for (int i = 0; i < devicesBox.Items.Count; i++)
            {
                var item = devicesBox.Items.GetItemAt(i) as Controls.DeviceListItem;
                if ((item.deviceInfo).Address == device.Address || (item.deviceInfo).Name == device.Name)
                {
                    item.deviceInfo = device;
                    devicesBox.Items.Refresh();
                    return;
                }
            }

            var deviceListItem = new Controls.DeviceListItem(device);

            devicesBox.Items.Add(deviceListItem);
            if (deviceListItem != null)
            {
                devicesBox.SelectedItem = deviceListItem;
                devicesBox.Items.Refresh();
            }
        }
    }
}
