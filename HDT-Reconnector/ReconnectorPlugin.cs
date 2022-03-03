using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using MahApps.Metro.Controls.Dialogs;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace HDT_Reconnector
{
    public class ReconnectorPlugin : IPlugin
    {
        public string Name => "Reconnector";

        public string Description => "Quickly skip hearthstone animation by disconnecting and reconnecting\n\nFor more information and updates, check out:\nhttps://github.com/haoruan/HDT-Reconnector";

        public string ButtonText => "No Settings";

        public string Author => "Hypervisor";

        public Version Version => Version.Parse("1.0.0");

        public MenuItem MenuItem { get; private set; }

        private ReconnectPanel reconnectPanel;

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
                reconnectPanel.OnUpdate();
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
                    try
                    {
                        reconnectPanel = new ReconnectPanel();
                        Core.OverlayCanvas.Children.Add(reconnectPanel);
                    }
                    catch (WfpException ex)
                    {
                        Log.Error(ex);
                    }
                }
            };

            MenuItem.Unchecked += (sender, args) =>
            {
                if (reconnectPanel != null)
                {
                    Core.OverlayCanvas.Children.Remove(reconnectPanel);
                    reconnectPanel = null;
                }
            };
        }
    }
}