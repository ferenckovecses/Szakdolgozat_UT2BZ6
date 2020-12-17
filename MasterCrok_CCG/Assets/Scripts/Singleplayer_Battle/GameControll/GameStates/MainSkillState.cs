using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class MainSkillState : GameState
	{
        private bool everyoneDecided;
    	private int playerMadeDecision;
    	private int currentKey;


		public MainSkillState(Module_Controller in_module, GameState_Controller in_controller) : base(in_module, in_controller)
		{
			this.modules = in_module;
            this.controller = in_controller;
		}

		public override void Init()
		{
        	controller.StartCoroutine(Skills());   
		}

		private IEnumerator Skills()
		{
            yield return new WaitForEndOfFrame();
            
			everyoneDecided = false;
			
            //A lateSkill kulcsok listáját kitakarítjuk
            controller.ClearLateSkills();

            controller.SetCurrentAction(SkillEffectAction.None);

            //Addig megyünk, amíg mindenki el nem használja vagy passzolja a képességét
            while (!everyoneDecided)
            {
                playerMadeDecision = 0;
                foreach (int playerKey in modules.GetDataModule().GetKeyList())
                {
                    currentKey = playerKey;
                    controller.SetCurrentKey(currentKey);
                    controller.NewSkillCycle(currentKey);

                    Debug.Log($"Current Player: {currentKey}");

                    //Ha ember, akkor várunk a döntésre
                    if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetPlayerStatus())
                    {
                        //Jelezzük a játékosnak, hogy itt az idő dönteni a képességről
                        controller.StartCoroutine(modules.GetClientModule().DisplayNotification("Dönts a kártyáid képességeiről!"));
                        yield return controller.WaitForEndOfText();

                        //Megjelenítjük a kör vége gombot
                        modules.GetClientModule().SetEndTurnButton(true);

                        //Megadjuk a lehetőséget a játékosnak, hogy döntsön a képességekről most
                        modules.GetClientModule().SetSkillStatus(currentKey, true);

                        //Várunk a visszajelzésére
                        yield return controller.WaitForTurnsEnd();

                        //Ha döntött, akkor elvesszük tőle a lehetőséget, hogy módosítson rajta
                        modules.GetClientModule().SetSkillStatus(currentKey, false);

                        //Az esetleges nem rendelkezett lapokat passzoljuk
                        modules.GetClientModule().HandleRemainingCards(currentKey);
                    }

                    //Ellenkező esetben az AI dönt a képességről
                    else
                    {
                        //Nem megyünk tovább, amíg nem végez a döntéssel
                        yield return controller.WaitForTurnsEnd();
                    }

                    controller.SetCurrentAction(SkillEffectAction.None);

                    if(controller.GetInstantWinStatus())
                    {
                    	controller.SetInstantWinStatus(false);
                        yield break;
                    }

                    //Ha az aktuális játékos végzett, növeljük a countert
                    if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetStatus() == PlayerTurnStatus.Finished)
                    {
                        playerMadeDecision++;
                    }
                }

                //Ha mindenki végzett, akkor loop vége
                if (playerMadeDecision == modules.GetDataModule().GetNumberOfOpponents() + 1)
                {
                    everyoneDecided = true;
                }
            }

            ChangeState();
		}

		public override void ChangeState()
		{
			controller.ChangePhase(MainGameStates.CompareCards);
		}
	}	
}
