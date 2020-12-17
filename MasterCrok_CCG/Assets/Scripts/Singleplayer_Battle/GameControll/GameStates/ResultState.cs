using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ClientSide;

namespace GameControll
{
	public class ResultState : GameState
	{
		//Ha a lapokból kifogyott valamelyik játékos: Legtöbb nyeréssel rendelkező játékos(ok) viszik a győzelmet
        private List<int> winnerCardCount = new List<int>();
        private int maxWin;
        private List<int> maxKeys = new List<int>();
        private int reward;


		public ResultState(Module_Controller in_module, GameState_Controller in_controller) : base(in_module, in_controller)
		{
			this.modules = in_module;
            this.controller = in_controller;
		}

		public override void Init()
		{
			controller.StartCoroutine(Result());
		}

		private IEnumerator Result()
		{
	        maxWin = 0;

            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                List<Card> list = modules.GetDataModule().GetPlayerWithKey(key).GetWinners();

                int res = (from s in list
                        group s by new {s = s.GetCardType()} into g
                        select new {value = g.Distinct().Count()}).Count();

                if (res >= GameSettings_Controller.winsNeeded)
                {
                    winnerCardCount.Add(res);
                    maxWin = res;
                    maxKeys.Add(key);
                }
            }

            yield return new WaitForSeconds(0.1f);

			reward = 0;

			//Kártyák elrakása a modelben
			modules.GetDataModule().ResetDecks();

			//Jutalom kiszámítása
            switch (GameObject.Find("GameData_Controller").GetComponent<GameSettings_Controller>().GetOpponents()) 
            {
                case 1: reward = 50; break;
                case 2: reward = 200; break;
                case 3: reward = 500; break;
                default: break;
            }

            ChangeState();
		}

		public override void ChangeState()
		{

            //Ha holtverseny van
            if (winnerCardCount.Count(p => p >= GameSettings_Controller.winsNeeded) > 1)
            {
                foreach (int key in maxKeys) 
                {
                    //Ha a játékos is a győztesek között van.
                    if(modules.GetDataModule().GetPlayerWithKey(key).GetPlayerStatus())
                    {
                        reward = (reward / maxKeys.Count);
                        GameObject.Find("Profile_Controller").GetComponent<Profile_Controller>().GetActivePlayer().AddCoins(reward);
                        Profile_Controller.settingsState = ProfileSettings.Silent;
                        GameObject.Find("Profile_Controller").GetComponent<Profile_Controller>().SaveProfile();

                    }
                }
                Notification_Controller.DisplayNotification($"Játék Vége!\nAz eredmény döntetlen!\nJutalmad:{reward} Érme", controller.ExitGame);
            }

            //Ha egyértelmű a győzelem
            else
            {
                //Ha a játékos a győztes
                if(modules.GetDataModule().GetPlayerWithKey(maxKeys[0]).GetPlayerStatus())
                {
                    GameObject.Find("Profile_Controller").GetComponent<Profile_Controller>().GetActivePlayer().AddCoins(reward);
                    Profile_Controller.settingsState = ProfileSettings.Silent;
                    GameObject.Find("Profile_Controller").GetComponent<Profile_Controller>().SaveProfile();
                }

                else 
                {
                    reward = 0;
                }

                Notification_Controller.DisplayNotification($"Játék Vége!\nA győztes: {modules.GetDataModule().GetPlayerWithKey(maxKeys[0]).GetUsername()}\nJutalmad:{reward} Érme", controller.ExitGame);
            }
		}
	}	
}
