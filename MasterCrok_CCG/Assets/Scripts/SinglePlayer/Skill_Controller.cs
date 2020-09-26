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
				case 5: Revive(); break;
				case 6: GreatFight(); break;
				case 8: JungleFight(); break;
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

		private void Revive()
		{
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

		private void GreatFight()
		{
			foreach (int key in gameState.GetDataModule().GetKeyList())
			{
				gameState.DrawTheCard(key, DrawTarget.Field);
			}
		}

		private void JungleFight()
		{
			//Jelezzük a vezérlőnek, hogy cserére készülünk
			gameState.SetSelectionAction(SkillEffectAction.BlindSwitch);
			gameState.SetSwitchType(CardListTarget.Deck);

			BlindSwitch();
		}

		//Erőre változtatja a harc típusát
		private void ShieldFight()
		{
			SetActiveStat(CardStatType.Power);
			Response();
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
				gameState.GetAImodule().CardSelectionEffect(filter, limit);
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
				gameState.GetClientModule().DrawNewCard(cardData, currentKey, gameState.IsTheActivePlayerHuman());

				int winSize = gameState.GetDataModule().GetWinnerAmount(currentKey);
				int lostSize = gameState.GetDataModule().GetLostAmount(currentKey);
				Sprite win = gameState.GetDataModule().GetLastWinnerImage(currentKey);
				Sprite lost = gameState.GetDataModule().GetLastLostImage(currentKey);

				gameState.GetClientModule().ChangePileText(winSize, lostSize, currentKey, win, lost);
			}
		}

		public void BlindSwitch()
		{

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