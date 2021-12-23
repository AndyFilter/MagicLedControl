using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using ColorPicker.Models;
using System.Text.Json;

namespace MagicLedControl
{
    public partial class MainWindow : Window
    {
        public DeviceController deviceController = new DeviceController();
        private string IP_ADDRESS = "192.168.0.206";//Currentyl static, will change on first release! Or will I? I might also forget to remove this comment...
        private Color lastSelectedColor;
        private bool wasInitialized = false;
        private Timer UpdateDataMethod;//Not really used, and I dont really see a use for it either, but better to have it than sorry... right?
        private bool isDisco = false, isCustomFunction = false;
        private Structs.UserData currentUserData = new Structs.UserData();

        public MainWindow()
        {
            InitializeComponent();
            var _tempUserData = Utils.GetUserData();
            if (_tempUserData != null)
            {
                currentUserData = _tempUserData;
                if (currentUserData.Devices.Count <= 0 || currentUserData.Devices[0] == null)
                {
                    Trace.WriteLine("Adding new device");
                    currentUserData.Devices.Add(new MagicStructs.DeviceInfo("Led Device", IP_ADDRESS));
                }
                else
                {
                    IP_ADDRESS = currentUserData.Devices[0].Address;
                }
                if (currentUserData.SavedColor.Count > 0)
                {
                    ReloadSavedColorsBox();
                }
                //currentUserData.deviceInfo = new MagicStructs.DeviceInfo() { address = IP_ADDRESS, name = "Krzyś" };
            }
            InitializeDevice();
            //Thread.Sleep(1000);
            //deviceController.Send(payload);
            //deviceController.Send(Commands.TURN_ON);
        }

        private void ReloadSavedColorsBox()
        {
            colorSelectBox.Items.Clear();
            colorSelectBox.ItemsSource = null;
            Trace.WriteLine("Childer in CB: " + colorSelectBox.Items.Count);
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
            //Trace.WriteLine(e.ToString());
            if (!wasInitialized) return;
            SetLedPowerState(true);
        }

        private void PowerOffChecked(object sender, RoutedEventArgs e)
        {
            //Trace.WriteLine(e.Handled.ToString());
            if (!wasInitialized) return;
            SetLedPowerState(false);
        }

        public async void SetLedPowerState(bool newState)
        {
            Trace.WriteLine("the leds are now: " + (newState ? "On" : "Off"));
            var ct = await deviceController.SetPowerState(newState);
            await Task.Delay(500);
            if (ct.CanBeCanceled)
                ct.WaitHandle.Close();
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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            deviceController.client?.Close();
            deviceController.stream?.Close();//Should close it self when closing the program, but you never know.
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
            await deviceController.Connect(IP_ADDRESS, "");
            //SetControllerData();
            UpdateDataMethod = new System.Threading.Timer((e) =>
            {
                Trace.WriteLine("Retrying connectio");
                SetControllerData();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            await Task.Delay(1000);
        }

        private async void SetControllerData()
        {
            var data = await deviceController.RequestControllerData();

            //var colorBackup = lastSelectedColor;

            if (data == null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                    ConnectionStateLabel.Content = "Device Disconnected"
                ));
                return;
            }

            //wasDataJustReceived = true;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Trace.WriteLine("The device powert is: " + data.powerState);
                //if (OnRB.IsChecked == null || OffRB.IsChecked == null) return;
                if (data.powerState && !OnRB.IsChecked.Value)
                    OnRB.IsChecked = true;
                else if (!data.powerState && !OffRB.IsChecked.Value)
                    OffRB.IsChecked = true;
                //OnRB.IsChecked = data.powerState;
                isCustomFunction = data.functionMode == 0x61 ? false : true;
                if (!isCustomFunction)
                {
                    isDisco = false;
                    DiscoButton.Style = Resources["NormalButton"] as Style;
                }
            }
            ));

            var currentColor = data.currentColor;

            if (isCustomFunction)
                Trace.WriteLine("Is running a custm func");

            if (lastSelectedColor != currentColor && !isCustomFunction)
            {
                Trace.WriteLine("Selected Color changed to:" + currentColor.ToString());
                //lastSelectedColor = lastSelectedColor == colorBackup ? currentColor : lastSelectedColor;
                lastSelectedColor = currentColor;
                currentColor.A = 255;
                //MainColorPicker.SecondaryColor = currentColor;
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MainColorPicker.SelectedColor = currentColor;
                }
                ));
            }
            wasInitialized = true;
            Application.Current.Dispatcher.Invoke(new Action(() =>
                ConnectionStateLabel.Content = "Device Connected" //Now that I think about I probably should place this hole method in one of these fancy "Application.Current.Dispatcher.Invoke" things...
            ));
            //wasDataJustReceived = false;
        }

        private void MouseTabDrag(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MinimalizeClicked(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeClicked(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowBorder.Margin = new Thickness(0);
                    this.WindowState = WindowState.Normal;
                    button.Content = "◻";//Bruhhh
                    break;
                case WindowState.Normal:
                    WindowBorder.Margin = new Thickness(5);
                    this.WindowState = WindowState.Maximized;
                    button.Content = "❏";//u fr?
                    break;
            }
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

            //Utils.SaveUserData(new Structs.UserData() { deviceInfo = new MagicStructs.DeviceInfo() { address = "nigger", name = "faggot"} });
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
            if(colorSelectBox.Items.Count > 0)
            {
                colorSelectBox.SelectedIndex = 0;
            }
        }

        private void SavedColorsSelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private async void OnDiscoButtonClicked(object sender, RoutedEventArgs e)
        {
            isDisco = !isDisco;
            isCustomFunction = isDisco;
            DiscoButton.Style = (isDisco ? Resources["DeleteButton"] : Resources["NormalButton"]) as Style;
            if (isDisco) {
                await deviceController.Send(Commands.SET_DISCO_FUNCTION);
                //await Task.Delay(200);
                //await deviceController.Send(Commands.SET_DISCO_FUNCTION);
            }
            if (!isDisco) deviceController.SetColor(lastSelectedColor);
        }
    }
}
