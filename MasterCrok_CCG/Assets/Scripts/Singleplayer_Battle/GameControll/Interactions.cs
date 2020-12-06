using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameControll
{
	public class Interactions
	{
		private GameState_Controller controller;
		private Module_Controller modules;

		public Interactions(GameState_Controller in_controller, Module_Controller in_module)
		{
			this.controller = in_controller;
			this.modules = in_module;
		}

		//Kártya felhúzás kör elején
		public void DrawCard(int currentKey, int amount = 1,  DrawTarget target = DrawTarget.Hand, DrawType drawType = DrawType.Normal, SkillState newState = SkillState.NotDecided)
		{
			controller.StartCoroutine(DrawCardAction(currentKey, amount, target, drawType, newState));
		}

		private IEnumerator DrawCardAction(int currentKey, int amount, DrawTarget target, DrawType drawType, SkillState newState)
		{
            for (var i = 0; i < amount; i++)
            {
                DrawTheCard(currentKey, target, drawType, newState);
                yield return new WaitForSeconds(GameSettings_Controller.drawTempo);
            }

            controller.ActionFinished();
		}

        //Kártya húzás: Modellből adatok lekérése és továbbítása a megfelelő játékos oldalnak.
        private void DrawTheCard(int key, DrawTarget target, DrawType drawType, SkillState newState)
        {
            Card cardData = null;

            if (target == DrawTarget.Hand && !controller.GetBlindMatchState())
            {
                cardData = modules.GetDataModule().GetPlayerWithKey(key).DrawCardFromDeck();
            }

            else if (controller.GetBlindMatchState() || target == DrawTarget.Field)
            {
                cardData = modules.GetDataModule().GetPlayerWithKey(key).BlindDraw();
            }

            //Ha a felhúzás sikeres
            if (cardData != null)
            {
                modules.GetClientModule().DrawNewCard(cardData, key, modules.GetDataModule().GetPlayerWithKey(key).GetPlayerStatus(), drawType, target, newState);

                //Ha a játékosról van szó továbbítjuk megjelenítésre a deck új méretét
                if (modules.GetDataModule().GetPlayerWithKey(key).GetPlayerStatus())
                {
                    int remainingDeckSize = modules.GetDataModule().GetPlayerWithKey(key).GetDeckSize();
                    modules.GetClientModule().RefreshDeckSize(key, remainingDeckSize);
                }
            }
        }

        public void CheckCardsInHand()
        {
        	controller.StartCoroutine(HandCheckAction());
        }

        private IEnumerator HandCheckAction()
        {
        	int currentKey = controller.GetCurrentKey();

            //Ha több mint 7 lap van a kezünkben, le kell dobni egy szabadon választottat.
            if(modules.GetDataModule().GetCardsFromPlayer(currentKey, CardListTarget.Hand).Count > 7)
            {
            	controller.SetCurrentAction(SkillEffectAction.TossCard);
            	controller.SetSwitchType(CardListTarget.Hand);

                modules.GetSkillModule().MakePlayerChooseCard();
                yield return new WaitForSeconds((GameSettings_Controller.drawTempo)/2);
                controller.ActionFinished();  
            }

            else 
            {
                yield return new WaitForSeconds((GameSettings_Controller.drawTempo)/2);
                controller.ActionFinished();    
            }
        }

		public void ShowFightTypeWindow()
		{
			//Feldobjuk a játékosnak a harctípus választó képernyőt
			controller.StartCoroutine(modules.GetClientModule().DisplayStatBox());
		}

		//Ingame notification megjelenítése
		public void DisplayTextOnScreen(string msg)
		{
			controller.StartCoroutine(TextDisplayAction(msg));
		}

		private IEnumerator TextDisplayAction(string msg)
		{
			controller.StartCoroutine(modules.GetClientModule().DisplayNotification(msg));
            yield return controller.WaitForEndOfText();
		}
	}
}


