using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

using System.Windows.Controls.Primitives;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using System.Windows.Media.Animation;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace HDT_Reconnector
{
    /// <summary>
    /// Interaction logic for ReconnectPanel.xaml
    /// </summary>
    public partial class ReconnectPanel : UserControl
    {
        private Reconnector reconnect;
        private DateTime lastGameStartTime;
        private double oriWidth;
        private double oriHeight;
        private double oriFontSize;
        private Brush oriBrush;

        public ReconnectPanel()
        {
            reconnect = new Reconnector();
            InitializeComponent();

            oriHeight = ReconnectButton.Height;
            oriWidth = ReconnectButton.Width;
            oriFontSize = ReconnectButton.FontSize;
            oriBrush = ReconnectButton.Background;

            OverlayExtensions.SetIsOverlayHitTestVisible(this, true);

            UpdatePosition();
            Core.OverlayCanvas.AddHandler(SizeChangedEvent, new RoutedEventHandler((sender, e) =>
            {
                UpdatePosition();
            }));
        }
        public void OnUpdate()
        {
            if (Visibility != Visibility.Visible && !Core.Game.IsInMenu)
            {
                Visibility = Visibility.Visible;
            }
            else if (Visibility != Visibility.Hidden && Core.Game.IsInMenu)
            {
                Visibility = Visibility.Hidden;
            }

            if (reconnect.Status == Reconnector.CONNECTION_STATUS.DISCONNECTED)
            {
                if (IsGameReStart())
                {
                    reconnect.Status = Reconnector.CONNECTION_STATUS.CONNECTED;
                    ReconnectButton.Content = Reconnector.ReconnectString;
                }
            }
        }

        private void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAbleToReconnect())
            {
                lastGameStartTime = Core.Game.CurrentGameStats.StartTime;
                if (reconnect.Disconnect() == 0)
                {
                    ReconnectButton.Content = Reconnector.DisconnectedString;
                }
            }
        }

        private void UpdatePosition()
        {
            ReconnectButton.Width = oriWidth * Core.OverlayWindow.AutoScaling;
            ReconnectButton.Height = oriHeight * Core.OverlayWindow.AutoScaling;
            ReconnectButton.FontSize = oriFontSize * Core.OverlayWindow.AutoScaling;

            Canvas.SetBottom(this, Core.OverlayWindow.Height * 10 / 100);
            Canvas.SetRight(this, 0);
        }

        private void ReconnectButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsAbleToReconnect())
            {
                ReconnectButton.Background = Brushes.Gray;
            }
        }

        private void ReconnectButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ReconnectButton.Background = oriBrush;
        }

        private bool IsAbleToReconnect()
        {
            return reconnect.Status == Reconnector.CONNECTION_STATUS.CONNECTED && !IsGameEnd();
        }

        private bool IsGameReStart()
        {
            return Core.Game.CurrentGameStats.StartTime > lastGameStartTime;
        }
        private bool IsGameEnd()
        {
            return Core.Game.CurrentGameStats.EndTime > Core.Game.CurrentGameStats.StartTime;
        }
    }
}
