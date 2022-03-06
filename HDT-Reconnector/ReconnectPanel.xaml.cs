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
using System.IO;

using Config = Hearthstone_Deck_Tracker.Config;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HDT_Reconnector.LogHandler;

namespace HDT_Reconnector
{
    /// <summary>
    /// Interaction logic for ReconnectPanel.xaml
    /// </summary>
    public partial class ReconnectPanel : UserControl
    {
        private Reconnector reconnect;
        private DateTime lastGameStartTime;
        private const string logSearchPattern = "hearthstone*.log";
        private double oriWidth;
        private double oriHeight;
        private double oriFontSize;
        private Brush oriBrush;
        private LogWatcher connectionLogWatcher = null;

        public string RemoteAddr { get; set; } = null;
        public ushort RemotePort { get; set; } = 0;

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

        ~ReconnectPanel()
        {
            reconnect = null;
            connectionLogWatcher = null;
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
                if (IsGameReStart() || IsGameEnd())
                {
                    reconnect.Status = Reconnector.CONNECTION_STATUS.CONNECTED;
                    ReconnectButton.Content = Reconnector.ReconnectString;
                    reconnect.ResumeConnect();
                }
            }

            // When game starts, Hearthstone generates a new log file hearthstone_yy_mm_dd_hh_mm_ss.log
            // So clear the Watcher on game exiting, and create the Watcher on game starting
            // We may miss the network disconnected log If the game exits during disconnecting,
            // but it won't be a matter since we always restore the connection on game end.
            if (Core.Game.IsRunning && connectionLogWatcher == null)
            {
                var logDirectory = Path.Combine(Config.Instance.HearthstoneDirectory, Config.Instance.HearthstoneLogsDirectoryName);
                var folder = new DirectoryInfo(logDirectory);
                var logFiles = folder.GetFiles(logSearchPattern).OrderByDescending(f => f.CreationTime).ToList();

                if (logFiles.Count == 0)
                {
                    throw new LogException(String.Format("Can't find any {0} in {1}", logSearchPattern, logDirectory));
                }

                Log.Info(String.Format("Find {0}", logFiles[0].FullName));

                connectionLogWatcher = new LogWatcher(this, logFiles[0].FullName);
                connectionLogWatcher.Start();
            }

            if (!Core.Game.IsRunning && connectionLogWatcher != null)
            {
                connectionLogWatcher = null;
            }
        }

        private void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAbleToReconnect())
            {
                lastGameStartTime = Core.Game.CurrentGameStats.StartTime;
                if (reconnect.Disconnect(RemoteAddr, RemotePort) == 0)
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
            return RemoteAddr != null && RemotePort != 0 && reconnect.Status == Reconnector.CONNECTION_STATUS.CONNECTED && !IsGameEnd();
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
