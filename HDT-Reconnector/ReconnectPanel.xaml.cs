using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;

using Core = Hearthstone_Deck_Tracker.API.Core;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HDT_Reconnector.LogHandler;
using System.Windows.Controls.Primitives;

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
        private Resize resize;
        private BattlegroundsBoardStat bgBoardStat = new BattlegroundsBoardStat();
        private Timer timer;
        private readonly int disconnectTimeout = 20; // second

        public string RemoteAddr { get; set; } = null;
        public ushort RemotePort { get; set; } = 0;

        public ReconnectPanel()
        {
            InitializeComponent();
            Settings.Load();

            resize = new Resize(this, ReconnectButton, ReconnectText, Settings.Instance.reconnect);
            timer = new Timer(new TimerCallback(DisconnectedTimeout));

            Visibility = Visibility.Collapsed;
            oriBrush = ReconnectButton.Background;

            resize.AddToOverlayWindowPrivate();
            resize.UpdatePosition();

            updatePositionHandler = new RoutedEventHandler((sender, e) =>
            {
                resize.UpdatePosition();
            });
            Core.OverlayCanvas.AddHandler(SizeChangedEvent, updatePositionHandler);
        }

        public void Dispose()
        {
            timer.Dispose();
            Settings.Instance.reconnect = resize.settings;
            Settings.Save();
            resize.RemoveFromOverlayWindowPrivate();
            Core.OverlayCanvas.RemoveHandler(SizeChangedEvent, updatePositionHandler);
            connectionLogWatcher?.Stop();
        }

        public void OnUpdate()
        {
            // Show reconnect button:
            // 1. When we want to resize to move it in menu
            // 2. When we're in the match
            Visibility = Core.Game.IsInMenu? resize.resizeGrip.Visibility : Visibility.Visible;

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
                    ReconnectText.Text = Utils.GetLoc("Text_Reconnect");
                    reconnect.ResumeConnect(timer);

                    if (!IsGameEnd())
                    {
                        RestoreAfterReconnect();
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
                    SaveBeforeReconnect();
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
                timer.Change(disconnectTimeout * 1000, 1000);
                SaveBeforeReconnect();
                lastGameStartTime = Core.Game.CurrentGameStats.StartTime;
                lock (this)
                {
                    if (reconnect.Disconnect(RemoteAddr, RemotePort) == 0)
                    {
                        ReconnectText.Text = Utils.GetLoc("Text_Disconnected");
                    }else
                    {
                        timer.Change(Timeout.Infinite, 0);
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

        private void SaveBeforeReconnect()
        {
            if (Core.Game.IsBattlegroundsMatch)
            {
                bgBoardStat.SaveBoardStat();
                BobsBuddyHelper.Instance.SaveInput();
            }
        }

        private void RestoreAfterReconnect()
        {
            if (Core.Game.IsBattlegroundsMatch)
            {
                bgBoardStat.RestoreBoardStat();
            }
        }

        private void DisconnectedTimeout(Object state)
        {
            Log.Info("Diconnect Timeout.");
            ReconnectText.Dispatcher.Invoke(() =>
            {
                ReconnectText.Text = Utils.GetLoc("Text_Reconnect");
            });
            reconnect.ResumeConnect(timer);
        }
    }
}
