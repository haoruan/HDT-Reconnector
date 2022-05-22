using Hearthstone_Deck_Tracker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hearthstone_Deck_Tracker.Utility.Logging;

namespace HDT_Reconnector
{
    public class ResizeSettings
    {
        public double Height;
        public double Width;
        public double FontSize;
        public double Top;
        public double Left;
    }

    public class Settings
    {

        private static Settings _settings;
        public ResizeSettings reconnect = new ResizeSettings();
        public ResizeSettings simulate = new ResizeSettings();

        public Settings()
        {
            reconnect.Height = 40;
            reconnect.Width = 140;
            reconnect.Top = 1080 * 90 / 100;
            reconnect.Left = 1920 - reconnect.Width;
            reconnect.FontSize = 20;

            simulate.Height = 40;
            simulate.Width = 140;
            simulate.Top = 1080 * 90 / 100 - reconnect.Height;
            simulate.Left = 1920 - simulate.Width;
            simulate.FontSize = 20;
        }

        public static Settings Instance
		{
			get
			{
				if (_settings == null)
				{
					_settings = new Settings();
				}

				return _settings;
			}
		}

        public static void Load()
        {
            string path = Path.Combine(Config.AppDataPath, "reconnector.xml");
            if (File.Exists(path))
            {
                try
                {
                    _settings = XmlManager<Settings>.Load(path);
                }
                catch(Exception ex)
                {
                    Log.Error("Error loading plugin settings:\n" + ex);
                }
            }
        }

        public static void Save()
        {
            string path = Path.Combine(Config.AppDataPath, "reconnector.xml");
            try
            {
                XmlManager<Settings>.Save(path, Instance);
            }
            catch(Exception ex)
            {
                Log.Error("Error saving plugin settings:\n" + ex);
            }
        }
    }
}
