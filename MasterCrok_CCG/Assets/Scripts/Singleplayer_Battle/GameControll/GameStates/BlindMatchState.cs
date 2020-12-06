using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class BlindMatchState : GameState
	{
		public BlindMatchState(Module_Controller in_module, GameState_Controller in_controller, Interactions in_interactions) : base(in_module, in_controller, in_interactions)
		{
			this.modules = in_module;
            this.controller = in_controller;
            this.interactions = in_interactions;
		}

		public override void Init()
		{
			controller.StartCoroutine(BlindMatch());
		}

		private IEnumerator BlindMatch()
		{
			controller.StartCoroutine(modules.GetClientModule().DisplayNotification("Vakharc következik!"));
            yield return controller.WaitForEndOfText();

            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                //A pakli felső lapját lerakjuk
                controller.StartCoroutine(controller.DrawCardsUp(key, 1, DrawTarget.Field, DrawType.Blind, SkillState.NotDecided));
            }

            //Ha nincs sorrend változtatás
            if(!controller.GetOrderChangeStatus())
            {
            	//Az előző körben lévő utolsó védő lesz a következő támadó
            	controller.SetCurrentWinnerKey(modules.GetDataModule().GetKeyList()[modules.GetDataModule().GetKeyList().Count - 1]);
            }

            //Ha egy képesség miatt változott, hogy ki kezd és választ típust a következő körben
            else 
            {
            	controller.SetCurrentWinnerKey(controller.GetNewAttacker());
            	controller.SetOrderChangeStatus(false);
            }

            ChangeState();
		}

		public override void ChangeState()
		{
			controller.ChangePhase(MainGameStates.CreateOrder);
		}
	}	
}
