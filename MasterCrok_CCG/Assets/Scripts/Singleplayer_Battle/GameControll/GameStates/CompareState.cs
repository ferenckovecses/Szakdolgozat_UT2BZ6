using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GameControll
{
	public class CompareState : GameState
	{
		//Kiértékeléshez szükséges változók
        List<int> values = new List<int>();
        int max;
        int maxId;

		public CompareState(Module_Controller in_module, GameState_Controller in_controller) : base(in_module, in_controller)
		{
			this.modules = in_module;
            this.controller = in_controller;
		}

		public override void Init()
		{
            values.Clear();
            max = 0;
            maxId = -1;
			controller.StartCoroutine(Compare());
		}

		private IEnumerator Compare()
		{
            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                //Hozzáadjuk minden mező értékét
                int playerFieldValue = modules.GetDataModule().GetPlayerWithKey(key).GetActiveCardsValue(controller.GetActiveStat());

                values.Add(playerFieldValue);


                //Eltároljuk a legnagyobb értéket menet közben
                if (playerFieldValue > max)
                {
                    max = playerFieldValue;
                    maxId = key;
                }
            }

            //Ha több ember is rendelkezik a max value-val: Vakharc
            if (values.Count(p => p == max) > 1)
            {
                controller.StartCoroutine(modules.GetClientModule().DisplayNotification("Döntetlen!"));
                yield return controller.WaitForEndOfText();

                controller.SetBlindMatchState(true);
            }

            //Ellenkező esetben eldönthető, hogy ki a győztes
            else
            {
                //Győzelmi üzenet megjelenítése
                controller.StartCoroutine(modules.GetClientModule().DisplayNotification("A kör győztese: " + modules.GetDataModule().GetPlayerName(maxId)));
                yield return controller.WaitForEndOfText();

                //Ha vakharcok voltak, akkor ezzel végetértek
                if (controller.GetBlindMatchState())
                {
                    controller.SetBlindMatchState(false);
                }
            }

            controller.SetCurrentWinnerKey(maxId);

            //Győzelmi státuszok beállítása
            foreach (int key in modules.GetDataModule().GetKeyList())
            {

                //Ha az első helyezettével megegyezik a kulcs és nincs vakharc státusz
                if (key == controller.GetCurrentWinnerKey() && !controller.GetBlindMatchState())
                {
                    modules.GetDataModule().GetPlayerWithKey(key).SetResult(PlayerTurnResult.Win);
                }

                //Ha vakharc lesz, akkor döntetlen volt az eredmény
                else if(controller.GetBlindMatchState())
                {
                    modules.GetDataModule().GetPlayerWithKey(key).SetResult(PlayerTurnResult.Draw);
                }

                //Ellenkező esetben vesztes
                else
                {
                    modules.GetDataModule().GetPlayerWithKey(key).SetResult(PlayerTurnResult.Lose);
                }
            }

            ChangeState();
		}

		public override void ChangeState()
		{
			controller.ChangePhase(MainGameStates.LateSkills);
		}
	}	
}
