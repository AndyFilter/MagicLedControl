using MagicLedControl.PluginLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MagicLedControl.ExamplePlugin.Control
{
    /// <summary>
    /// Interaction logic for ExampleUserControl.xaml
    /// </summary>
    public partial class ExampleUserControl : Window
    {
        private DeviceController deviceController = null;
        public ExampleUserControl(DeviceController controller)
        {
            InitializeComponent();

            deviceController = controller;
        }

        private void GreenColorClicked(object sender, RoutedEventArgs e)
        {
            if (deviceController == null) return;
            deviceController.SetColor(Color.FromRgb(0, 255, 0));
        }

        private void RedColorClicked(object sender, RoutedEventArgs e)
        {
            if (deviceController == null) return;
            deviceController.SetColor(Color.FromRgb(255, 0, 0));
        }
    }
}
