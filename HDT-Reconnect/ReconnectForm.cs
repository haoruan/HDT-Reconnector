using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HDT_Reconnect
{
    public partial class ReconnectForm : Form
    {
        const string reconnectButtonText = "Disconnect";
        const string reconnectButtonChangeText = "Disconnecting...";
        public ReconnectForm()
        {
            InitializeComponent();
        }

        private void Disconnect()
        {

        }

        private void ReconnectButton_Click(object sender, EventArgs e)
        {
            if (ReconnectButton.Text == reconnectButtonText)
            {
                ReconnectButton.Text = reconnectButtonChangeText;

                Disconnect();

                ReconnectButton.Text = reconnectButtonText;

            }
        }
    }
}
