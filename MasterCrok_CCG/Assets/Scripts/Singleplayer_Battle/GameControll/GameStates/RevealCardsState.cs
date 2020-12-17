using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class RevealCardsState : GameState
	{
		public RevealCardsState(Module_Controller in_module, GameState_Controller in_controller) : base(in_module, in_controller)
		{
			this.modules = in_module;
            this.controller = in_controller;
		}

		public override void Init()
		{
			controller.StartCoroutine(RevealCards());
		}

		private IEnumerator RevealCards()
		{
			foreach (int key in modules.GetDataModule().GetKeyList())
            {
                modules.GetClientModule().RevealCards(key);
                yield return new WaitForSeconds(GameSettings_Controller.drawTempo * 5);
            }

            ChangeState();
		}

		public override void ChangeState()
		{
			controller.ChangePhase(MainGameStates.QuickSkills);
		}
	}	
}
