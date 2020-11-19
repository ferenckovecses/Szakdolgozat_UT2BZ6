using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
	public class Skill_Controller
	{
		private Module_Controller modules;

		public Skill_Controller(Module_Controller in_modules)
		{
			this.modules = in_modules;
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
				default: modules.GetGameModule().Pass(); break;
			}
		}

		//Cserélhet kézből egy nem Master Crok lapot
		private void UnexpectedAttack()
		{

			//Jelezzük a vezérlőnek, hogy cserére készülünk
			modules.GetGameModule().SetSelectionAction(SkillEffectAction.Switch);
			modules.GetGameModule().SetSwitchType(CardListTarget.Hand);

			MakePlayerChooseCard();
		}

		//Megváltoztathatjuk a harc típusát
		private void PlotTwist()
		{
			modules.GetGameModule().StartCoroutine(TriggerStatChange());
		}

		//Használhatod egy másik játékos vesztes krokjának képességét
		private void SoulStealing()
		{
			//Vannak-e ellenséges vesztes kártyák, amik közül választhatunk
			if (modules.GetGameModule().IsThereOtherLostCards())
			{
				modules.GetGameModule().SetSelectionAction(SkillEffectAction.SkillUse);
				modules.GetGameModule().SetSwitchType(CardListTarget.Hand);

				//Ha bot, akkor kell egy kis rásegítés neki
				if (!modules.GetGameModule().IsTheActivePlayerHuman())
				{
					MakePlayerChooseCard();
				}

				//Ha játékos, akkor jelezzünk neki, hogy válasszon játékost
				else 
				{
					modules.GetGameModule().StartCoroutine(modules.GetClientModule().DisplayNotification("Válassz egy játékos vesztesei közül!"));
				}
			}

			//Ha nincs még vesztes kártya, akkor passzolunk
			else
			{
				if(modules.GetGameModule().IsTheActivePlayerHuman())
				{
					modules.GetGameModule().StartCoroutine(modules.GetClientModule().DisplayNotification("Nincsenek vesztes lapok!"));
				}
				modules.GetGameModule().Pass();
			}
		}

		//Mindenki képességét hatástalanítja/passzoltatja
		private void FirstCut()
		{
            int ownKey = modules.GetGameModule().GetCurrentKey();

			foreach (int key in modules.GetDataModule().GetKeyList())
            {
                int position = 0;
                foreach (Card card in modules.GetDataModule().GetCardsFromPlayer(key, CardListTarget.Field)) 
                {
                	modules.GetClientModule().SetSkillState(key, position, SkillState.Pass);
                    position++;
                }
                modules.GetDataModule().GetPlayerWithKey(key).SetStatus(PlayerTurnStatus.Finished);
            }

            modules.GetGameModule().StartCoroutine(modules.GetClientModule().DisplayNotification(modules.GetDataModule().GetPlayerName(ownKey) +" hatástalanította a többiek képességét!"));
            modules.GetGameModule().SetNegationStatus(true);
            modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
		}

		private void Revive()
		{
			//Van-e feltámasztható vesztes
			if (modules.GetGameModule().DoWeHaveLosers())
			{
				//Jelezzük a vezérlőnek, hogy kártya felvételre készülünk
				modules.GetGameModule().SetSelectionAction(SkillEffectAction.Revive);
				modules.GetGameModule().SetSwitchType(CardListTarget.Losers);

				MakePlayerChooseCard();
			}

			else
			{
				modules.GetGameModule().Pass();
			}
		}
		//Ütközet indítása: Felhúz mindenkie +1 lapot a pakli tetejéről
		private void GreatFight()
		{

			foreach (int key in modules.GetDataModule().GetKeyList())
			{
				if(modules.GetDataModule().GetCardsFromPlayer(key, CardListTarget.Field).Count < 4)
				{
					modules.GetGameModule().DrawTheCard(key, DrawTarget.Field, DrawType.Normal, SkillState.Pass);
				}
			}

			modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
		}

		//Egy kiválasztott játékosnak le kell dobnia a kezéből egy lapot.
		private void NoMercy()
		{
			modules.GetGameModule().SetSelectionAction(SkillEffectAction.Execute);
			modules.GetGameModule().SetSwitchType(CardListTarget.Hand);
			int currentKey = modules.GetGameModule().GetCurrentKey();
			//Ha botról van szó
			if(!modules.GetGameModule().IsTheActivePlayerHuman())
			{
				modules.GetGameModule().StartCoroutine(modules.GetAImodule().CardSelectionEffect(currentKey, CardListFilter.None));
			}

			//Ha játékos
			else 
			{
				modules.GetGameModule().StartCoroutine(modules.GetClientModule().DisplayNotification("Nevezz meg egy játékost, aki eldobjon egy kártyát!"));
			}
		}

		//Kicseréli a pályán lévő lapot a pakli legfelső lapjára
		private void JungleFight()
		{
			int currentKey = modules.GetGameModule().GetCurrentKey();

			//Ha van a paklinkban lap
			if(modules.GetDataModule().GetDeckAmount(currentKey) > 0)
			{
				//Jelezzük a vezérlőnek, hogy cserére készülünk
				modules.GetGameModule().SetSelectionAction(SkillEffectAction.BlindSwitch);
				modules.GetGameModule().SetSwitchType(CardListTarget.Deck);

				SwitchCard(currentKey, modules.GetGameModule().GetActiveCardID(), 0);

				modules.GetGameModule().SetCurrentAction(SkillEffectAction.None);
			}

			//Ha nincs lap a pakliban
			else 
			{
				//Ha játékos, akkor tájékoztatjuk
				if(modules.GetGameModule().IsTheActivePlayerHuman())
				{
					modules.GetGameModule().StartCoroutine(modules.GetClientModule().DisplayNotification("Nincs már lap a paklidban!"));
				}
			}

			modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
		}

		//A következő körre ad +1 bónuszt minden statra
		private void PowerOfTuition()
		{
			int currentKey = modules.GetGameModule().GetCurrentKey();
			int cardPosition = modules.GetGameModule().GetActiveCardID();
			Card card = modules.GetDataModule().GetCardFromPlayer(currentKey, cardPosition, CardListTarget.Field);

			modules.GetGameModule().StartCoroutine(PowerOfTuition_LateEffect(currentKey, card));
			modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
		}

		private IEnumerator PowerOfTuition_LateEffect(int playerKey, Card target)
		{
			while(true)
			{
				//Csak a late skill fázisban adjuk hozzá, hogy a következő körre legyen érvényes
				if(modules.GetGameModule().GetGameState() == MainGameStates.LateSkills && modules.GetDataModule().IsCardOnTheField(playerKey, target) )
				{
					int bonusDuration = 1;

					StatBonus bonusEffect = new StatBonus(1,1,1);

					FieldBonus newBonus = new FieldBonus(bonusEffect, bonusDuration);

					modules.GetDataModule().GetPlayerWithKey(playerKey).AddFieldBonus(newBonus);

					break;
				}

				yield return null;
			}
			
		}

		//A lap, ami használja +2 Erő bónuszt kap az aktuális körre
		private void Overpowering()
		{
			int currentKey = modules.GetGameModule().GetCurrentKey();
			int cardPosition = modules.GetGameModule().GetActiveCardID();
			Card card = modules.GetDataModule().GetCardFromPlayer(currentKey, cardPosition, CardListTarget.Field);

			modules.GetGameModule().StartCoroutine(Overpowering_LateEffect(currentKey, card));

			modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
		}

		private IEnumerator Overpowering_LateEffect(int playerKey, Card target)
		{
			bool needsToCheck = false;
			bool bonusGiven = false;
			List<Card> opponentCardList = new List<Card>();
			List<int> keylist = modules.GetDataModule().GetKeyList();

			while(true)
			{

				//Ha nincs már a kártya a mezőn
				if(!modules.GetDataModule().IsCardOnTheField(playerKey, target))
				{
					break;
				}


				//Ellenséges lapok feljegyzése: Ha új lap kerül a pályára, ami miatt változhat a képesség hatása. 
				//A változást a pályán észre kell venni és felül kell vizsgálni a helyzetünket.
				foreach (int key in keylist) 
                {
                    if(key != playerKey)
                    {
                        foreach (Card card in modules.GetDataModule().GetCardsFromPlayer(key, CardListTarget.Field)) 
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
					bool result = modules.GetDataModule().IsMyStatIsTheHighest(playerKey, target.GetIntelligence(), CardStatType.Intelligence);
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
				if(modules.GetGameModule().GetGameState() == MainGameStates.CompareCards)
                {
                    break;
                }

                yield return null;
			}			
		}

		//Ha veszítettünk a lappal, akkor vakharc indul és mi leszünk a támadók benne
		private void Scuffle()
		{
			int playerKey = modules.GetGameModule().GetCurrentKey();
			modules.GetGameModule().AddKeyToLateSkills(playerKey);
			int cardPosition = modules.GetGameModule().GetActiveCardID();
			Card card = modules.GetDataModule().GetCardFromPlayer(playerKey, cardPosition, CardListTarget.Field);

			modules.GetGameModule().StartCoroutine(Scuffle_LateEffect(playerKey, card));
			modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
		}

		private IEnumerator Scuffle_LateEffect(int playerKey, Card target)
		{
			while(true)
			{

				//Ha nincs már a kártya a mezőn
				if(!modules.GetDataModule().IsCardOnTheField(playerKey, target))
				{
					break;
				}

				//Csak a late skill fázisban fejti ki hatását, amikor a mi képességeink érvényesítésének hatása van
				if(modules.GetGameModule().GetGameState() == MainGameStates.LateSkills 
					&& modules.GetGameModule().GetCurrentKey() == playerKey
					&& modules.GetDataModule().IsCardOnTheField(playerKey, target))
				{
					
					//Ha vesztettünk
					if(modules.GetDataModule().GetPlayerWithKey(playerKey).GetResult() == PlayerTurnResult.Lose)
					{
						Debug.Log("Player with Scuffle effect: " + playerKey.ToString());

						//Vakharccal kezdünk
						modules.GetGameModule().SetBlindMatchState(true);

						//Beállítjuk, hogy a következő kört mi kezdjük
						modules.GetGameModule().SetNewAttacker(playerKey);
					}

						break;
				}

				yield return null;
			}
		}

		//Minden ellenséges crok után +1 minden értékére. Cserék új croknak számítanak
		private void BurstFire()
		{
			int currentKey = modules.GetGameModule().GetCurrentKey();
			int cardPosition = modules.GetGameModule().GetActiveCardID();
			Card card = modules.GetDataModule().GetCardFromPlayer(currentKey, cardPosition, CardListTarget.Field);

			modules.GetGameModule().StartCoroutine(BurstFire_LateEffect(card, currentKey));

			modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
		}

		private IEnumerator BurstFire_LateEffect(Card cardForBonus, int currentKey)
        {
            List<Card> opponentCardList = new List<Card>();
            List<int> keylist = modules.GetDataModule().GetKeyList();

            while(true)
            {

				//Ha nincs már a kártya a mezőn
				if(!modules.GetDataModule().IsCardOnTheField(currentKey, cardForBonus))
				{
					break;
				}

                foreach (int key in keylist) 
                {
                    if(key != currentKey)
                    {
                        foreach (Card card in modules.GetDataModule().GetCardsFromPlayer(key, CardListTarget.Field)) 
                        {
                            if(!opponentCardList.Contains(card) && modules.GetDataModule().IsCardOnTheField(currentKey, cardForBonus))
                            {
                                opponentCardList.Add(card);
                                cardForBonus.AddBonus(new StatBonus(1,1,1));
                            }
                        }
                    }
                }


                if(modules.GetGameModule().GetGameState() == MainGameStates.CompareCards)
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
			int currentKey = modules.GetGameModule().GetCurrentKey();
			int cardPosition = modules.GetGameModule().GetActiveCardID();
			Card card = modules.GetDataModule().GetCardFromPlayer(currentKey, cardPosition, CardListTarget.Field);
			modules.GetGameModule().AddKeyToLateSkills(currentKey);

			//Csak a normál skill fázisban fejti ki hatását: Védekezéskor bónuszt kapunk
			if(modules.GetGameModule().GetGameState() == MainGameStates.NormalSkills 
			&& modules.GetDataModule().GetPlayerWithKey(currentKey).GetRole() == PlayerTurnRole.Defender)
			{
				card.AddBonus(new StatBonus(0,0,2));
			}

			modules.GetGameModule().StartCoroutine(CheekyTricks_LateEffect(currentKey, card));
			modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
		}

		private IEnumerator CheekyTricks_LateEffect(int playerKey, Card target)
		{
			while(true)
			{

				//Ha nincs már a kártya a mezőn
				if(!modules.GetDataModule().IsCardOnTheField(playerKey, target))
				{
					break;
				}

				//Csak a late skill fázisban fejti ki hatását: Mi leszünk a támadók legközelebb
				if(modules.GetGameModule().GetGameState() == MainGameStates.LateSkills 
					&& modules.GetGameModule().GetCurrentKey() == playerKey
					&& modules.GetDataModule().IsCardOnTheField(playerKey, target))
				{
					Debug.Log("Player with Cheeky Tricks effect: " + playerKey.ToString());

					//Beállítjuk, hogy a következő kört mi kezdjük
					modules.GetGameModule().SetNewAttacker(playerKey);

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
			modules.GetGameModule().SetSelectionAction(SkillEffectAction.CheckWinnerAmount);
			modules.GetGameModule().SetSwitchType(CardListTarget.None);
			int currentKey = modules.GetGameModule().GetCurrentKey();
			//Ha botról van szó
			if(!modules.GetGameModule().IsTheActivePlayerHuman())
			{
				modules.GetGameModule().StartCoroutine(modules.GetAImodule().CardSelectionEffect(currentKey, CardListFilter.None));
			}

		}

		//A kiválasztott játékos egyik lapját kicserélheted a paklija legalján lévő lapra
		private void TheresAnother()
		{
			modules.GetGameModule().SetSelectionAction(SkillEffectAction.SwitchOpponentCard);
			modules.GetGameModule().SetSwitchType(CardListTarget.Hand);
			int currentKey = modules.GetGameModule().GetCurrentKey();

			//Ha botról van szó
			if(!modules.GetGameModule().IsTheActivePlayerHuman())
			{
				modules.GetGameModule().StartCoroutine(modules.GetAImodule().CardSelectionEffect(currentKey, CardListFilter.None));
			}

			//Ha játékos, akkor tájékoztatjuk őt a teendőiről
			else 
			{
				modules.GetGameModule().StartCoroutine(modules.GetClientModule().DisplayNotification("Nevezz meg egy játékost, akinek cserélnéd valamely lapját"));
			}
		}

		//Erőre változtatja a harc típusát
		private void ShieldFight()
		{
			if(modules.GetGameModule().GetActiveStat() != CardStatType.Power)
			{
				SetActiveStat(CardStatType.Power);
				modules.GetGameModule().StartCoroutine(modules.GetClientModule().DisplayNotification("A harc típusa Erőre változott!"));
			}
			modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
		}

		private void Scouting()
		{
			modules.GetGameModule().SetSelectionAction(SkillEffectAction.Reorganize);
			modules.GetGameModule().SetSwitchType(CardListTarget.Deck);

			MakePlayerChooseCard(CardListFilter.None, 6);
		}

		//A kézből ledobott kártya Erejét használhatod
		private void TigerClaw()
		{
			modules.GetGameModule().SetSelectionAction(SkillEffectAction.SacrificeFromHand);
			modules.GetGameModule().SetSwitchType(CardListTarget.Hand);

			MakePlayerChooseCard();
		}

		private void Arrest()
		{
			modules.GetGameModule().SetSelectionAction(SkillEffectAction.SacrificeDoppelganger);
			modules.GetGameModule().SetSwitchType(CardListTarget.Hand);

			MakePlayerChooseCard(CardListFilter.EnemyDoppelganger);
		}

		private void Preach()
		{
			int currentKey = modules.GetGameModule().GetCurrentKey();
			int positionID = modules.GetGameModule().GetActiveCardID();
			modules.GetClientModule().SetSkillState(currentKey, positionID, SkillState.Use);

			bool purified = false;
			foreach (int key in modules.GetDataModule().GetKeyList()) 
			{
				if(key != currentKey)
				{
					int position = 0;
					foreach (Card card in modules.GetDataModule().GetCardsFromPlayer(key, CardListTarget.Field)) 
					{
						if(card.GetCardType() == CardType.Démon)
						{
							modules.GetClientModule().SetSkillState(key, position, SkillState.Pass);
							purified = true;
						}

						position += 1;
					}
				}
			}

			if(purified)
			{
				modules.GetGameModule().StartCoroutine(modules.GetClientModule().DisplayNotification("A Démonok képességei blokkolva!"));
			}

			else 
			{
				modules.GetGameModule().StartCoroutine(modules.GetGameModule().DrawCardsUp(currentKey));
			}

			modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
		}

		private void GunFight()
		{
			int currentKey = modules.GetGameModule().GetCurrentKey();
			modules.GetGameModule().AddKeyToLateSkills(currentKey);
			int cardPosition = modules.GetGameModule().GetActiveCardID();
			Card card = modules.GetDataModule().GetCardFromPlayer(currentKey, cardPosition, CardListTarget.Field);

			modules.GetGameModule().StartCoroutine(GunFight_LateEffect(card, currentKey));

			modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
		}

		private IEnumerator GunFight_LateEffect(Card target, int playerKey)
		{
			while(true)
			{
				//Csak a late skill fázisban fejti ki hatását: Nyerünk, ha döntetlen
				if(modules.GetGameModule().GetGameState() == MainGameStates.LateSkills && modules.GetGameModule().GetCurrentKey() == playerKey)
				{
					if(modules.GetDataModule().GetPlayerWithKey(playerKey).GetResult() == PlayerTurnResult.Draw)
					{
						foreach (int key in modules.GetDataModule().GetKeyList()) 
						{
							if(key == playerKey)
							{
								modules.GetDataModule().GetPlayerWithKey(key).SetResult(PlayerTurnResult.Win);
							}

							else
							{
								modules.GetDataModule().GetPlayerWithKey(key).SetResult(PlayerTurnResult.Lose);
							}
						}

						modules.GetGameModule().SetBlindMatchState(false);
						modules.GetGameModule().SetNewAttacker(playerKey);
						Debug.Log("Player with Gun Fight effect: " + playerKey.ToString());
						break;
					}

					else
					{
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
			modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
		}

		//Kicserél egy mezőn lévő lapot egy másikra
		public void SwitchCard(int currentKey, int cardOnField, int cardToSwitch)
		{
			//Ha kézben kell keresni a választott lapot
			if (modules.GetGameModule().GetCurrentListType() == CardListTarget.Hand)
			{
				//Adatok kinyerése
				Card handData = modules.GetDataModule().GetCardFromPlayer(currentKey, cardToSwitch, CardListTarget.Hand);
				Card fieldData = modules.GetDataModule().GetCardFromPlayer(currentKey, cardOnField, CardListTarget.Field);

				//Model frissítése
				modules.GetDataModule().SwitchFromHand(currentKey, cardOnField, cardToSwitch);

				//UI frissítése
				modules.GetClientModule().SwitchHandFromField(currentKey, fieldData, cardOnField, handData, 
					cardToSwitch, modules.GetGameModule().IsTheActivePlayerHuman());
			}

			//Ha a pakliból cserélünk lapot
			else if(modules.GetGameModule().GetCurrentListType() == CardListTarget.Deck)
			{
				Card fieldData = modules.GetDataModule().GetCardFromPlayer(currentKey, cardOnField, CardListTarget.Field);
				Card deckData = modules.GetDataModule().GetCardFromPlayer(currentKey, cardToSwitch, CardListTarget.Deck);

				//Model frissítése
				modules.GetDataModule().SwitchFromDeck(currentKey, cardOnField, cardToSwitch);

				//UI frissítése
				modules.GetClientModule().SwitchDeckFromField(currentKey,cardOnField, deckData );
			}
		}

		public void ReviveCard(int cardID)
		{
			int currentKey = modules.GetGameModule().GetCurrentKey();

			//Ha kézben kell keresni a választott lapot
			if (modules.GetGameModule().GetCurrentListType() == CardListTarget.Losers)
			{
				//Adatok kinyerése
				Card cardData = modules.GetDataModule().GetCardFromPlayer(currentKey, cardID, CardListTarget.Losers);

				//Model frissítése
				modules.GetDataModule().ReviveLostCard(currentKey, cardID);

				//UI frissítése
				modules.GetClientModule().DrawNewCard(cardData, currentKey, modules.GetGameModule().IsTheActivePlayerHuman(), DrawType.Normal, DrawTarget.Hand, SkillState.NotDecided);

				int winSize = modules.GetDataModule().GetWinnerAmount(currentKey);
				int lostSize = modules.GetDataModule().GetLostAmount(currentKey);
				Sprite win = modules.GetDataModule().GetLastWinnerImage(currentKey);
				Sprite lost = modules.GetDataModule().GetLastLostImage(currentKey);

				modules.GetClientModule().ChangePileText(winSize, lostSize, currentKey, win, lost);
			}
		}

		//Megváltoztatásra kerül az aktív harctípus
		public IEnumerator TriggerStatChange()
		{
			if (modules.GetGameModule().IsTheActivePlayerHuman())
			{
				modules.GetGameModule().ShowStatBox();
				yield return modules.GetGameModule().WaitForEndOfAction();
			}

			else
			{
				int currentKey = modules.GetGameModule().GetCurrentKey();
				modules.GetAImodule().DecideNewStat(currentKey);
				modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
			}
		}

		//Megváltoztatja a harc típusát egy megadott értékre
		public void SetActiveStat(CardStatType newStat)
		{
			modules.GetGameModule().SetActiveStat(newStat);
			modules.GetClientModule().RefreshStatDisplay();
		}

		//A játékos választ egy előre definiált paraméterekkel rendelkező kártyalistából
        public void MakePlayerChooseCard(CardListFilter filter = CardListFilter.None, int limit = 0)
        {
			int currentKey = modules.GetGameModule().GetCurrentKey();

			//Megjelenítjük a kézben lévő lapokat, hogy a játékos választhasson közülük
			ChooseCard(currentKey, filter, limit);
        }

        //A megadott paraméterekkel megjelenít egy kártya listát, amiből választhat a játékos
		private void ChooseCard(int currentKey, CardListFilter filter, int limit)
		{

			//Ha ember
			if (modules.GetGameModule().IsTheActivePlayerHuman())
			{
				modules.GetGameModule().StartCoroutine(modules.GetClientModule().DisplayCardList(currentKey, filter, limit));
			}

			//Ha bot
			else
			{
				modules.GetGameModule().StartCoroutine(modules.GetAImodule().CardSelectionEffect(currentKey, filter, limit));
			}
		}

		//A játékos listáról választott lapjával kezdünk valamit
		public void HandleSelectionForSkill(int selectedCard, int currentKey)
		{
			//Ha az akció a csere volt
            if (modules.GetGameModule().GetCurrentAction() == SkillEffectAction.Switch)
            {
                SwitchCard(currentKey, modules.GetGameModule().GetActiveCardID(), selectedCard);
                modules.GetGameModule().SetCurrentAction(SkillEffectAction.None);
                modules.GetGameModule().ActionFinished();
            }

            //Ha az akció skill lopás volt
            else if (modules.GetGameModule().GetCurrentAction() == SkillEffectAction.SkillUse)
            {
                Card otherCard = modules.GetDataModule().GetCardFromPlayer(currentKey, selectedCard, CardListTarget.Losers);

                //Másik Devil crok képesség esetén passzolunk
                if (otherCard.GetCardID() == 3)
                {
                    modules.GetGameModule().Pass();
                    modules.GetGameModule().SetCurrentAction(SkillEffectAction.None);
                }

                //Amúgy meg használjuk a másik lap képességét
                else
                {
                    UseSkill(otherCard.GetCardID());
                    return;
                }

            }

            else if (modules.GetGameModule().GetCurrentAction() == SkillEffectAction.Revive)
            {
                ReviveCard(selectedCard);
                modules.GetGameModule().SetCurrentAction(SkillEffectAction.None);
                modules.GetGameModule().ActionFinished();
            }

            else if(modules.GetGameModule().GetCurrentAction() == SkillEffectAction.TossCard)
            {
                modules.GetDataModule().TossCardFromHand(currentKey, selectedCard);
                modules.GetClientModule().TossCard(currentKey, selectedCard);
                modules.GetGameModule().SetCurrentAction(SkillEffectAction.None);
                modules.GetGameModule().ActionFinished();
            }

            else if(modules.GetGameModule().GetCurrentAction() == SkillEffectAction.SwitchOpponentCard)
            {
                modules.GetGameModule().SetSwitchType(CardListTarget.Deck);
                int deckCardID = (modules.GetDataModule().GetDeckAmount(currentKey)) - 1 ;
                SwitchCard(currentKey, selectedCard, deckCardID);
            }

            else if(modules.GetGameModule().GetCurrentAction() == SkillEffectAction.SacrificeFromHand)
            {
            	//Kártya adatok bekérése
            	Card ownCard = modules.GetDataModule().GetCardFromPlayer(currentKey, modules.GetGameModule().GetActiveCardID(), CardListTarget.Field);
            	Card sacrificed = modules.GetDataModule().GetCardFromPlayer(currentKey, selectedCard, CardListTarget.Hand);

            	//Bónusz meghatározása és aktiválása
            	int ownPower = ownCard.GetPower();
            	int sacrPower = sacrificed.GetPower();
        		ownCard.AddBonus(new StatBonus((sacrPower - ownPower),0,0));

        		//Választott kártya eldobása
                modules.GetDataModule().TossCardFromHand(currentKey, selectedCard);
                modules.GetClientModule().TossCard(currentKey, selectedCard);

                modules.GetGameModule().SetCurrentAction(SkillEffectAction.None);
                modules.GetGameModule().ActionFinished();
            }

            else if (modules.GetGameModule().GetCurrentAction() == SkillEffectAction.SacrificeDoppelganger) 
            {
            	//Kártya adatok bekérése
            	Card ownCard = modules.GetDataModule().GetCardFromPlayer(currentKey, modules.GetGameModule().GetActiveCardID(), CardListTarget.Field);
            	Card sacrificed = modules.GetDataModule().GetCardFromPlayer(currentKey, selectedCard, CardListTarget.Hand);

        		//Választott kártya eldobása
                modules.GetDataModule().TossCardFromHand(currentKey, selectedCard);
                modules.GetClientModule().TossCard(currentKey, selectedCard);

                modules.GetGameModule().MakeWinner(currentKey);
            	modules.GetGameModule().ActionFinished();
            }

            modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
		}

		#endregion


	}
}