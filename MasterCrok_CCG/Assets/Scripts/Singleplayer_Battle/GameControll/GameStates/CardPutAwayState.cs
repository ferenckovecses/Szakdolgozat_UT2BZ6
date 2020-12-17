using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GameControll
{
	public class CardPutAwayState : GameState
	{
		public CardPutAwayState(Module_Controller in_module, GameState_Controller in_controller) : base(in_module, in_controller)
		{
			this.modules = in_module;
            this.controller = in_controller;
		}

		public override void Init()
		{
			controller.StartCoroutine(PutCardsAway());
		}

		private IEnumerator PutCardsAway()
		{
			foreach (int key in modules.GetDataModule().GetKeyList())
            {
                //Ha az első helyezettével megegyezik a kulcs, akkor győztes
                if (modules.GetDataModule().GetPlayerWithKey(key).GetResult() == PlayerTurnResult.Win)
                {
                    //Lapok elrakása a győztesek közé a UI-ban
                    controller.StartCoroutine(modules.GetClientModule().PutCardsAway(key, true));
                }

                //Ha nem győztes, akkor vesztes.
                else
                {
                    //Lapok elrakása a vesztesek közé a UI-ban
                    controller.StartCoroutine(modules.GetClientModule().PutCardsAway(key, false));
                    
                }
                yield return controller.WaitForEndOfAction();

                //Player modell frissítése, aktív kártya field elemeinek elrakása a győztes vagy vesztes tárolóba
                modules.GetDataModule().GetPlayerWithKey(key).PutActiveCardAway();

                //Továbblépés előtt nullázzuk a kézben lévő lapok bónuszait, hogy tiszta lappal kezdődjön a következő kör
                modules.GetDataModule().GetPlayerWithKey(key).ResetBonuses();

                //Csökkentjük a bónuszok számlálóját és megválunk a lejárt bónuszoktól
                modules.GetDataModule().GetPlayerWithKey(key).UpdateBonuses();
		}

		ChangeState();
	}

		public override void ChangeState()
		{
			//Ha a következő kör vakharc kell hogy legyen.
            if(controller.GetBlindMatchState())
            {
                controller.ChangePhase(MainGameStates.BlindMatch);
            }

            //Megnézzük, hogy van-e a győzelmi feltételeknek megfelelő játékos
            else if (controller.CheckForWinners())
            {
                controller.ChangePhase(MainGameStates.CreateResult);
            }

            //Ha nem, akkor megy minden tovább a következő körrel
            else
            {
                //Ha történt sorrendváltoztatás
                if(controller.GetOrderChangeStatus())
                {
                	controller.HandleOrderChange();
                    controller.SetOrderChangeStatus(false);
                    controller.ChangePhase(MainGameStates.CreateOrder);
                }

                //Normál esetben pedig
                else 
                {
                    //A győztes kulccsal rendelkező játékos lesz a következő támadó
                    controller.ChangePhase(MainGameStates.CreateOrder);
                }

            }
		}
	}	
}
