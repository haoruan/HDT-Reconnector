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
using System.Windows.Shapes;

using Core = Hearthstone_Deck_Tracker.API.Core;
using Hearthstone_Deck_Tracker.BobsBuddy;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;
using HearthDb.Enums;
using BobsBuddy.Simulation;
using BobsBuddy;
using System.Reflection;
using static HearthDb.CardIds;

namespace HDT_Reconnector
{
    /// <summary>
    /// Interaction logic for SimulatePanel.xaml
    /// </summary>
    public partial class SimulatePanel : UserControl, IDisposable
    {
        private RoutedEventHandler updatePositionHandler;
        private Resize resize;
        private Brush oriBrush;

        public SimulatePanel()
        {
            InitializeComponent();
            Settings.Load();

            resize = new Resize(this, SimulateButton, SimulateText, Settings.Instance.simulate);

            Visibility = Visibility.Collapsed;
            oriBrush = SimulateButton.Background;

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
            Settings.Instance.simulate = resize.settings;
            Settings.Save();
            resize.RemoveFromOverlayWindowPrivate();
            Core.OverlayCanvas.RemoveHandler(SizeChangedEvent, updatePositionHandler);
        }

        public void OnUpdate()
        {
            Visibility = !Core.Game.IsInMenu && Core.Game.IsBattlegroundsMatch ?
                Visibility.Visible : resize.resizeGrip.Visibility;
        }

        private void SimulateButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAbleToSimulate())
            {
                RunSimulation();
            }
        }

        private void SimulateButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsAbleToSimulate())
            {
                SimulateButton.Background = new SolidColorBrush(Color.FromRgb(0x2E, 0x34, 0x38));
            }
        }

        private void SimulateButton_MouseLeave(object sender, MouseEventArgs e)
        {
            SimulateButton.Background = oriBrush;
        }

        private void RunSimulation()
        {
            var obj = BobsBuddyHelper.GetInvokerInstance();
            if (obj == null)
            {
                // Try restore the input from last reconnect
                BobsBuddyHelper.Instance.RestoreInput();
                obj = BobsBuddyHelper.GetInvokerInstance();
                if (obj == null)
                {
                    return;
                }
            }

            var getOrderedMinions = Utils.GetTypeMethod("Hearthstone_Deck_Tracker.BobsBuddy.BobsBuddyUtils", "GetOrderedMinions");
            var getAttachedEntities = Utils.GetKnownTypeMethod(obj.GetType(), "GetAttachedEntities");
            var getMinionFromEntity = Utils.GetTypeMethod("Hearthstone_Deck_Tracker.BobsBuddy.BobsBuddyUtils", "GetMinionFromEntity");
            var wasHeroPowerActivated = Utils.GetTypeMethod("Hearthstone_Deck_Tracker.BobsBuddy.BobsBuddyUtils", "WasHeroPowerActivated");
            if (getOrderedMinions == null || 
                getAttachedEntities == null || 
                getMinionFromEntity == null || 
                wasHeroPowerActivated == null || 
                !UpdatePlayerBoard(obj, getOrderedMinions, getAttachedEntities, getMinionFromEntity, wasHeroPowerActivated))
            {
                return;
            }

            // Instance.BobsBuddyDisplay.SetState(BobsBuddyState.Combat);
            var get_BobsBuddyDisplay = Utils.GetKnownTypeMethod(obj.GetType(), "get_BobsBuddyDisplay");
            BobsBuddyPanel bobsBuddyDisplay = (BobsBuddyPanel)get_BobsBuddyDisplay?.Invoke(obj, null);

            var setState = Utils.GetKnownTypeMethod(typeof(BobsBuddyPanel), "SetState");
            var parameters = new object[]{ BobsBuddyState.Combat };
            setState?.Invoke(bobsBuddyDisplay, parameters);

            // Instance.RunAndDisplaySimulationAsync()
            var simulation = Utils.GetKnownTypeMethod(obj.GetType(), "RunAndDisplaySimulationAsync");
            simulation?.Invoke(obj, null);

        }

        // The same code copied from BobsBuddyInvoker.SnapshotBoardState to set player board
        private bool UpdatePlayerBoard(object obj,
            MethodInfo getOrderedMinions,
            MethodInfo getAttachedEntities,
            MethodInfo getMinionFromEntity,
            MethodInfo wasHeroPowerActivated)
        {
            // Get Instance._input
            Input input = (Input)Utils.GetFieldValue(obj, "_input");
            if (input == null)
            {
                BobsBuddyHelper.Instance.RestoreInput();
                input = (Input)Utils.GetFieldValue(obj, "_input");
                if (input == null)
                {
                    Log.Error("input is null");
                    return false;
                }
            }

            var playerHero = Core.Game.Player.Board.FirstOrDefault(x => x.IsHero);
            if (playerHero == null)
            {
                Log.Error("playerHero is null");
                return false;
            }

            var playerTechLevel = playerHero.GetTag(GameTag.PLAYER_TECH_LEVEL);

            input.availableRaces = BattlegroundsUtils.GetAvailableRaces(Core.Game.CurrentGameStats.GameId).ToList();
            input.DamageCap = Core.Game.GameEntity.GetTag(GameTag.BACON_COMBAT_DAMAGE_CAP);

            input.playerHealth = playerHero.Health + playerHero.GetTag(GameTag.ARMOR);
            input.playerTier = playerTechLevel;

            var playerHeroPower = Core.Game.Player.Board.FirstOrDefault(x => x.IsHeroPower);
			var pHpData = playerHeroPower?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1) ?? 0;
			if(playerHeroPower?.CardId == NonCollectible.Neutral.TeronGorefiend_RapidReanimation)
			{
				var ench = Core.Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == NonCollectible.Neutral.TeronGorefiend_ImpendingDeath && (x.IsInPlay || x.IsInSetAside))
						?? Core.Game.Player.Graveyard.LastOrDefault(x => x.CardId == NonCollectible.Neutral.TeronGorefiend_ImpendingDeath);
				var target = ench?.GetTag(GameTag.ATTACHED) ?? 0;
				if(target > 0)
					pHpData = target;
			}
			input.SetPlayerHeroPower(playerHeroPower?.CardId ?? "", (bool)wasHeroPowerActivated.Invoke(null, new object[] { playerHeroPower }), pHpData);

            foreach(var quest in Core.Game.Player.Quests)
			{
				var rewardDbfId = quest.GetTag(GameTag.QUEST_REWARD_DATABASE_ID);
				var reward = Database.GetCardFromDbfId(rewardDbfId, false);
				input.PlayerQuests.Add(new QuestData()
				{
					QuestProgress = quest.GetTag(GameTag.QUEST_PROGRESS),
					QuestProgressTotal = quest.GetTag(GameTag.QUEST_PROGRESS_TOTAL),
					QuestCardId = quest.CardId ?? "",
					RewardCardId = reward?.Id ?? ""
				});
			}

			foreach(var reward in Core.Game.Player.QuestRewards)
			{
				input.PlayerQuests.Add(new QuestData()
				{
					RewardCardId = reward.Info.LatestCardId ?? ""
				});
			}

            input.SetupSecretsFromDbfidList(Core.Game.Player.Secrets.Select(x => x.Card.DbfId).ToList(), true);

            var playerSide = ((IOrderedEnumerable<Entity>)getOrderedMinions
                .Invoke( null, new object[] { Core.Game.Player.Board }))
				.Where(e => e.IsControlledBy(Core.Game.Player.Id))
				.Select(e => (Minion)getMinionFromEntity.Invoke(null,
                                new object[] {
                                    input.simulator.MinionFactory,
                                    true,
                                    e,
                                    getAttachedEntities.Invoke(obj, new object[] { e.Id })
                                }
                                )
                );
            input.playerSide.Clear();
			foreach(var m in playerSide)
				input.playerSide.Add(m);

            foreach(var e in Core.Game.Player.Hand)
			{
				if(e.IsMinion)
					input.PlayerHand.Add(new MinionCardEntity((Minion)getMinionFromEntity.Invoke(null,
                                new object[] {
                                    input.simulator.MinionFactory,
                                    true,
                                    e,
                                    getAttachedEntities.Invoke(obj, new object[] { e.Id })
                                }
                                ), null));
				else if(e.CardId == NonCollectible.Neutral.BloodGem1)
					input.PlayerHand.Add(new BloodGem(null));
				else if(e.IsSpell)
					input.PlayerHand.Add(new SpellCardEntity(null));
				else
					input.PlayerHand.Add(new CardEntity(e.CardId ?? "", null)); // Not Unknown
			}

            var playerAttached = (IEnumerable<Entity>)getAttachedEntities.Invoke(obj, new object[] { Core.Game.PlayerEntity.Id });
			var pEternalLegion = playerAttached.FirstOrDefault(x => x.CardId == NonCollectible.Invalid.EternalKnight_EternalKnightPlayerEnchant);
			if(pEternalLegion != null)
				input.PlayerEternalKnightCounter = pEternalLegion.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			var pUndeadBonus = playerAttached.FirstOrDefault(x => x.CardId == NonCollectible.Neutral.NerubianDeathswarmer_UndeadBonusAttackPlayerEnchantDnt);
			if(pUndeadBonus != null)
				input.PlayerUndeadAttackBonus = pUndeadBonus.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);

            input.PlayerBloodGemAtkBuff = Core.Game.PlayerEntity.GetTag(GameTag.BACON_BLOODGEMBUFFATKVALUE);
			input.PlayerBloodGemHealthBuff = Core.Game.PlayerEntity.GetTag(GameTag.BACON_BLOODGEMBUFFHEALTHVALUE);

            // Set Instance._input
            Utils.SetFieldValue(obj, "_input", input);

            return true;
        }

        private bool IsAbleToSimulate()
        {
            int turnNumber = Core.Game.GetTurnNumber() - 1;
            
			if (turnNumber < 1 ||
                Core.Game.OpponentEntity == null ||
                Core.Game.OpponentEntity.IsCurrentPlayer ||
                Core.Game.CurrentGameStats == null)
			{
                // It's our first turn, or we're not in the shopping stats
				return false;
			}

            return true;
        }
    }
}
