using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClientControll;

namespace GameControll
{
    public class Client_Controller
    {
        private GameState_Controller gameState;
        private Client client;
        private Input_Controller input;

        public Client_Controller(GameState_Controller controller_m, Client client_m)
        {
            this.gameState = controller_m;
            this.client = client_m;
            this.input = gameState.GetInputModule();

            client.SetReportAgent(input);
        }

        //Legenerálja a UI felületet
        public void GenerateUI(int numberOfPlayers, List<int> playerKeys, List<string> playerNames, int playerDeckSize)
        {
            client.CreatePlayerFields(numberOfPlayers, playerKeys, playerNames, playerDeckSize);
        }
        //Beállíthatjuk, hogy az adott játékos rakhat-e kártyát most, vagy sem
        public void SetDragStatus(int playerKey, bool newStatus)
        {
            client.GetFieldFromKey(playerKey).SetDraggableStatus(newStatus);
        }

        //Beállítjuk, hogy a megadott játékos rendelkezhet-e képességeiről
        public void SetSkillStatus(int playerKey, bool newStatus)
        {
            client.GetFieldFromKey(playerKey).SetSkillStatus(newStatus);
        }

        //Botok UI-jának küld egy parancsot egy megadott indexű kártya lerakására/aktiválására
        public IEnumerator SummonCard(int playerKey, int cardHandIndex)
        {
            yield return new WaitForSeconds(GameSettings_Controller.drawTempo * UnityEngine.Random.Range(15, 30));

            client.GetFieldFromKey(playerKey).SummonCard(cardHandIndex);
            gameState.TurnFinished();
        }

        //Frissíti a játékos paklijának méretét a modell szerinti aktuális értékre
        public void RefreshDeckSize(int playerKey, int newDeckSize)
        {
            client.GetFieldFromKey(playerKey).UpdateDeckCounter(newDeckSize);
        }

        //Felfedi a lerakott kártyát
        public void RevealCards(int playerKey)
        {
            client.GetFieldFromKey(playerKey).RevealCardsOnField();
        }

        //Elrakatja a UI-on megjelenített kártyákat a megfelelő helyre
        public IEnumerator PutCardsAway(int playerKey, bool isWinner)
        {
            yield return new WaitForSeconds(1f);
            client.GetFieldFromKey(playerKey).PutCardsAway(isWinner);
        }

        //Frissítést küld a játékpanelen az aktuális harctípus szövegére
        public void RefreshStatDisplay()
        {
            string statText;

            switch (gameState.GetActiveStat())
            {
                case CardStatType.Power: statText = "Erő"; break;
                case CardStatType.Intelligence: statText = "Intelligencia"; break;
                case CardStatType.Reflex: statText = "Reflex"; break;
                default: statText = "Harctípus"; break;
            }

            client.ChangeStatText(statText);
        }

        //Megjeleníti a játékos számára a harctípus választó képernyőt
        public IEnumerator DisplayStatBox()
        {
            while (gameState.IsMessageOnScreen())
            {
                yield return null;
            }

            client.DisplayStatBox();
        }

        //Értesítő üzenetet jelenít meg a UI felületen
        public IEnumerator DisplayNotification(string msg)
        {
            //Megjelenítjük az üzenetet
            client.DisplayMessage(msg);
            gameState.SetMessageStatus(true);

            //Adunk időt a játékosoknak, hogy elolvassák
            yield return new WaitForSeconds(GameSettings_Controller.textTempo);

            //Eltüntetjük az üzenetet
            client.HideMessage();
            gameState.SetMessageStatus(false);
        }

        //Megjeleníttet az UI felülettel egy listányi kártyát, amiből választani kell
        public void CardChoice(List<Card> cardData, SkillEffectAction action, int key, string msg)
        {
            client.DisplayListOfCards(cardData, action, key, msg);
        }

        //Frissíti a győztes és vesztes halmok képét és darabszámát
        public void ChangePileText(int winSize, int lostSize, int playerKey, Sprite win, Sprite lost)
        {
            client.GetFieldFromKey(playerKey).ChangePileText(winSize, lostSize, win, lost);
        }

        //Új Skill ciklus: A tartalékolt képességekről újra lehet dönteni
        public void NewSkillCycle(int playerKey)
        {
            client.GetFieldFromKey(playerKey).NewSkillCycle();
        }

        //A megadott játékosnál lévő és megadott pozícióban található kártya képességét reseteli
        public void ResetCardSkill(int playerKey, int cardPosition)
        {
            client.GetFieldFromKey(playerKey).ResetCardSkill(cardPosition);
        }

        public void SwitchHandFromField(int playerKey, Card fieldData, int fieldId, Card handData, int handId, bool visibility)
        {
            client.GetFieldFromKey(playerKey).SwitchHandFromField(fieldData, fieldId, handData, handId, visibility);
        }

        public void SwitchDeckFromField(int playerKey, int fieldId, Card deckData)
        {
            client.GetFieldFromKey(playerKey).SwitchDeckFromField(fieldId, deckData);
        }

        public bool AskSkillStatus(int playerKey)
        {
            return client.GetFieldFromKey(playerKey).GetCardSkillStatusFromField();
        }

        public SkillState AskCardSkillStatus(int playerKey, int cardID)
        {
            return client.GetFieldFromKey(playerKey).GetSpecificCardSkillStatus(cardID);
        }

        public void SetSkillState(int playerKey, int cardID, SkillState newSkillState)
        {
            client.GetFieldFromKey(playerKey).SetSpecificCardSkillStatus(cardID, newSkillState);
        }

        public void DrawNewCard(Card data, int playerKey, bool visibleForPlayer, DrawType drawType, DrawTarget target, SkillState newState)
        {
            client.DisplayNewCard(data, playerKey, visibleForPlayer, drawType, target, newState);
        }

        //Bizonyos kártya listát jelenít meg az aktív játékos számára, amiből választhat ezután
        public IEnumerator DisplayCardList(int currentKey, CardListFilter filter = CardListFilter.None, int limit = 0)
        {

            List<Card> cardList = new List<Card>();
            Data_Controller dataModule = gameState.GetDataModule();

            switch (gameState.GetCurrentListType())
            {
                case CardListTarget.Hand: cardList = dataModule.GetCardsFromHand(currentKey, filter); break;
                case CardListTarget.Winners: cardList = dataModule.GetPlayerWithKey(currentKey).GetWinners(); break;
                case CardListTarget.Losers: cardList = dataModule.GetPlayerWithKey(currentKey).GetLosers(); break;
                case CardListTarget.Deck: cardList = dataModule.GetPlayerWithKey(currentKey).GetDeck(limit); break;
                case CardListTarget.Field: cardList = dataModule.GetCardsFromField(currentKey); break;
                default: break;
            }
            SkillEffectAction currentAction = gameState.GetCurrentAction();

            string msg = "";
            switch (currentAction) 
            {
                case SkillEffectAction.Switch: 
                    msg = "Válaszd ki, hogy melyik lappal cserélsz!"; 
                    break;

                case SkillEffectAction.Revive: 
                    msg = "Válaszd ki, hogy melyik lapot éleszted fel!";
                    break;

                case SkillEffectAction.SwitchOpponentCard: 
                    msg = "Válaszd ki, hogy melyik ellenséges lapot cseréled ki!";
                    break;

                case SkillEffectAction.SacrificeFromHand: 
                    msg = "Válaszd ki, hogy melyik lapot áldozod fel az Erőért!";
                    break;

                default:
                    break;
            }

            //Ha van miből választani, akkor megjelenítjük a listát
            if(cardList.Count > 0)
            {
                CardChoice(cardList, currentAction, currentKey, msg);

                //Várunk a visszajelzésére
                yield return gameState.WaitForEndOfAction();
            }

            else 
            {
                if(currentAction == SkillEffectAction.SacrificeDoppelganger)
                {
                    gameState.StartCoroutine(DisplayNotification("Nincs megfelelő lap a kezedben!"));
                }

                gameState.SetSelectionAction(SkillEffectAction.None);
                gameState.SetSwitchType(CardListTarget.None);
                gameState.StartCoroutine(gameState.SkillFinished());
            }

        }

        public void SetEndTurnButton(bool newStatus)
        {
            client.SetTurnButtonStatus(newStatus);
        }

        public void HandleRemainingCards(int playerKey)
        {
            client.GetFieldFromKey(playerKey).HandleRemainingCards();
            input.CheckIfFinished();
        }

        public void TossCard(int playerKey, int cardID)
        {
            client.GetFieldFromKey(playerKey).Toss(cardID);
        }

        #region Client State Modifiers

        public void StartOfRound()
        {
            client.SetClientStates(ClientStates.WaitingForTurn);
        }

        public void WaitForSummon()
        {
            client.SetClientStates(ClientStates.WaitingForSummon);
        }

        public void SummonEnded()
        {
            client.SetClientStates(ClientStates.SummonFinished);
        }

        public void WaitForSkill()
        {
            client.SetClientStates(ClientStates.WaitingForSkills);
        }

        public void SkillStateEnded()
        {
            client.SetClientStates(ClientStates.SkillStateFinished);
        }

        public void EndOfRound()
        {
            client.SetClientStates(ClientStates.RoundEnded);
        }

        #endregion

    }
}
