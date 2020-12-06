using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;


namespace GameControll
{
	public class OrderChangeState : GameState
	{
		private bool firstRound;

		public OrderChangeState(Module_Controller in_module, GameState_Controller in_controller, Interactions in_interactions) : base(in_module, in_controller, in_interactions)
		{
			this.modules = in_module;
			this.controller = in_controller;
			this.interactions = in_interactions;
		}

		public override void Init()
		{
			modules.GetDataModule().CreateRandomOrder();

            //A legelső körnél nincs szükség különleges sorrendre
            if (controller.GetFirstRoundStatus())
            {
            	controller.SetFirstRoundStatus(false);
            	firstRound = true;

                ChangeState();
            }

            //A többi esetben az előző meccs nyertese lesz az új támadó, azaz az első a listában
            else
            {
            	firstRound = false;
                //Valós érték vizsgálat
                if (controller.GetCurrentWinnerKey() != -1)
                {
                    modules.GetDataModule().PutWinnerInFirstPlace(controller.GetCurrentWinnerKey());
                }

                else 
                {
                	Notification_Controller.DisplayNotification("Hiba történt! Visszatérés a Főmenübe!", controller.ExitGame);	
                }
            }

            ChangeState();
		}

		public override void ChangeState()
		{
			if(firstRound)
			{
				controller.ChangePhase(MainGameStates.StarterDraw);
			}

			else 
			{
				controller.ChangePhase(MainGameStates.SetRoles);
			}
		}
	}	
}

