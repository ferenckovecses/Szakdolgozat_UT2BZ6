using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
    public class AI_Controller
    {
        private GameState_Controller gameState;

        //AI Context
        private List<Card> cardsOnField;
        private List<List<Card>> opponentCards;
        private List<Card> cardsInHand;
        private int winAmount;
        private int currentKey;
        private int cardCount;
        private CardStatType currentStat;
        private Data_Controller data;
        private Input_Controller input;
        private Client_Controller client;


        public AI_Controller(GameState_Controller in_gameState)
        {
            this.gameState = in_gameState;
            List<Card> cardsOnField = new List<Card>();
            List<List<Card>> opponentCards = new List<List<Card>>();
            List<Card> cardsInHand = new List<Card>();
            this.data = gameState.GetDataModule();
            this.client = gameState.GetClientModule();
            this.input = gameState.GetInputModule();
        }

        public void SummonCard(int playerKey)
        {
            GetCurrentContext(playerKey);

            //AI agy segítségét hívjuk a döntésben
            int cardIndex = Bot_Behaviour.ChooseRightCard(this.cardsInHand, this.currentStat);

            //Lerakatjuk a kártyát a UI-ban
            gameState.StartCoroutine(client.SummonCard(currentKey, cardIndex));

        }

        //Dönt a képességeiről
        public IEnumerator DecideSkill(int key)
        {
            GetCurrentContext(key);

            for(var id = 0; id < cardCount; id++)
            {
                if(client.AskCardSkillStatus(currentKey, id) == SkillState.NotDecided)
                {
                    //GetSkillChoiceFromAIBrain
                    input.ReportSkillStatusChange(SkillState.Use, id, false);
                    yield return gameState.WaitForEndOfSkill();

                    client.SetSkillState(currentKey, id, SkillState.Use);
                    
                }
            }
            yield return new WaitForSeconds(0.01f);

            //Az AI végzett a körével a döntések után
            data.GetPlayerWithKey(currentKey).SetStatus(PlayerTurnStatus.Finished);

            gameState.TurnFinished();
        }

        //Dönt az új statról
        public void DecideNewStat(int key = -1)
        {
            GetCurrentContext(key);

            //AI agy segítségét hívjuk a döntésben
            CardStatType newStat = Bot_Behaviour.ChangeFightType(cardsOnField,
                opponentCards, currentStat);

            gameState.SetActiveStat(newStat);

            //Jelenítsük meg a változást
            client.RefreshStatDisplay();
        }

        //Kártyacsere vagy választás
        public IEnumerator CardSelectionEffect(int key, CardListFilter filter, int limit = 0, int otherPlayer = -1)
        {
            GetCurrentContext(key);

            //Ha cseréről van szó
            if (gameState.GetCurrentAction() == SkillEffectAction.Switch)
            {
                int handID = -1;

                //Ha kézből cserélünk
                if (gameState.GetCurrentListType() == CardListTarget.Hand)
                {
                    handID = Bot_Behaviour.HandSwitch(data.GetCardsFromHand(currentKey),
                        data.GetCardsFromField(currentKey), data.GetOpponentsCard(currentKey), currentStat);

                    gameState.GetSkillModule().SwitchCard(currentKey, gameState.GetActiveCardID(), handID);
                }
            }

            //Ha skill lopásról van szó
            else if (gameState.GetCurrentAction() == SkillEffectAction.SkillUse)
            {
                //Adatgyűjtés
                Card ownCard = data.GetCardFromField(currentKey, gameState.GetActiveCardID());
                List<PlayerCardPairs> cards = data.GetOtherLosers(currentKey);
                int index = Bot_Behaviour.WhichSkillToUse(cards);
                int otherKey = cards[index].playerKey;
                Card otherCard = cards[index].card;
                List<SkillProperty> temp = otherCard.GetSkillProperty();

                //Skill tulajdonságok módosítása
                ownCard.ModifySkills(temp);
                ownCard.SetSKillID(otherCard.GetCardID());

                //Új skill használata
                gameState.GetSkillModule().UseSkill(ownCard.GetCardID());
                yield return null;
            }

            //Ha felélesztésről van szó
            else if (gameState.GetCurrentAction() == SkillEffectAction.Revive)
            {
                List<Card> cards = data.GetLostList(currentKey);
                int cardID = Bot_Behaviour.WhomToRevive(cards);
                gameState.GetSkillModule().ReviveCard(cardID);
            }

            else if(gameState.GetCurrentAction() == SkillEffectAction.Execute)
             {
                List<int> temp = data.GetOtherKeyList(currentKey);
                int choosenKey = Bot_Behaviour.WhichPlayerToExecute(temp);

                input.ReportNameBoxTapping(choosenKey);
                yield return gameState.WaitForEndOfAction();
             }

             else if(gameState.GetCurrentAction() == SkillEffectAction.SwitchOpponentCard)
             {
                List<int> temp = data.GetOtherKeyList(currentKey);
                int choosenKey = Bot_Behaviour.WhichPlayerToExecute(temp);

                input.ReportNameBoxTapping(choosenKey);
                yield return gameState.WaitForEndOfAction();
             }

             else if(gameState.GetCurrentAction() == SkillEffectAction.PickCardForSwitch)
             {
                List<Card> cardsOnField = data.GetCardsFromField(otherPlayer);
                int choosenCardID = Bot_Behaviour.WhichCardToSwitch(cardsOnField);
                int deckCount = (data.GetDeckAmount(otherPlayer)) - 1;
                gameState.SetSwitchType(CardListTarget.Deck);
                gameState.GetSkillModule().SwitchCard(otherPlayer, choosenCardID, deckCount);
             }

             else if(gameState.GetCurrentAction() == SkillEffectAction.CheckWinnerAmount)
             {
                List<int> keyList = data.GetOtherKeyList(currentKey);
                List<int> winAmounts = data.GetOthersWinAmount(currentKey);

                int playerID = Bot_Behaviour.WhichPlayerToChoose(winAmounts);

                input.ReportNameBoxTapping(keyList[playerID]);
             }

            //Ha kézből eldobásról van szó
            else if (gameState.GetCurrentAction() == SkillEffectAction.TossCard)
            {
                //Csak akkor dobunk el lapot, ha van a kezünkben
                if(cardsInHand.Count > 1)
                {
                    int cardID = Bot_Behaviour.WhomToToss(cardsInHand);
                    input.HandleCardSelection(cardID, currentKey);
                }

                else 
                {
                    Debug.Log("Nincs mit eldobni");
                    gameState.ActionFinished();
                }
            }

            else if(gameState.GetCurrentAction() == SkillEffectAction.SacrificeFromHand)
            {
                int cardID = Bot_Behaviour.WhomToToss(cardsInHand);
                input.HandleCardSelection(cardID, currentKey);
            }

            else if(gameState.GetCurrentAction() == SkillEffectAction.SacrificeDoppelganger)
            {
                List<Card> cardList = data.GetCardsFromHand(currentKey, filter);
                if(cardList.Count > 0)
                {
                    int cardID = Bot_Behaviour.WhomToToss(cardList);
                    input.HandleCardSelection(cardID, currentKey);
                }

                else 
                {
                    Debug.Log("Nincs megfelelő lap a kézben");
                    gameState.ActionFinished();
                }
            }

            gameState.StartCoroutine(gameState.SkillFinished());
        }

        //Lekéri az aktuális számára is elérhető adatokat
        private void GetCurrentContext(int key)
        {
            this.currentKey = key;
            this.cardsOnField = data.GetPlayerWithKey(currentKey).GetCardsOnField();
            this.cardCount = cardsOnField.Count;
            this.opponentCards = data.GetOpponentsCard(currentKey);
            this.cardsInHand = data.GetPlayerWithKey(currentKey).GetCardsInHand();
            this.winAmount = data.GetWinnerAmount(currentKey);
            this.currentStat = gameState.GetActiveStat();
        }
    }
}
