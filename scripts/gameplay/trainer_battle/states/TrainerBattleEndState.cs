using Godot;
using System;
using System.Threading.Tasks;
using Game.Utilities;

namespace Game.Gameplay
{
    public partial class TrainerBattleEndState : State
    {
        private TrainerBattleStateMachine _battleSM;

        public override void _Ready()
        {
            _battleSM = StateMachine as TrainerBattleStateMachine;
        }

        public override async void EnterState()
        {
            base.EnterState();
            
            if (_battleSM != null && _battleSM.Battle != null)
            {
                var battle = _battleSM.Battle;
                battle.UpdateLog("The battle has ended!");

                // Player won (opponent fainted)
                if (battle.PlayerHP > 0)
                {
                    var save = SaveManager.Instance?.CurrentSave;
                    
                    // Handle gym leader battles
                    if (battle.IsGymLeader)
                    {
                        if (save != null && !save.Badges.Contains(battle.GymBadge))
                        {
                            save.Badges.Add(battle.GymBadge);
                            SaveManager.Instance.SaveToDisk();
                        }

                        await battle.PlayVictoryDialogueAsync();
                    }
                    // Handle regular trainer battles
                    else if (Game.Core.BattleManager.Instance?.CurrentBattleConfig != null)
                    {
                        var config = Game.Core.BattleManager.Instance.CurrentBattleConfig;
                        
                        // Mark trainer as defeated if they have a TrainerID
                        if (save != null && !string.IsNullOrEmpty(config.TrainerID))
                        {
                            if (!save.DefeatedTrainers.Contains(config.TrainerID))
                            {
                                save.DefeatedTrainers.Add(config.TrainerID);
                                SaveManager.Instance.SaveToDisk();
                            }
                        }
                    }
                }
                
                await Task.Delay(2000);
                battle.EndBattle(1);
            }
        }
    }
}
