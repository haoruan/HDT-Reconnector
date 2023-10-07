using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hearthstone_Deck_Tracker.Utility.Logging;

namespace HDT_Reconnector.LogHandler
{
    internal class LogWatcher
    {
        internal const int UpdateDelay = 200;
        private readonly ConnectionLogHandler connectionLogHandler = new ConnectionLogHandler();
        private ReconnectPanel reconnectPanel;
        private readonly List<LogReader> logReaders = new List<LogReader>();

        public LogWatcher(ReconnectPanel reconnectPanel, string logName)
        {
            this.reconnectPanel = reconnectPanel;

            Log.Info(String.Format("Adding LogReader for file: {0}", logName));
            var connectionLogReader = new LogReader(logName);
            connectionLogReader.OnNewLine += ConnectionLogReader_OnNewLine;
            logReaders.Add(connectionLogReader);
        }

        public void Stop()
        {
            foreach (var logReader in logReaders)
            {
                logReader.Stop();
            }
            logReaders.Clear();
        }

        private void ConnectionLogReader_OnNewLine(string obj)
        {
            connectionLogHandler.Handle(obj, reconnectPanel);
        }

        public void Start()
        {
            foreach (var logReader in logReaders)
            {
                logReader.Start();
            }
        }
    }
}
