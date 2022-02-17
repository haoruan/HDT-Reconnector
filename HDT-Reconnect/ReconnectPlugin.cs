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

        public string ButtonText => "None";

        public string Author => "Hypervisor";

        public Version Version => Version.Parse("1.0.0");

        public MenuItem MenuItem => null;

        public void OnButtonPress()
        {
        }

        public void OnLoad()
        {
        }

        public void OnUnload()
        {
        }

        public void OnUpdate()
        {
        }
    }
}
