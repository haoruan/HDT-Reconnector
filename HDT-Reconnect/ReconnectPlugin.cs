using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Plugins;

namespace HDT_Reconnect
{
    public class ReconnectPlugin : IPlugin
    {
        public string Name => "ReconnectPlugin";

        public string Description => "Quickly skip hearthstone animation by disconnecting and reconnecting";

        public string ButtonText => null;

        public string Author => "Hypervisor";

        public Version Version => Version.Parse("1.0.0");

        public MenuItem MenuItem => null;

        private ReconnectForm reconnectForm;

        public void OnButtonPress()
        {
        }

        public void OnLoad()
        {
            reconnectForm = new ReconnectForm();
        }

        public void OnUnload()
        {
            reconnectForm.Dispose();
            reconnectForm = null;
        }

        public void OnUpdate()
        {
            reconnectForm.OnUpdate();
        }
    }
}