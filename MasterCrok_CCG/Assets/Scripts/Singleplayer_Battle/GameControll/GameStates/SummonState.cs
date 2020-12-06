using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class SummonState : GameState
	{
		private int currentKey;
		private bool gameOver;

		public SummonState(Module_Controller in_module, GameState_Controller in_controller, Interactions in_interactions) : base(in_module, in_controller, in_interactions)
		{
			this.modules = in_module;
			this.controller = in_controller;
			this.interactions = in_interactions;
		}

		public override void Init()
		{
			controller.StartCoroutine(Summon());
		}

		private IEnumerator Summon()
		{
			gameOver = false;
			//Minden játékoson végigmegy
            foreach (int playerKey in modules.GetDataModule().GetKeyList())
            {
                currentKey = playerKey;
                controller.SetCurrentKey(currentKey);

                //Ha a játékosnak már nincs idézhető lapja: játék vége
                if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetHandCount() == 0)
                {
                    controller.StartCoroutine(modules.GetClientModule().DisplayNotification(modules.GetDataModule().GetPlayerWithKey(currentKey).GetUsername() + " kifogyott a lapokból!\nJáték vége!"));
                    yield return controller.WaitForEndOfText();
                    gameOver = true;
                    ChangeState();
                }

                //Csak akkor kell kézből idézni, ha nincs vakharc
                if (!controller.GetBlindMatchState())
                {
                    //Értesítést adunk a kör kezdetéről
                    controller.StartCoroutine(modules.GetClientModule().DisplayNotification(modules.GetDataModule().GetPlayerWithKey(currentKey).GetUsername() + " következik!"));
                    yield return controller.WaitForEndOfText();

                    modules.GetClientModule().WaitForSummon();

                    //A támadó az előző fázisban már húzott lapot, de a védők húzhatnak a kör elején
                    if(modules.GetDataModule().GetPlayerWithKey(currentKey).GetRole() == PlayerTurnRole.Defender)
                    {   
                        //Húzunk fel a játékosnak 1 lapot a kör elején
                        controller.StartCoroutine(controller.DrawCardsUp(currentKey));
                        yield return controller.WaitForEndOfAction();
                        controller.StartCoroutine(controller.CardAmountCheck());
                        yield return controller.WaitForEndOfAction();
                    }

                    //Ha a játékos státusza szerint a kártyahúzásra vár
                    if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetStatus() == PlayerTurnStatus.ChooseCard)
                    {

                        //Engedélyezzük neki a kártyák dragelését
                        modules.GetClientModule().SetDragStatus(currentKey, true);

                        //Ha ember, akkor adunk jelzést neki
                        if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetPlayerStatus())
                        {
                            controller.StartCoroutine(modules.GetClientModule().DisplayNotification("Rakj le egy kártyát, " + modules.GetDataModule().GetPlayerWithKey(currentKey).GetUsername()));
                            yield return controller.WaitForEndOfText();
                        }

                        //Várunk a visszajelzésére
                        yield return controller.WaitForTurnsEnd();
                    }
                }

                //Átállítjuk a játékos státuszát: A képesség kiválasztására vár
                modules.GetDataModule().GetPlayerWithKey(playerKey).SetStatus(PlayerTurnStatus.ChooseSkill);
            }

            ChangeState();
		}

		public override void ChangeState()
		{
			//Ha elfogytak a lapok és vége a játéknak
			if(gameOver)
			{
				controller.ChangePhase(MainGameStates.CreateResult);
			}

			else 
			{
				controller.ChangePhase(MainGameStates.RevealCards);	
			}
		}
	}	
}
