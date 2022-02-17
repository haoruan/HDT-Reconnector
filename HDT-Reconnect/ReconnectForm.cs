using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;

namespace HDT_Reconnect
{
    public partial class ReconnectForm : Form
    {
        private Reconnect reconnect;
        DateTime lastGameStartTime;

        public ReconnectForm()
        {
            InitializeComponent();
            Visible = false;
        }

        public void OnUpdate()
        {
            if (!Core.Game.IsRunning)
            {
                Visible = false;
            }
            else
            {
                if (Core.Game.CurrentGameMode != GameMode.None && !Visible)
                {
                    Visible = true;
                }
            }

            if (Visible && reconnect.Status == Reconnect.CONNECTION_STATUS.DISCONNECTED)
            {
                if (Core.Game.CurrentGameStats.StartTime > lastGameStartTime)
                {
                    reconnect.Status = Reconnect.CONNECTION_STATUS.CONNECTED;
                    ReconnectButton.Text = Reconnect.ReconnectString;
                }
            }
        }


        private void ReconnectButton_Click(object sender, EventArgs e)
        {
            if (reconnect.Status == Reconnect.CONNECTION_STATUS.CONNECTED)
            {
                lastGameStartTime = Core.Game.CurrentGameStats.StartTime;
                if (reconnect.Disconnect() == 0)
                {
                    ReconnectButton.Text = Reconnect.DisconnectedString;
                }
            }
        }

        private void ReconnectForm_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                reconnect = new Reconnect();
                if (reconnect.Status == Reconnect.CONNECTION_STATUS.DISCONNECTED)
                {
                    Log.Error("Can't find the hearthstone tcp connection");
                    Visible = false;
                }
            }
            else
            {
                reconnect = null;
            }
        }
    }
}
