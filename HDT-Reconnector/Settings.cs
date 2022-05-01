using Hearthstone_Deck_Tracker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MahApps.Metro.Controls.Dialogs;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace HDT_Reconnector
{
    public class Settings
    {
        public double Height;
        public double Width;
        public double FontSize;
        public double Top;
        public double Left;

        public static Settings _settings;

        public Settings()
        {
            Height = 40;
            Width = 140;
            Top = 1080 * 90 / 100;
            Left = 1920 - Width;
            FontSize = 20;
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
                _settings = XmlManager<Settings>.Load(path);
            }
        }

        public static void Save()
        {
            string path = Path.Combine(Config.AppDataPath, "reconnector.xml");
            XmlManager<Settings>.Save(path, Instance);
        }
    }
}
