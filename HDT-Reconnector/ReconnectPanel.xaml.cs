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

using Core = Hearthstone_Deck_Tracker.API.Core;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HDT_Reconnector.LogHandler;
using System.Windows.Controls.Primitives;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace HDT_Reconnector
{
    /// <summary>
    /// Interaction logic for ReconnectPanel.xaml
    /// </summary>
    public partial class ReconnectPanel : UserControl, IDisposable
    {
        private Reconnector reconnect = new Reconnector();
        private DateTime lastGameStartTime;
        private const string logSearchPattern = "hearthstone*.log";
        private bool hasRunningInit = false;
        private Brush oriBrush;
        private LogWatcher connectionLogWatcher = null;
        private RoutedEventHandler updatePositionHandler;
        private BattlegroundsBoardStat bgBoardStat = new BattlegroundsBoardStat();

        public string RemoteAddr { get; set; } = null;
        public ushort RemotePort { get; set; } = 0;

        public ReconnectPanel()
        {
            InitializeComponent();
            Settings.Load();

            Visibility = Visibility.Collapsed;
            oriBrush = ReconnectButton.Background;

            resizeGrip = new ResizeGrip();
            resizeGrip.MouseDown += ResizeGrip_MouseDown;
            resizeGrip.MouseMove += ResizeGrip_MouseMove;
            resizeGrip.MouseUp += ResizeGrip_MouseUp;
            resizeGrip.Visibility = Visibility.Collapsed;
            OverlayExtensions.SetIsOverlayHitTestVisible(resizeGrip, true);

            AddToOverlayWindowPrivate();

            UpdatePosition();
            updatePositionHandler = new RoutedEventHandler((sender, e) =>
            {
                UpdatePosition();
            });
            Core.OverlayCanvas.AddHandler(SizeChangedEvent, updatePositionHandler);
        }

        public void Dispose()
        {
            Settings.Save();
            RemoveFromOverlayWindowPrivate();
            Core.OverlayCanvas.RemoveHandler(SizeChangedEvent, updatePositionHandler);
            connectionLogWatcher?.Stop();
        }

        public void OnUpdate()
        {
            // Show reconnect button:
            // 1. When we want to resize to move it in menu
            // 2. When we're in the match
            Visibility = Core.Game.IsInMenu? resizeGrip.Visibility : Visibility.Visible;

            if (reconnect.Status == Reconnector.CONNECTION_STATUS.DISCONNECTED)
            {
                // We will restore the boardstat on every reconnect, including hearthstone restart.
                // But there is a case that we close the game and can't reconnect, like losing the game,
                // in this case we will return to main menu or in battlegrounds menu (this func is called every 100ms), 
                // so we have to reset is reconnect status
                if (IsInMainOrBgMenu())
                {
                    Log.Info("Can't reconnect to the game");
                    reconnect.Status = Reconnector.CONNECTION_STATUS.CONNECTED;
                    return;
                }

                if (IsGameReStart() || IsGameEnd())
                {
                    reconnect.Status = Reconnector.CONNECTION_STATUS.CONNECTED;
                    ReconnectText.Text = Reconnector.ReconnectString;
                    reconnect.ResumeConnect();

                    if (!IsGameEnd())
                    {
                        bgBoardStat.RestoreBoardStat();
                    }
                }
            }

            // When game starts, Hearthstone generates a new log file hearthstone_yy_mm_dd_hh_mm_ss.log
            // So clear the Watcher on game exiting, and create the Watcher on game starting
            // We may miss the network disconnected log If the game exits during disconnecting,
            // but it won't be a matter since we always restore the connection on game end.
            if (Core.Game.IsRunning && !hasRunningInit)
            {
                var logDirectory = Path.Combine(Config.Instance.HearthstoneDirectory,
                    Config.Instance.HearthstoneLogsDirectoryName);
                var folder = new DirectoryInfo(logDirectory);
                var logFiles = folder.GetFiles(logSearchPattern).OrderByDescending(f => f.CreationTime).ToList();

                if (logFiles.Count == 0)
                {
                    throw new LogException(String.Format("Can't find any {0} in {1}", logSearchPattern, logDirectory));
                }

                Log.Info(String.Format("Find {0}", logFiles[0].FullName));

                connectionLogWatcher = new LogWatcher(this, logFiles[0].FullName);
                connectionLogWatcher.Start();

                hasRunningInit = true;
            }

            // Game closing
            if (!Core.Game.IsRunning && hasRunningInit)
            {
                connectionLogWatcher.Stop();

                if (IsAbleToReconnect())
                {
                    // A simulation of disconnecting when game is closed on any gamemode
                    bgBoardStat.SaveBoardStat();
                    lastGameStartTime = Core.Game.CurrentGameStats.StartTime;
                    reconnect.Status = Reconnector.CONNECTION_STATUS.DISCONNECTED;
                }

                hasRunningInit = false;
            }
        }

        private void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAbleToReconnect())
            {
                bgBoardStat.SaveBoardStat();
                lastGameStartTime = Core.Game.CurrentGameStats.StartTime;
                lock (this)
                {
                    if (reconnect.Disconnect(RemoteAddr, RemotePort) == 0)
                    {
                        ReconnectText.Text = Reconnector.DisconnectedString;
                    }
                }
            }
        }

        private void ReconnectButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsAbleToReconnect())
            {
                ReconnectButton.Background = new SolidColorBrush(Color.FromRgb(0x2E, 0x34, 0x38));
            }
        }

        private void ReconnectButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ReconnectButton.Background = oriBrush;
        }

        private bool IsAbleToReconnect()
        {
            return RemoteAddr != null &&
                RemotePort != 0 &&
                reconnect.Status == Reconnector.CONNECTION_STATUS.CONNECTED &&
                Core.Game.CurrentGameMode != GameMode.None &&
                !IsGameEnd();
        }

        private bool IsGameReStart()
        {
            return Core.Game.CurrentGameMode != GameMode.None &&
                Core.Game.CurrentGameStats != null &&
                Core.Game.CurrentGameStats.StartTime > lastGameStartTime;
        }

        private bool IsInMainOrBgMenu()
        {
            return Core.Game.CurrentMode == Mode.HUB || Core.Game.CurrentMode == Mode.BACON;
        }

        private bool IsGameEnd()
        {
            return Core.Game.CurrentGameStats != null &&
                Core.Game.CurrentGameStats.EndTime > Core.Game.CurrentGameStats.StartTime;
        }
    }
}
