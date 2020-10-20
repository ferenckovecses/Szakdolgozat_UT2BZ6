using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class Skill_Controller
	{
		private GameState_Controller gameState;
		private Data_Controller data;
		private Client_Controller client;

		public Skill_Controller(GameState_Controller con)
		{
			this.gameState = con;
			this.data = gameState.GetDataModule();
			this.client = gameState.GetClientModule();
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
				case 17: Scouting(); break;
				case 18: TigerClaw(); break;
				case 19: Arrest(); break;
				case 20: Preach(); break;
				case 21: GunFight(); break;

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

			MakePlayerChooseCard();
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
					MakePlayerChooseCard();
				}

				//Ha játékos, akkor jelezzünk neki, hogy válasszon játékost
				else 
				{
					gameState.StartCoroutine(client.DisplayNotification("Válassz egy játékos vesztesei közül!"));
				}
			}

			//Ha nincs még vesztes kártya, akkor passzolunk
			else
			{
				if(gameState.IsTheActivePlayerHuman())
				{
					gameState.StartCoroutine(client.DisplayNotification("Nincsenek vesztes lapok!"));
				}
				gameState.Pass();
			}
		}

		//Mindenki képességét hatástalanítja/passzoltatja
		private void FirstCut()
		{
            int ownKey = gameState.GetCurrentKey();

			foreach (int key in data.GetKeyList())
            {
                int position = 0;
                foreach (Card card in data.GetCardsFromField(key)) 
                {
                	client.SetSkillState(key, position, SkillState.Pass);
                    position++;
                }
                data.GetPlayerWithKey(key).SetStatus(PlayerTurnStatus.Finished);
            }

            gameState.StartCoroutine(client.DisplayNotification(data.GetPlayerName(ownKey) +" hatástalanította a többiek képességét!"));
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

				MakePlayerChooseCard();
			}

			else
			{
				gameState.Pass();
			}
		}
		//Ütközet indítása: Felhúz mindenkie +1 lapot a pakli tetejéről
		private void GreatFight()
		{

			foreach (int key in data.GetKeyList())
			{
				if(data.GetPlayerWithKey(key).GetCardsOnField().Count < 4)
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

			//Ha játékos
			else 
			{
				gameState.StartCoroutine(client.DisplayNotification("Nevezz meg egy játékost, aki eldobjon egy kártyát!"));
			}
		}

		//Kicseréli a pályán lévő lapot a pakli legfelső lapjára
		private void JungleFight()
		{
			int currentKey = gameState.GetCurrentKey();

			//Ha van a paklinkban lap
			if(data.GetDeckAmount(currentKey) > 0)
			{
				//Jelezzük a vezérlőnek, hogy cserére készülünk
				gameState.SetSelectionAction(SkillEffectAction.BlindSwitch);
				gameState.SetSwitchType(CardListTarget.Deck);

				SwitchCard(currentKey, gameState.GetActiveCardID(), 0);

				gameState.SetCurrentAction(SkillEffectAction.None);
			}

			//Ha nincs lap a pakliban
			else 
			{
				//Ha játékos, akkor tájékoztatjuk
				if(gameState.IsTheActivePlayerHuman())
				{
					gameState.StartCoroutine(client.DisplayNotification("Nincs már lap a paklidban!"));
				}
			}

			gameState.StartCoroutine(gameState.SkillFinished());
		}

		//A következő körre ad +1 bónuszt minden statra
		private void PowerOfTuition()
		{
			int currentKey = gameState.GetCurrentKey();
			int cardPosition = gameState.GetActiveCardID();
			Card card = data.GetCardFromField(currentKey, cardPosition);

			gameState.StartCoroutine(PowerOfTuition_LateEffect(currentKey, card));
			gameState.StartCoroutine(gameState.SkillFinished());
		}

		private IEnumerator PowerOfTuition_LateEffect(int playerKey, Card target)
		{
			while(true)
			{
				//Csak a late skill fázisban adjuk hozzá, hogy a következő körre legyen érvényes
				if(gameState.GetGameState() == MainGameStates.LateSkills && data.IsCardOnTheField(playerKey, target) )
				{
					int bonusDuration = 1;

					StatBonus bonusEffect = new StatBonus(1,1,1);

					FieldBonus newBonus = new FieldBonus(bonusEffect, bonusDuration);

					data.GetPlayerWithKey(playerKey).AddFieldBonus(newBonus);

					break;
				}

				yield return null;
			}
			
		}

		//A lap, ami használja +2 Erő bónuszt kap az aktuális körre
		private void Overpowering()
		{
			int currentKey = gameState.GetCurrentKey();
			int cardPosition = gameState.GetActiveCardID();
			Card card = data.GetCardFromField(currentKey, cardPosition);

			gameState.StartCoroutine(Overpowering_LateEffect(currentKey, card));

			gameState.StartCoroutine(gameState.SkillFinished());
		}

		private IEnumerator Overpowering_LateEffect(int playerKey, Card target)
		{
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
					Debug.Log("Player with Overpowering effect: " + playerKey.ToString());
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
			int playerKey = gameState.GetCurrentKey();
			gameState.AddKeyToLateSkills(playerKey);
			int cardPosition = gameState.GetActiveCardID();
			Card card = data.GetCardFromField(playerKey, cardPosition);
			gameState.StartCoroutine(Scuffle_LateEffect(playerKey, card));
			gameState.StartCoroutine(gameState.SkillFinished());
		}

		private IEnumerator Scuffle_LateEffect(int playerKey, Card target)
		{
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
						Debug.Log("Player with Scuffle effect: " + playerKey.ToString());

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
			int currentKey = gameState.GetCurrentKey();
			int cardPosition = gameState.GetActiveCardID();
			Card card = data.GetCardFromField(currentKey, cardPosition);

			gameState.StartCoroutine(BurstFire_LateEffect(card, currentKey));

			gameState.StartCoroutine(gameState.SkillFinished());
		}

		private IEnumerator BurstFire_LateEffect(Card cardForBonus, int currentKey)
        {
            List<Card> opponentCardList = new List<Card>();
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
					Debug.Log("Player with Cheeky Tricks effect: " + playerKey.ToString());

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

		//A kiválasztott játékos egyik lapját kicserélheted a paklija legalján lévő lapra
		private void TheresAnother()
		{
			gameState.SetSelectionAction(SkillEffectAction.SwitchOpponentCard);
			gameState.SetSwitchType(CardListTarget.Hand);
			int currentKey = gameState.GetCurrentKey();

			//Ha botról van szó
			if(!gameState.IsTheActivePlayerHuman())
			{
				gameState.StartCoroutine(gameState.GetAImodule().CardSelectionEffect(currentKey, CardListFilter.None));
			}

			//Ha játékos, akkor tájékoztatjuk őt a teendőiről
			else 
			{
				gameState.StartCoroutine(client.DisplayNotification("Nevezz meg egy játékost, akinek cserélnéd valamely lapját"));
			}
		}

		//Erőre változtatja a harc típusát
		private void ShieldFight()
		{
			if(gameState.GetActiveStat() != CardStatType.Power)
			{
				SetActiveStat(CardStatType.Power);
				gameState.StartCoroutine(client.DisplayNotification("A harc típusa Erőre változott!"));
			}
			gameState.StartCoroutine(gameState.SkillFinished());
		}

		private void Scouting()
		{
			gameState.SetSelectionAction(SkillEffectAction.Reorganize);
			gameState.SetSwitchType(CardListTarget.Deck);

			MakePlayerChooseCard(CardListFilter.None, 6);
		}

		//A kézből ledobott kártya Erejét használhatod
		private void TigerClaw()
		{
			gameState.SetSelectionAction(SkillEffectAction.SacrificeFromHand);
			gameState.SetSwitchType(CardListTarget.Hand);

			MakePlayerChooseCard();
		}

		private void Arrest()
		{
			gameState.SetSelectionAction(SkillEffectAction.SacrificeDoppelganger);
			gameState.SetSwitchType(CardListTarget.Hand);

			MakePlayerChooseCard(CardListFilter.EnemyDoppelganger);
		}

		private void Preach()
		{
			int currentKey = gameState.GetCurrentKey();
			int positionID = gameState.GetActiveCardID();
			client.SetSkillState(currentKey, positionID, SkillState.Use);

			bool purified = false;
			foreach (int key in data.GetKeyList()) 
			{
				if(key != currentKey)
				{
					int position = 0;
					foreach (Card card in data.GetCardsFromField(key)) 
					{
						if(card.GetCardType() == CardType.Démon)
						{
							client.SetSkillState(key, position, SkillState.Pass);
							purified = true;
						}

						position += 1;
					}
				}
			}

			if(purified)
			{
				gameState.StartCoroutine(client.DisplayNotification("A Démonok képességei blokkolva!"));
			}

			else 
			{
				gameState.StartCoroutine(gameState.DrawCardsUp(currentKey));
			}

			gameState.StartCoroutine(gameState.SkillFinished());
		}

		private void GunFight()
		{
			int currentKey = gameState.GetCurrentKey();
			gameState.AddKeyToLateSkills(currentKey);
			int cardPosition = gameState.GetActiveCardID();
			Card card = data.GetCardFromField(currentKey, cardPosition);

			gameState.StartCoroutine(GunFight_LateEffect(card, currentKey));

			gameState.StartCoroutine(gameState.SkillFinished());
		}

		private IEnumerator GunFight_LateEffect(Card target, int playerKey)
		{
			Debug.Log("rutin started");
			while(true)
			{
				//Csak a late skill fázisban fejti ki hatását: Nyerünk, ha döntetlen
				if(gameState.GetGameState() == MainGameStates.LateSkills && gameState.GetCurrentKey() == playerKey)
				{

					Debug.Log("sajt");
					if(data.GetPlayerWithKey(playerKey).GetResult() == PlayerTurnResult.Draw)
					{
						Debug.Log("alma");
						foreach (int key in data.GetKeyList()) 
						{
							if(key == playerKey)
							{
								data.GetPlayerWithKey(key).SetResult(PlayerTurnResult.Win);
							}

							else
							{
								data.GetPlayerWithKey(key).SetResult(PlayerTurnResult.Lose);
							}
						}

						gameState.SetBlindMatchState(false);
						Debug.Log("Player with Gun Fight effect: " + playerKey.ToString());
						break;
					}

					else
					{
						Debug.Log("break");
						break;	
					}
				}

				else 
				{
					yield return null;
				}
			}
		}

        #region Skill Actions

        //Jelzünk a játékvezérlőnek, hogy a képesség végzett, mehet a játék tovább
        private void Response()
		{
			gameState.StartCoroutine(gameState.SkillFinished());
		}

		//Kicserél egy mezőn lévő lapot egy másikra
		public void SwitchCard(int currentKey, int cardOnField, int cardToSwitch)
		{
			//Ha kézben kell keresni a választott lapot
			if (gameState.GetCurrentListType() == CardListTarget.Hand)
			{
				//Adatok kinyerése
				Card handData = data.GetCardFromHand(currentKey, cardToSwitch);
				Card fieldData = data.GetCardFromField(currentKey, cardOnField);

				//Model frissítése
				data.SwitchFromHand(currentKey, cardOnField, cardToSwitch);

				//UI frissítése
				client.SwitchHandFromField(currentKey, fieldData, cardOnField, handData, 
					cardToSwitch, gameState.IsTheActivePlayerHuman());
			}

			//Ha a pakliból cserélünk lapot
			else if(gameState.GetCurrentListType() == CardListTarget.Deck)
			{
				Card fieldData = data.GetCardFromField(currentKey, cardOnField);
				Card deckData = data.GetCardFromDeck(currentKey, cardToSwitch);

				//Model frissítése
				data.SwitchFromDeck(currentKey, cardOnField, cardToSwitch);

				//UI frissítése
				client.SwitchDeckFromField(currentKey,cardOnField, deckData );
			}
		}

		public void ReviveCard(int cardID)
		{
			int currentKey = gameState.GetCurrentKey();

			//Ha kézben kell keresni a választott lapot
			if (gameState.GetCurrentListType() == CardListTarget.Losers)
			{
				//Adatok kinyerése
				Card cardData = data.GetCardFromLosers(currentKey, cardID);

				//Model frissítése
				data.ReviveLostCard(currentKey, cardID);

				//UI frissítése
				client.DrawNewCard(cardData, currentKey, gameState.IsTheActivePlayerHuman(), DrawType.Normal, DrawTarget.Hand, SkillState.NotDecided);

				int winSize = data.GetWinnerAmount(currentKey);
				int lostSize = data.GetLostAmount(currentKey);
				Sprite win = data.GetLastWinnerImage(currentKey);
				Sprite lost = data.GetLastLostImage(currentKey);

				client.ChangePileText(winSize, lostSize, currentKey, win, lost);
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
			client.RefreshStatDisplay();
		}

		//A játékos választ egy előre definiált paraméterekkel rendelkező kártyalistából
        public void MakePlayerChooseCard(CardListFilter filter = CardListFilter.None, int limit = 0)
        {
			int currentKey = gameState.GetCurrentKey();

			//Megjelenítjük a kézben lévő lapokat, hogy a játékos választhasson közülük
			ChooseCard(currentKey, filter, limit);
        }

        //A megadott paraméterekkel megjelenít egy kártya listát, amiből választhat a játékos
		private void ChooseCard(int currentKey, CardListFilter filter, int limit)
		{

			//Ha ember
			if (gameState.IsTheActivePlayerHuman())
			{
				gameState.StartCoroutine(client.DisplayCardList(currentKey, filter, limit));
			}

			//Ha bot
			else
			{
				gameState.StartCoroutine(gameState.GetAImodule().CardSelectionEffect(currentKey, filter, limit));
			}
		}

		//A játékos listáról választott lapjával kezdünk valamit
		public void HandleSelectionForSkill(int selectedCard, int currentKey)
		{
			//Ha az akció a csere volt
            if (gameState.GetCurrentAction() == SkillEffectAction.Switch)
            {
                SwitchCard(currentKey, gameState.GetActiveCardID(), selectedCard);
                gameState.SetCurrentAction(SkillEffectAction.None);
                gameState.ActionFinished();
            }

            //Ha az akció skill lopás volt
            else if (gameState.GetCurrentAction() == SkillEffectAction.SkillUse)
            {
                Card otherCard = data.GetCardFromLosers(currentKey, selectedCard);

                //Másik Devil crok képesség esetén passzolunk
                if (otherCard.GetCardID() == 3)
                {
                    gameState.Pass();
                    gameState.SetCurrentAction(SkillEffectAction.None);
                    gameState.StartCoroutine(gameState.SkillFinished());
                }

                //Amúgy meg használjuk a másik lap képességét
                else
                {
                    //Adatgyűjtés
                    Card ownCard = data.GetCardFromField(currentKey, gameState.GetActiveCardID());
                    List<SkillProperty> temp = otherCard.GetSkillProperty();

                    //Skill tulajdonságok módosítása
                    ownCard.ModifySkills(temp);
                    ownCard.SetSKillID(otherCard.GetCardID());

                    UseSkill(ownCard.GetCardID());
                }

            }

            else if (gameState.GetCurrentAction() == SkillEffectAction.Revive)
            {
                ReviveCard(selectedCard);
                gameState.SetCurrentAction(SkillEffectAction.None);
                gameState.ActionFinished();
            }

            else if(gameState.GetCurrentAction() == SkillEffectAction.TossCard)
            {
                data.TossCardFromHand(currentKey, selectedCard);
                client.TossCard(currentKey, selectedCard);
                gameState.SetCurrentAction(SkillEffectAction.None);
                gameState.ActionFinished();
            }

            else if(gameState.GetCurrentAction() == SkillEffectAction.SwitchOpponentCard)
            {
                gameState.SetSwitchType(CardListTarget.Deck);
                int deckCardID = (data.GetDeckAmount(currentKey)) - 1 ;
                SwitchCard(currentKey, selectedCard, deckCardID);
            }

            else if(gameState.GetCurrentAction() == SkillEffectAction.SacrificeFromHand)
            {
            	//Kártya adatok bekérése
            	Card ownCard = data.GetCardFromField(currentKey, gameState.GetActiveCardID());
            	Card sacrificed = data.GetCardFromHand(currentKey, selectedCard);

            	//Bónusz meghatározása és aktiválása
            	int ownPower = ownCard.GetPower();
            	int sacrPower = sacrificed.GetPower();
        		ownCard.AddBonus(new StatBonus((sacrPower - ownPower),0,0));

        		//Választott kártya eldobása
                data.TossCardFromHand(currentKey, selectedCard);
                client.TossCard(currentKey, selectedCard);

                gameState.SetCurrentAction(SkillEffectAction.None);
                gameState.ActionFinished();
            }

            else if (gameState.GetCurrentAction() == SkillEffectAction.SacrificeDoppelganger) 
            {
            	//Kártya adatok bekérése
            	Card ownCard = data.GetCardFromField(currentKey, gameState.GetActiveCardID());
            	Card sacrificed = data.GetCardFromHand(currentKey, selectedCard);

        		//Választott kártya eldobása
                data.TossCardFromHand(currentKey, selectedCard);
                client.TossCard(currentKey, selectedCard);

                gameState.MakeWinner(currentKey);
            	gameState.ActionFinished();
            }
		}

		#endregion


	}
}