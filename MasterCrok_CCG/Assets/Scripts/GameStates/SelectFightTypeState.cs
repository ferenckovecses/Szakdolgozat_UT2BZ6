using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class SelectFightTypeState : GameState
	{
		private int currentKey;

		public SelectFightTypeState(Module_Controller in_module, GameState_Controller in_controller, Interactions in_interactions) : base(in_module, in_controller, in_interactions)
		{
			this.modules = in_module;
			this.controller = in_controller;
			this.interactions = in_interactions;
		}

		public override void Init()
		{
			controller.StartCoroutine(SelectFightType());
		}

		private IEnumerator SelectFightType()
		{
			currentKey = modules.GetDataModule().GetKeyList()[0];
			controller.SetCurrentKey(currentKey);

            //Ha nem vakharc van, akkor húzhat lapot
            if(!controller.GetBlindMatchState())
            {
                //Húzunk fel a játékosnak 1 lapot a kör elején
                controller.StartCoroutine(controller.DrawCardsUp(currentKey));
                yield return controller.WaitForEndOfAction();
                controller.StartCoroutine(controller.CardAmountCheck());
                yield return controller.WaitForEndOfAction();
            }

            //Ha ember, akkor dobjuk fel neki az értékválasztó képernyőt
            if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetPlayerStatus())
            {
                controller.ShowStatBox();

                //Várunk, amíg befejezi a döntést
                yield return controller.WaitForTurnsEnd();
            }

            //Ellenkező esetben az AI dönt
            else
            {
                //AI agy segítségét hívjuk a döntésben
                controller.SetActiveStat(Bot_Behaviour.ChooseFightType(modules.GetDataModule().GetPlayerWithKey(currentKey).GetCardsInHand()));

                //Jelenítsük meg a változást
                modules.GetClientModule().RefreshStatDisplay();
            }

            ChangeState();
		}

		public override void ChangeState()
		{
			controller.ChangePhase(MainGameStates.SummonCard);
		}
	}	
}