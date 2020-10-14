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
				case 10: Overpowering(); break;
				case 11: Scuffle(); break;
				case 12: BurstFire(); break;
				case 13: CheekyTricks(); break;
				case 14: Rampage(); break;
				case 15: TheresAnother(); break;
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

			int currentKey = gameState.GetCurrentKey();

			//Megjelenítjük a kézben lévő lapokat, hogy a játékos választhasson közülük
			ChooseCard(currentKey, CardListFilter.NoMasterCrok);
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
					int currentKey = gameState.GetCurrentKey();
					ChooseCard(currentKey);
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

				int currentKey = gameState.GetCurrentKey();

				//Megjelenítjük a kézben lévő lapokat, hogy a játékos választhasson közülük
				ChooseCard(currentKey);
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
			int currentKey = gameState.GetCurrentKey();
			//Ha botról van szó
			if(!gameState.IsTheActivePlayerHuman())
			{
				gameState.StartCoroutine(gameState.GetAImodule().CardSelectionEffect(currentKey, CardListFilter.None));
			}
		}

		//Kicseréli a pályán lévő lapot a pakli legfelső lapjára
		private void JungleFight()
		{
			//Jelezzük a vezérlőnek, hogy cserére készülünk
			gameState.SetSelectionAction(SkillEffectAction.BlindSwitch);
			gameState.SetSwitchType(CardListTarget.Deck);

			int currentKey = gameState.GetCurrentKey();

			SwitchCard(currentKey, gameState.GetActiveCardID(), 0);
			gameState.StartCoroutine(gameState.SkillFinished());
		}

		//A következő körre ad +1 bónusz minden statra
		private void PowerOfTuition()
		{
			Data_Controller data = gameState.GetDataModule();
			int currentKey = gameState.GetCurrentKey();
			int cardPosition = gameState.GetActiveCardID();
			Card card = data.GetCardFromField(currentKey, cardPosition);

			gameState.StartCoroutine(PowerOfTuition_LateEffect(currentKey, card));
			gameState.StartCoroutine(gameState.SkillFinished());
		}

		private IEnumerator PowerOfTuition_LateEffect(int playerKey, Card target)
		{
			Data_Controller data = gameState.GetDataModule();

			while(true)
			{
				//Csak a late skill fázisban adjuk hozzá, hogy a következő körre legyen érvényes
				if(gameState.GetGameState() == MainGameStates.LateSkills && data.IsCardOnTheField(playerKey, target) )
				{
					int bonusDuration = 1;

					StatBonus bonusEffect = new StatBonus(1,1,1);

					FieldBonus newBonus = new FieldBonus(bonusEffect, bonusDuration);

					gameState.GetDataModule().GetPlayerWithKey(playerKey).AddFieldBonus(newBonus);

					break;
				}

				yield return null;
			}
			
		}

		//A lap, ami használja +2 Erő bónuszt kap az aktuális körre
		private void Overpowering()
		{
			Data_Controller data = gameState.GetDataModule();
			int currentKey = gameState.GetCurrentKey();
			int cardPosition = gameState.GetActiveCardID();
			Card card = data.GetCardFromField(currentKey, cardPosition);

			gameState.StartCoroutine(Overpowering_LateEffect(currentKey, card));

			gameState.StartCoroutine(gameState.SkillFinished());
		}

		private IEnumerator Overpowering_LateEffect(int playerKey, Card target)
		{
			Data_Controller data = gameState.GetDataModule();
			bool needsToCheck = false;
			bool bonusGiven = false;
			List<Card> opponentCardList = new List<Card>();
			List<int> keylist = data.GetKeyList();

			while(true)
			{

				//Ha nincs már a kártya a mezőn
				if(!data.IsCardOnTheField(playerKey, target))
				{
					break;
				}


				//Ellenséges lapok feljegyzése: Ha új lap kerül a pályára, ami miatt változhat a képesség hatása. 
				//A változást a pályán észre kell venni és felül kell vizsgálni a helyzetünket.
				foreach (int key in keylist) 
                {
                    if(key != playerKey)
                    {
                        foreach (Card card in data.GetCardsFromField(key)) 
                        {
                            if(!opponentCardList.Contains(card))
                            {
                                opponentCardList.Add(card);
                                needsToCheck = true;
                            }
                        }
                    }
                }

				//Ha felülvizsgálásra van szükség
				if(needsToCheck)
				{
					Debug.Log("Player with the effect: " + playerKey.ToString());
					needsToCheck = false;

					//Ha magasabb az intelligencia statja, mint az ellenfeleié: +2 Erőt kap
					bool result = data.IsMyStatIsTheHighest(playerKey, target.GetIntelligence(), CardStatType.Intelligence);
					if( result && !bonusGiven)
					{
						StatBonus newBonus = new StatBonus(2,0,0);
						target.AddBonus(newBonus);
						bonusGiven = true;
					}

					//Ha már megadtuk a bónuszt, de nem vagyunk többé a legokosabbak: elvesszük a bónuszt
					else if(!result && bonusGiven)
					{
						StatBonus newBonus = new StatBonus(-2,0,0);
						target.AddBonus(newBonus);
						bonusGiven = false;
					}
				}

				//Ha az összehasonlítás fázis eljött: végeztünk
				if(gameState.GetGameState() == MainGameStates.CompareCards)
                {
                    break;
                }

                yield return null;
			}			
		}

		//Ha veszítettünk a lappal, akkor vakharc indul és mi leszünk a támadók benne
		private void Scuffle()
		{
			Data_Controller data = gameState.GetDataModule();
			int playerKey = gameState.GetCurrentKey();
			gameState.AddKeyToLateSkills(playerKey);
			int cardPosition = gameState.GetActiveCardID();
			Card card = data.GetCardFromField(playerKey, cardPosition);
			gameState.StartCoroutine(Scuffle_LateEffect(playerKey, card));
			gameState.StartCoroutine(gameState.SkillFinished());
		}

		private IEnumerator Scuffle_LateEffect(int playerKey, Card target)
		{
			Data_Controller data = gameState.GetDataModule();
			while(true)
			{

				//Ha nincs már a kártya a mezőn
				if(!data.IsCardOnTheField(playerKey, target))
				{
					break;
				}

				//Csak a late skill fázisban fejti ki hatását, amikor a mi képességeink érvényesítésének hatása van
				if(gameState.GetGameState() == MainGameStates.LateSkills 
					&& gameState.GetCurrentKey() == playerKey
					&& data.IsCardOnTheField(playerKey, target))
				{
					
					//Ha vesztettünk
					if(data.GetPlayerWithKey(playerKey).GetResult() == PlayerTurnResult.Lose)
					{
						Debug.Log("Player with the effect: " + playerKey.ToString());

						//Vakharccal kezdünk
						gameState.SetBlindMatchState(true);

						//Beállítjuk, hogy a következő kört mi kezdjük
						gameState.SetNewAttacker(playerKey);
					}

						break;
				}

				yield return null;
			}
		}

		//Minden ellenséges crok után +1 minden értékére. Cserék új croknak számítanak
		private void BurstFire()
		{
			Data_Controller data = gameState.GetDataModule();
			int currentKey = gameState.GetCurrentKey();
			int cardPosition = gameState.GetActiveCardID();
			Card card = data.GetCardFromField(currentKey, cardPosition);

			gameState.StartCoroutine(BurstFire_LateEffect(card, currentKey));

			gameState.StartCoroutine(gameState.SkillFinished());
		}

		private IEnumerator BurstFire_LateEffect(Card cardForBonus, int currentKey)
        {
            List<Card> opponentCardList = new List<Card>();
            Data_Controller data = gameState.GetDataModule();
            List<int> keylist = data.GetKeyList();

            while(true)
            {

				//Ha nincs már a kártya a mezőn
				if(!data.IsCardOnTheField(currentKey, cardForBonus))
				{
					break;
				}

                foreach (int key in keylist) 
                {
                    if(key != currentKey)
                    {
                        foreach (Card card in data.GetCardsFromField(key)) 
                        {
                            if(!opponentCardList.Contains(card) && data.IsCardOnTheField(currentKey, cardForBonus))
                            {
                                opponentCardList.Add(card);
                                cardForBonus.AddBonus(new StatBonus(1,1,1));
                            }
                        }
                    }
                }


                if(gameState.GetGameState() == MainGameStates.CompareCards)
                {
                    break;
                }

                else 
                {
                    yield return null;    
                }
            }
        }

		//Mi támadunk a következő körben, illetve +2 Reflex most ha védekezünk.
		private void CheekyTricks()
		{
			Data_Controller data = gameState.GetDataModule();
			int currentKey = gameState.GetCurrentKey();
			int cardPosition = gameState.GetActiveCardID();
			Card card = data.GetCardFromField(currentKey, cardPosition);
			gameState.AddKeyToLateSkills(currentKey);

			//Csak a normál skill fázisban fejti ki hatását: Védekezéskor bónuszt kapunk
			if(gameState.GetGameState() == MainGameStates.NormalSkills 
			&& data.GetPlayerWithKey(currentKey).GetRole() == PlayerTurnRole.Defender)
			{
				card.AddBonus(new StatBonus(0,0,2));
			}

			gameState.StartCoroutine(CheekyTricks_LateEffect(currentKey, card));
			gameState.StartCoroutine(gameState.SkillFinished());
		}

		private IEnumerator CheekyTricks_LateEffect(int playerKey, Card target)
		{
			Data_Controller data = gameState.GetDataModule();
			while(true)
			{

				//Ha nincs már a kártya a mezőn
				if(!data.IsCardOnTheField(playerKey, target))
				{
					break;
				}

				//Csak a late skill fázisban fejti ki hatását: Mi leszünk a támadók legközelebb
				if(gameState.GetGameState() == MainGameStates.LateSkills 
					&& gameState.GetCurrentKey() == playerKey
					&& data.IsCardOnTheField(playerKey, target))
				{
					Debug.Log("Player with the effect: " + playerKey.ToString());

					//Beállítjuk, hogy a következő kört mi kezdjük
					gameState.SetNewAttacker(playerKey);

					break;
				}

				else 
				{
					yield return null;
				}
			}
		}



		private void Rampage()
		{
			gameState.SetSelectionAction(SkillEffectAction.CheckWinnerAmount);
			gameState.SetSwitchType(CardListTarget.None);
			int currentKey = gameState.GetCurrentKey();
			//Ha botról van szó
			if(!gameState.IsTheActivePlayerHuman())
			{
				gameState.StartCoroutine(gameState.GetAImodule().CardSelectionEffect(currentKey, CardListFilter.None));
			}

		}

		private void TheresAnother()
		{
			gameState.SetSelectionAction(SkillEffectAction.SwitchOpponentCard);
			gameState.SetSwitchType(CardListTarget.Hand);
			int currentKey = gameState.GetCurrentKey();
			//Ha botról van szó
			if(!gameState.IsTheActivePlayerHuman())
			{
				Debug.Log("Bot path start");
				gameState.StartCoroutine(gameState.GetAImodule().CardSelectionEffect(currentKey, CardListFilter.None));
			}
		}

		//Erőre változtatja a harc típusát
		private void ShieldFight()
		{
			SetActiveStat(CardStatType.Power);
			gameState.StartCoroutine(gameState.SkillFinished());
		}

        #region Skill Actions

        //Jelzünk a játékvezérlőnek, hogy a képesség végzett, mehet a játék tovább
        private void Response()
		{
			gameState.StartCoroutine(gameState.SkillFinished());
		}

		//A megadott paraméterekkel megjelenít egy kártya listát, amiből választhat a játékos
		public void ChooseCard(int currentKey, CardListFilter filter = CardListFilter.None, int limit = 0)
		{

			//Ha ember
			if (gameState.IsTheActivePlayerHuman())
			{
				gameState.StartCoroutine(gameState.GetClientModule().DisplayCardList(currentKey, filter, limit));
			}

			//Ha bot
			else
			{
				gameState.StartCoroutine(gameState.GetAImodule().CardSelectionEffect(currentKey, filter, limit));
			}
		}

		//Kicserél egy mezőn lévő lapot egy másikra
		public void SwitchCard(int currentKey, int cardOnField, int cardToSwitch)
		{
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