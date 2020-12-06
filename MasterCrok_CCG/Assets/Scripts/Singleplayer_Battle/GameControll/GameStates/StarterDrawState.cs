using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class StarterDrawState : GameState
	{
		public StarterDrawState(Module_Controller in_module, GameState_Controller in_controller, Interactions in_interactions) : base(in_module, in_controller, in_interactions)
		{
			this.modules = in_module;
            this.controller = in_controller;
            this.interactions = in_interactions;
		}

		public override void Init()
		{
            //Paklik megkeverése
            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                modules.GetDataModule().ShuffleDeck(key);
            }

            //Kezdő 4 lap felhúzása
            controller.StartCoroutine(controller.DrawStarterCards());
		}

		public override void ChangeState()
		{
			controller.ChangePhase(MainGameStates.SetRoles);
		}
	}	
}