using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker;
using MahApps.Metro.Controls.Dialogs;

namespace HDT_Reconnect
{
    public class ReconnectPlugin : IPlugin
    {
        public string Name => "ReconnectPlugin";

        public string Description => "Quickly skip hearthstone animation by disconnecting and reconnecting";

        public string ButtonText => "No Settings";

        public string Author => "Hypervisor";

        public Version Version => Version.Parse("1.0.0");

        public MenuItem MenuItem { get; private set; }

        private ReconnectForm reconnectForm;

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
            if (reconnectForm != null)
            {
                reconnectForm.OnUpdate();
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

                if (reconnectForm == null)
                {
                    reconnectForm = new ReconnectForm();
                }
            };

            MenuItem.Unchecked += (sender, args) =>
            {
                if (reconnectForm != null)
                {
                    reconnectForm.Dispose();
                    reconnectForm = null;
                }
            };
        }
    }
}