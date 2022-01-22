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

namespace MagicLedControl.Controls
{
    /// <summary>
    /// Interaction logic for PluginListItem.xaml
    /// </summary>
    public partial class PluginListItem : UserControl
    {
        private Structs.PluginInfo? pluginInfo = null;
        public PluginListItem(Structs.PluginInfo pluginInfo)
        {
            InitializeComponent();

            pluginNameLabel.Content = pluginInfo.Name;
            pluginEnabledCB.IsChecked = pluginInfo.IsEnabled;

            var tt = new ToolTip();
            tt.Content = pluginInfo.Description;
            backGrid.ToolTip = tt;

            this.pluginInfo = pluginInfo;
        }

        private void pluginStateChanged(object sender, RoutedEventArgs e)
        {
            CheckBox? cb = sender as CheckBox;
            if(cb != null && pluginInfo != null && cb.IsChecked != null)
            {
                pluginInfo.IsEnabled = cb.IsChecked.Value;
                pluginInfo.Plugin?.ChangeState(cb.IsChecked.Value);

                Utils.SaveUserData((Application.Current.MainWindow as MainWindow).currentUserData);
            }
        }

        private void OpenSettingsClicke(object sender, RoutedEventArgs e)
        {
            if (pluginInfo == null || !pluginInfo.IsEnabled) return;
            pluginInfo.Plugin?.SettingsClicked();
        }
    }
}
