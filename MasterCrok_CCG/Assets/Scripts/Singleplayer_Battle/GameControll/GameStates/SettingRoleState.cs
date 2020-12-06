using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class SettingRoleState : GameState
	{
		public SettingRoleState(Module_Controller in_module, GameState_Controller in_controller, Interactions in_interactions) : base(in_module, in_controller, in_interactions)
		{
			this.modules = in_module;
			this.controller = in_controller;
			this.interactions = in_interactions;
		}

		public override void Init()
		{
			bool firstPlayer = true;
            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                if (firstPlayer)
                {
                    firstPlayer = false;
                    modules.GetDataModule().GetPlayerWithKey(key).SetRole(PlayerTurnRole.Attacker);
                }

                else
                {
                    modules.GetDataModule().GetPlayerWithKey(key).SetRole(PlayerTurnRole.Defender);
                }

                modules.GetDataModule().GetPlayerWithKey(key).SetStatus(PlayerTurnStatus.ChooseCard);

                //A kézben lévő lapokra érvényesítjük a bónuszokat, így az a következő fázisban már látható hatással bír
                modules.GetDataModule().GetPlayerWithKey(key).ApplyFieldBonusToAll();

                modules.GetClientModule().StartOfRound();
            }

            ChangeState();
            
		}

		public override void ChangeState()
		{
			controller.ChangePhase(MainGameStates.SetStat);
		}
	}	
}