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

                // Player won (opponent fainted) and this is a gym leader battle
                if (battle.PlayerHP > 0 && battle.IsGymLeader)
                {
                    var save = SaveManager.Instance?.CurrentSave;
                    if (save != null && !save.Badges.Contains(battle.GymBadge))
                    {
                        save.Badges.Add(battle.GymBadge);
                        SaveManager.Instance.SaveToDisk();
                    }

                    await battle.PlayVictoryDialogueAsync();
                }
                
                await Task.Delay(2000);
                battle.EndBattle(1);
            }
        }
    }
}
