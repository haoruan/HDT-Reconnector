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
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;
using HearthDb.Enums;
using BobsBuddy.Simulation;
using BobsBuddy;
using System.Reflection;

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
            Visibility = Core.Game.CurrentGameMode == GameMode.Battlegrounds ? Visibility.Visible : resize.resizeGrip.Visibility;
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
            // last opponent
            int turnNumber = Core.Game.GetTurnNumber() - 1;

            // BobsBuddyInvoker.GetInstance(Core.Game.CurrentGameStats.GameId, turnNumber, false)
            var getInstance = Utils.GetTypeMethod("Hearthstone_Deck_Tracker.BobsBuddy.BobsBuddyInvoker", "GetInstance");
            object[] parameters = { Core.Game.CurrentGameStats.GameId, turnNumber, false};
            object obj = getInstance?.Invoke(null, parameters);
            if (obj == null)
            {
                return;
            }

            var getOrderedMinions = Utils.GetTypeMethod("Hearthstone_Deck_Tracker.BobsBuddy.BobsBuddyUtils", "GetOrderedMinions");
            var getAttachedEntities = Utils.GetKnownTypeMethod(obj.GetType(), "GetAttachedEntities");
            var getMinionFromEntity = Utils.GetTypeMethod("Hearthstone_Deck_Tracker.BobsBuddy.BobsBuddyUtils", "GetMinionFromEntity");
            if (getOrderedMinions == null || 
                getAttachedEntities == null || 
                getMinionFromEntity == null || 
                !UpdatePlayerBoard(obj, getOrderedMinions, getAttachedEntities, getMinionFromEntity))
            {
                return;
            }

            // Instance.BobsBuddyDisplay.SetState(BobsBuddyState.Combat);
            var get_BobsBuddyDisplay = Utils.GetKnownTypeMethod(obj.GetType(), "get_BobsBuddyDisplay");
            BobsBuddyPanel bobsBuddyDisplay = (BobsBuddyPanel)get_BobsBuddyDisplay?.Invoke(obj, null);

            var setState = Utils.GetKnownTypeMethod(typeof(BobsBuddyPanel), "SetState");
            parameters = new object[]{ BobsBuddyState.Combat };
            setState?.Invoke(bobsBuddyDisplay, parameters);

            // Instance.RunAndDisplaySimulationAsync()
            var simulation = Utils.GetKnownTypeMethod(obj.GetType(), "RunAndDisplaySimulationAsync");
            simulation?.Invoke(obj, null);

        }

        private bool UpdatePlayerBoard(object obj,
            MethodInfo getOrderedMinions,
            MethodInfo getAttachedEntities,
            MethodInfo getMinionFromEntity)
        {
            // Get Instance._input
            Input input = (Input)Utils.GetFieldValue(obj, "_input");
            if (input == null)
            {
                return false;
            }

            var playerHero = Core.Game.Player.Board.FirstOrDefault(x => x.IsHero);
            var playerTechLevel = playerHero.GetTag(GameTag.PLAYER_TECH_LEVEL);

            input.DamageCap = Core.Game.GameEntity.GetTag(GameTag.BACON_COMBAT_DAMAGE_CAP);

            input.playerHealth = playerHero.Health + playerHero.GetTag(GameTag.ARMOR);
            input.playerTier = playerTechLevel;

            input.SetPlayerHandSize(Core.Game.Player.HandCount);
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
                // We're not in the shopping stats, or it's our next turn
				return false;
			}

            return true;
        }
    }
}
