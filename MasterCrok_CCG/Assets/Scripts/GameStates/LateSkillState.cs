using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class LateSkillState : GameState
	{
		private int currentKey;

		public LateSkillState(Module_Controller in_module, GameState_Controller in_controller, Interactions in_interactions) : base(in_module, in_controller, in_interactions)
		{
			this.modules = in_module;
            this.controller = in_controller;
            this.interactions = in_interactions;
		}

		public override void Init()
		{
			controller.StartCoroutine(LateSkills());
		}

		private IEnumerator LateSkills()
		{
			foreach (int key in controller.GetLateSkillKeys())
            {
                currentKey = key;
                controller.SetCurrentKey(currentKey);

                yield return new WaitForSeconds(0.1f);
            }

            ChangeState();
		}

		public override void ChangeState()
		{
			controller.ChangePhase(MainGameStates.PutCardsAway);
		}
	}	
}
