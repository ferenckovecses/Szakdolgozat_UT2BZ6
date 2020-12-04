using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class QuickSkillState : GameState
	{
		private int currentKey;

		public QuickSkillState(Module_Controller in_module, GameState_Controller in_controller, Interactions in_interactions) : base(in_module, in_controller, in_interactions)
		{
			this.modules = in_module;
            this.controller = in_controller;
            this.interactions = in_interactions;
		}

		public override void Init()
		{
			controller.StartCoroutine(QuickSkills());
		}

		private IEnumerator QuickSkills()
		{
			foreach (int key in modules.GetDataModule().GetKeyList())
            {
                currentKey = key;
                controller.SetCurrentKey(currentKey);

                int position = 0;
                foreach (Card card in modules.GetDataModule().GetCardsFromPlayer(currentKey, CardListTarget.Field)) 
                {
                    //Ha a megadott játékosnál van a mezőn gyors skill és még eldöntetlen/felhasználható
                    if(card.HasAQuickSkill() && modules.GetClientModule().AskCardSkillStatus(key, position) == SkillState.NotDecided)
                    {
                        controller.SetActiveCardID(position);
                        controller.Use(position);
                        yield return controller.WaitForEndOfSkill();
                    }
                    
                    position++;

                }

                if(controller.GetNegationStatus())
                {
                    controller.SetNegationStatus(false);
                    break;
                }
            }

            ChangeState();
		}

		public override void ChangeState()
		{
			controller.ChangePhase(MainGameStates.NormalSkills);
		}
	}	
}
