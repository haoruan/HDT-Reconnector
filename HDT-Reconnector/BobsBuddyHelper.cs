using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core = Hearthstone_Deck_Tracker.API.Core;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;
using Hearthstone_Deck_Tracker.Utility.Logging;
using BobsBuddy.Simulation;

namespace HDT_Reconnector
{
    internal class BobsBuddyHelper
    {
		private static BobsBuddyHelper _instance;
		private Input input;

        public static BobsBuddyHelper Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new BobsBuddyHelper();
				}

				return _instance;
			}
		}

		public static object GetInvokerInstance(bool createInstanceIfNoneFound = false)
        {
			// Get Last opponent
			int turnNumber = Core.Game.GetTurnNumber() - 1;
			if (Core.Game.OpponentEntity.IsCurrentPlayer)
            {
				// We're in the battle, get current opponent
				turnNumber++;
            }

			// BobsBuddyInvoker.GetInstance(Core.Game.CurrentGameStats.GameId, turnNumber, false)
            var getInstance = Utils.GetTypeMethod("Hearthstone_Deck_Tracker.BobsBuddy.BobsBuddyInvoker", "GetInstance");
            object[] parameters = { Core.Game.CurrentGameStats.GameId, turnNumber, createInstanceIfNoneFound};
            object obj = getInstance?.Invoke(null, parameters);
            if (obj == null)
            {
				Log.Error("BobsBuddyInovker instance not found");
				return null;
            }

			return obj;
        }

		public void SaveInput()
        {
			Log.Info("Saving BobsBuddy Input");
			var obj = GetInvokerInstance();
			if (obj == null) {
				return;
            }

			input = (Input)Utils.GetFieldValue(obj, "_input");
        }

		public void RestoreInput()
        {
			Log.Info("Restoring BobsBuddy Input");
			var obj = GetInvokerInstance(true);
            if (obj == null)
            {
				return;
            }
			Utils.SetFieldValue(obj, "_input", input);
        }
    }
}
