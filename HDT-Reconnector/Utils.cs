using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;
using System.Net;
using System.Reflection;

using Hearthstone_Deck_Tracker.Utility.Logging;
using System.Windows;
using WPFLocalizeExtension.Engine;

namespace HDT_Reconnector
{
    internal class Utils
    {
        public static readonly Point Resolution = new Point(1920, 1080);
        private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>();
        public static bool IsElevated()
        {
            bool isElevated;

            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return isElevated;
        }

        public static string GetLoc(string key, bool upper = false)
		{
			var culture = LocalizeDictionary.Instance.Culture;
			var cacheKey = culture + key;
			if(!Cache.TryGetValue(cacheKey, out var str))
			{
				str = LocalizeDictionary.Instance.GetLocalizedObject("HDT-Reconnector", "Strings", key, culture)?.ToString();
				Cache[cacheKey] = str;
			}
			if(str == null)
				return string.Empty;
			return upper ? str.ToUpper(culture) : str;
		}

        public static DateTime ToDateTime(FILETIME time)
        {
            ulong high = (ulong)time.dwHighDateTime;
            uint low = (uint)time.dwLowDateTime;
            long fileTime = (long)((high << 32) + low);
            try
            {
                return DateTime.FromFileTimeUtc(fileTime);
            }
            catch
            {
                return DateTime.FromFileTimeUtc(0xFFFFFFFF);
            }
        }

        public static uint ConvertFromIpAddressToInteger(string ipAddress)
        {
            var address = IPAddress.Parse(ipAddress);
            byte[] bytes = address.GetAddressBytes();

            // flip big-endian(network order) to little-endian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static object GetFieldValue(object obj, string name) {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return field?.GetValue(obj);
        }

        public static void SetFieldValue(object obj, string name, object value) {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            field?.SetValue(obj, value);
        }

        public static bool PointInsideControl(Point pos, double actualWidth, double actualHeight, Thickness margin)
			=> pos.X > 0 - margin.Left && pos.X < actualWidth + margin.Right && (pos.Y > 0 - margin.Top && pos.Y < actualHeight + margin.Bottom);
    }
}
