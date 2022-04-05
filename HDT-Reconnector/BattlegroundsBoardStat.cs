using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Enums;

namespace HDT_Reconnector
{
    internal class BattlegroundsBoardStat
    {
        private Dictionary<string, BoardSnapshot> lastKnownBoardState = new Dictionary<string, BoardSnapshot>();
        private object battlegroundsBoardState = null;

        public void SaveBoardStat()
        {
            if (Core.Game.CurrentGameMode != GameMode.Battlegrounds)
            {
                return;
            }

            battlegroundsBoardState = Utils.GetFieldValue(Core.Game, "_battlegroundsBoardState");
            // Setting read-only automatically-implemented properties can be done through it's backing field
            var obj = (Dictionary<string, BoardSnapshot>)Utils.GetFieldValue(battlegroundsBoardState, "<LastKnownBoardState>k__BackingField");
            // obj is a ref to LastKnownBoardState, so create a shaodow copy here
            lastKnownBoardState = obj.ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        public void RestoreBoardStat()
        {
            if (Core.Game.CurrentGameMode != GameMode.Battlegrounds)
            {
                return;
            }

            if (battlegroundsBoardState != null)
            {
                Utils.SetFieldValue(battlegroundsBoardState, "<LastKnownBoardState>k__BackingField", lastKnownBoardState);
            }
        }
    }
}
