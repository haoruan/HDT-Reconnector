using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.API;
using MahApps.Metro.Controls.Dialogs;
using Hearthstone_Deck_Tracker.Utility.Logging;

using HDT_Reconnector.LogHandler;
using HDT_Reconnector.Native;

namespace HDT_Reconnector
{
    public class ReconnectorPlugin : IPlugin
    {
        public string Name => "Reconnector";

        public string Description => "Quickly skip hearthstone animation by disconnecting and reconnecting\n\nFor more information and updates, check out:\nhttps://github.com/haoruan/HDT-Reconnector";

        public string ButtonText => "No Settings";

        public string Author => "Hypervisor";

        public Version Version => Version.Parse("1.4.6");

        public MenuItem MenuItem { get; private set; }

        private ReconnectPanel reconnectPanel;
        private SimulatePanel simulatePanel;

        public void OnButtonPress()
        {
        }

        public void OnLoad()
        {
            CreateMenuItem();
            MenuItem.IsChecked = true;
        }

        public void OnUnload()
        {
            MenuItem.IsChecked = false;
        }

        public void OnUpdate()
        {
            if (reconnectPanel != null)
            {
                try
                {
                    reconnectPanel.OnUpdate();
                }
                catch (LogException ex)
                {
                    MenuItem.IsChecked = false;
                    Log.Error(ex);
                }

            }

            if (simulatePanel != null)
            {
                try
                {
                    simulatePanel.OnUpdate();
                }
                catch (LogException ex)
                {
                    MenuItem.IsChecked = false;
                    Log.Error(ex);
                }

            }
        }

        private void CreateMenuItem()
        {
            MenuItem = new MenuItem()
            {
                Header = "Reconnector"
            };

            MenuItem.IsCheckable = true;

            MenuItem.Checked += async (sender, args) =>
            {
                if (!Utils.IsElevated())
                {
                    await Core.MainWindow.ShowMessageAsync("Permission Denied", "Restart with admin privilige to enable reconnect feature");
                    MenuItem.IsChecked = false;
                    return;
                }

                if (reconnectPanel == null)
                {
                    reconnectPanel = new ReconnectPanel();
                    Core.OverlayCanvas.Children.Add(reconnectPanel);
                }

                if (simulatePanel == null)
                {
                    simulatePanel = new SimulatePanel();
                    Core.OverlayCanvas.Children.Add(simulatePanel);
                }
            };

            MenuItem.Unchecked += (sender, args) =>
            {
                using (reconnectPanel)
                {
                    Core.OverlayCanvas.Children.Remove(reconnectPanel);
                    reconnectPanel = null;
                }

                using (simulatePanel)
                {
                    Core.OverlayCanvas.Children.Remove(simulatePanel);
                    simulatePanel= null;
                }
            };
        }
    }
}