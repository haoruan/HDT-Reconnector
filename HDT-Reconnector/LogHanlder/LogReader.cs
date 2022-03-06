using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;

namespace HDT_Reconnector.LogHandler
{
    internal class LogReader
    {
        private ConcurrentQueue<string> _lines = new ConcurrentQueue<string>();
        private string filePath;
		private long offset;
		private bool stop;
		private Thread threadRead;
		private Thread threadParse;

        public event Action<string> OnNewLine;

        public LogReader(string filePath)
        {
            this.filePath = filePath;
        }

        public void Start()
        {
            stop = false;
			offset = 0;
			threadRead = new Thread(ReadLogFile) { IsBackground = true };
			threadRead.Start();

			threadParse = new Thread(ParseLogFile) { IsBackground = true };
			threadParse.Start();
        }

        public void Stop()
        {
			stop = true;
            StopThread(threadRead);
            StopThread(threadParse);
            _lines = null;
        }

        private void StopThread(Thread thread)
		{
			while(thread == null || thread.ThreadState == ThreadState.Unstarted)
            {
                Thread.Sleep(100);
            }
			thread?.Join();
		}

        private void ParseLogFile()
        {
            while (!stop)
            {
                var count = _lines.Count;
                for(var i = 0; i < count; i++)
                {
                    if (_lines.TryDequeue(out string line))
                    {
                        OnNewLine(line);
                    }
                }
                Thread.Sleep(LogWatcher.UpdateDelay);
            }
        }

        private void ReadLogFile()
        {
            while (!stop)
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists)
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fs.Seek(offset, SeekOrigin.Begin);
                        if (fs.Length == offset)
                        {
                            Thread.Sleep(LogWatcher.UpdateDelay);
                            continue;
                        }
                        using (var sr = new StreamReader(fs))
                        {
                            string line;
                            while (!sr.EndOfStream && (line = sr.ReadLine()) != null)
                            {
                                _lines.Enqueue(line);
                                offset += Encoding.UTF8.GetByteCount(line + Environment.NewLine);
                            }
                        }
                    }
                }
                Thread.Sleep(LogWatcher.UpdateDelay);
            }
        }
    }
}
