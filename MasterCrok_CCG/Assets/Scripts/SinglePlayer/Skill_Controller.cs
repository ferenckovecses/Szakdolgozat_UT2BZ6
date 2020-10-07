using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class Skill_Controller
	{
		private GameState_Controller gameState;

		public Skill_Controller(GameState_Controller con)
		{
			this.gameState = con;
		}

		public void UseSkill(int skillId)
		{
			switch (skillId)
			{
				case 1: UnexpectedAttack(); break;
				case 2: PlotTwist(); break;
				case 3: SoulStealing(); break;
				case 4: FirstCut(); break;
				case 5: Revive(); break;
				case 6: GreatFight(); break;
				case 7: NoMercy(); break;
				case 8: JungleFight(); break;
				case 9: PowerOfTuition(); break;
				case 16: ShieldFight(); break;

				//Bármilyen hiba esetén passzolunk, hogy ne akadjon meg a játék
				default: this.gameState.Pass(); break;
			}
		}

		//Cserélhet kézből egy nem Master Crok lapot
		private void UnexpectedAttack()
		{

			//Jelezzük a vezérlőnek, hogy cserére készülünk
			gameState.SetSelectionAction(SkillEffectAction.Switch);
			gameState.SetSwitchType(CardListTarget.Hand);

			//Megjelenítjük a kézben lévő lapokat, hogy a játékos választhasson közülük
			ChooseCard(CardListFilter.NoMasterCrok);
		}

		//Megváltoztathatjuk a harc típusát
		private void PlotTwist()
		{
			gameState.StartCoroutine(TriggerStatChange());
		}

		//Használhatod egy másik játékos vesztes krokjának képességét
		private void SoulStealing()
		{
			//Vannak-e ellenséges vesztes kártyák, amik közül választhatunk
			if (gameState.IsThereOtherLostCards())
			{
				gameState.SetSelectionAction(SkillEffectAction.SkillUse);
				gameState.SetSwitchType(CardListTarget.Hand);

				//Ha bot, akkor kell egy kis rásegítés neki
				if (!gameState.IsTheActivePlayerHuman())
				{
					ChooseCard();
				}
			}

			else
			{
				gameState.Pass();
			}
		}

		//Mindenki képességét hatástalanítja/passzoltatja
		private void FirstCut()
		{
			foreach (int key in gameState.GetDataModule().GetKeyList())
            {
                int position = 0;
                foreach (Card card in gameState.GetDataModule().GetCardsFromField(key)) 
                {
                	gameState.GetClientModule().SetSkillState(key, position, SkillState.Pass);
                    position++;
                }
                gameState.GetDataModule().GetPlayerWithKey(key).SetStatus(PlayerTurnStatus.Finished);
            }

            gameState.StartCoroutine(gameState.SkillFinished());
		}

		private void Revive()
		{
			//Van-e feltámasztható vesztes
			if (gameState.DoWeHaveLosers())
			{
				//Jelezzük a vezérlőnek, hogy kártya felvételre készülünk
				gameState.SetSelectionAction(SkillEffectAction.Revive);
				gameState.SetSwitchType(CardListTarget.Losers);

				//Megjelenítjük a kézben lévő lapokat, hogy a játékos választhasson közülük
				ChooseCard();
			}

			else
			{
				gameState.Pass();
			}
		}
		//Ütközet indítása: Felhúz mindenkie +1 lapot a pakli tetejéről
		private void GreatFight()
		{

			foreach (int key in gameState.GetDataModule().GetKeyList())
			{
				if(gameState.GetDataModule().GetPlayerWithKey(key).GetCardsOnField().Count < 4)
				{
					gameState.DrawTheCard(key, DrawTarget.Field, DrawType.Normal, SkillState.Pass);
				}
			}

			gameState.StartCoroutine(gameState.SkillFinished());
		}

		//Egy kiválasztott játékosnak le kell dobnia a kezéből egy lapot.
		private void NoMercy()
		{
			gameState.SetSelectionAction(SkillEffectAction.Execute);
			gameState.SetSwitchType(CardListTarget.Hand);

			//Ha botról van szó
			if(!gameState.IsTheActivePlayerHuman())
			{
				gameState.StartCoroutine(gameState.GetAImodule().CardSelectionEffect(CardListFilter.None, 0));
			}
		}

		//Kicseréli a pályán lévő lapot a pakli legfelső lapjára
		private void JungleFight()
		{
			//Jelezzük a vezérlőnek, hogy cserére készülünk
			gameState.SetSelectionAction(SkillEffectAction.BlindSwitch);
			gameState.SetSwitchType(CardListTarget.Deck);

			SwitchCard(gameState.GetActiveCardID(), 0);
			gameState.StartCoroutine(gameState.SkillFinished());
		}

		//A következő körre ad +1 bónusz minden statra
		private void PowerOfTuition()
		{
			//Csak a late skill fázisban adjuk hozzá, hogy a következő körre legyen érvényes
			if(gameState.GetGameState() == MainGameStates.LateSkills)
			{
				StatBonus bonus = new StatBonus(1,1,1);
				int currentKey = gameState.GetCurrentKey();
				gameState.GetDataModule().GetPlayerWithKey(currentKey).AddBonus(bonus);
			}
			
			gameState.StartCoroutine(gameState.SkillFinished());
		}

		//Erőre változtatja a harc típusát
		private void ShieldFight()
		{
			SetActiveStat(CardStatType.Power);
		}

        #region Skill Actions

        //Jelzünk a játékvezérlőnek, hogy a képesség végzett, mehet a játék tovább
        private void Response()
		{
			gameState.ActionFinished();
		}

		//A megadott paraméterekkel megjelenít egy kártya listát, amiből választhat a játékos
		public void ChooseCard(CardListFilter filter = CardListFilter.None, int limit = 0)
		{

			//Ha ember
			if (gameState.IsTheActivePlayerHuman())
			{
				gameState.StartCoroutine(gameState.GetClientModule().DisplayCardList(filter, limit));
			}

			//Ha bot
			else
			{
				gameState.StartCoroutine(gameState.GetAImodule().CardSelectionEffect(filter, limit));
			}
		}

		//Kicserél egy mezőn lévő lapot egy másikra
		public void SwitchCard(int cardOnField, int cardToSwitch)
		{
			int currentKey = gameState.GetCurrentKey();

			//Ha kézben kell keresni a választott lapot
			if (gameState.GetCurrentListType() == CardListTarget.Hand)
			{
				//Adatok kinyerése
				Card handData = gameState.GetDataModule().GetCardFromHand(currentKey, cardToSwitch);
				Card fieldData = gameState.GetDataModule().GetCardFromField(currentKey, cardOnField);

				//Model frissítése
				gameState.GetDataModule().SwitchFromHand(currentKey, cardOnField, cardToSwitch);

				//UI frissítése
				gameState.GetClientModule().SwitchHandFromField(currentKey, fieldData, cardOnField, handData, 
					cardToSwitch, gameState.IsTheActivePlayerHuman());
			}

			//Ha a pakliból cserélünk lapot
			else if(gameState.GetCurrentListType() == CardListTarget.Deck)
			{
				Card fieldData = gameState.GetDataModule().GetCardFromField(currentKey, cardOnField);
				Card deckData = gameState.GetDataModule().GetCardFromDeck(currentKey, cardToSwitch);

				//Model frissítése
				gameState.GetDataModule().SwitchFromDeck(currentKey, cardOnField, cardToSwitch);

				//UI frissítése
				gameState.GetClientModule().SwitchDeckFromField(currentKey,cardOnField, deckData );
			}
		}

		public void ReviveCard(int cardID)
		{
			int currentKey = gameState.GetCurrentKey();

			//Ha kézben kell keresni a választott lapot
			if (gameState.GetCurrentListType() == CardListTarget.Losers)
			{
				//Adatok kinyerése
				Card cardData = gameState.GetDataModule().GetCardFromLosers(currentKey, cardID);

				//Model frissítése
				gameState.GetDataModule().ReviveLostCard(currentKey, cardID);

				//UI frissítése
				gameState.GetClientModule().DrawNewCard(cardData, currentKey, gameState.IsTheActivePlayerHuman(), DrawType.Normal, DrawTarget.Hand, SkillState.NotDecided);

				int winSize = gameState.GetDataModule().GetWinnerAmount(currentKey);
				int lostSize = gameState.GetDataModule().GetLostAmount(currentKey);
				Sprite win = gameState.GetDataModule().GetLastWinnerImage(currentKey);
				Sprite lost = gameState.GetDataModule().GetLastLostImage(currentKey);

				gameState.GetClientModule().ChangePileText(winSize, lostSize, currentKey, win, lost);
			}
		}

		//Megváltoztatásra kerül az aktív harctípus
		public IEnumerator TriggerStatChange()
		{
			if (gameState.IsTheActivePlayerHuman())
			{
				gameState.ShowStatBox();
				yield return gameState.WaitForEndOfAction();
			}

			else
			{
				gameState.GetAImodule().DecideNewStat();
				gameState.StartCoroutine(gameState.SkillFinished());
			}
		}

		//Megváltoztatja a harc típusát egy megadott értékre
		public void SetActiveStat(CardStatType newStat)
		{
			gameState.SetActiveStat(newStat);
			gameState.GetClientModule().RefreshStatDisplay();
		}

		#endregion


	}
}