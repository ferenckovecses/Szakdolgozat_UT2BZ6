using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class PreparationState : GameState
	{
		public PreparationState(Module_Controller in_module, GameState_Controller in_controller, Interactions in_interactions) : base(in_module, in_controller, in_interactions)
		{
			this.modules = in_module;
			this.controller = in_controller;
			this.interactions = in_interactions;
		}

		public override void Init()
		{
			modules.GetDataModule().AddPlayer();
            modules.GetDataModule().GenerateOpponents();
            modules.GetClientModule().GenerateUI(modules.GetDataModule().GetNumberOfOpponents(), modules.GetDataModule().GetKeyList(), modules.GetDataModule().GetNameList(), modules.GetDataModule().GetPlayerAtIndex(0).GetDeckSize());

            ChangeState();
		}

		public override void ChangeState()
		{
			controller.ChangePhase(MainGameStates.CreateOrder);
		}
	}	
}
