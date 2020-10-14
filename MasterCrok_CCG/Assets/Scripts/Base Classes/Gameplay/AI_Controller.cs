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


        public AI_Controller(GameState_Controller cont)
        {
            this.gameState = cont;
            List<Card> cardsOnField = new List<Card>();
            List<List<Card>> opponentCards = new List<List<Card>>();
            List<Card> cardsInHand = new List<Card>();
        }

        //Dönt a képességeiről
        public IEnumerator DecideSkill(int key)
        {
            GetCurrentContext(key);

            Debug.Log("Before the loop");
            for(var id = 0; id < cardCount; id++)
            {
                Debug.Log("Start of the loop");
                if(gameState.GetClientModule().AskCardSkillStatus(currentKey, id) == SkillState.NotDecided)
                {
                    Debug.Log("In the If");
                    //GetSkillChoiceFromAIBrain
                    gameState.GetInputModule().ReportSkillStatusChange(SkillState.Use, id, false);
                    yield return gameState.WaitForEndOfSkill();

                    gameState.GetClientModule().SetSkillState(currentKey, id, SkillState.Use);
                    
                }
            }
            yield return new WaitForSeconds(0.01f);

            Debug.Log("End of the loop");

            //Az AI végzett a körével a döntések után
            gameState.GetDataModule().GetPlayerWithKey(currentKey).SetStatus(PlayerTurnStatus.Finished);

            Debug.Log("Turn Finished");
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
            gameState.GetClientModule().RefreshStatDisplay();
        }

        //Kártyacsere vagy választás
        public IEnumerator CardSelectionEffect(int key, CardListFilter filter, int limit = 0, int otherPlayer = -1)
        {
            GetCurrentContext(key);
            Data_Controller dataModule = gameState.GetDataModule();

            //Ha cseréről van szó
            if (gameState.GetCurrentAction() == SkillEffectAction.Switch)
            {
                int handID = -1;

                //Ha kézből cserélünk
                if (gameState.GetCurrentListType() == CardListTarget.Hand)
                {
                    handID = Bot_Behaviour.HandSwitch(dataModule.GetCardsFromHand(currentKey),
                        dataModule.GetCardsFromField(currentKey), dataModule.GetOpponentsCard(currentKey), currentStat);

                    gameState.GetSkillModule().SwitchCard(currentKey, gameState.GetActiveCardID(), handID);
                }
            }

            //Ha skill lopásról van szó
            else if (gameState.GetCurrentAction() == SkillEffectAction.SkillUse)
            {
                //Adatgyűjtés
                Card ownCard = dataModule.GetCardFromField(currentKey, gameState.GetActiveCardID());
                List<PlayerCardPairs> cards = dataModule.GetOtherLosers(currentKey);
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
                List<Card> cards = dataModule.GetLostList(currentKey);
                int cardID = Bot_Behaviour.WhomToRevive(cards);
                gameState.GetSkillModule().ReviveCard(cardID);
            }

            else if(gameState.GetCurrentAction() == SkillEffectAction.Execute)
             {
                List<int> temp = dataModule.GetOtherKeyList(currentKey);
                int choosenKey = Bot_Behaviour.WhichPlayerToExecute(temp);

                gameState.GetInputModule().ReportNameBoxTapping(choosenKey);
                yield return gameState.WaitForEndOfAction();
             }

             else if(gameState.GetCurrentAction() == SkillEffectAction.SwitchOpponentCard)
             {
                Debug.Log("Switch Opponent Card");
                List<int> temp = dataModule.GetOtherKeyList(currentKey);
                int choosenKey = Bot_Behaviour.WhichPlayerToExecute(temp);

                gameState.GetInputModule().ReportNameBoxTapping(choosenKey);
                yield return gameState.WaitForEndOfAction();
             }

             else if(gameState.GetCurrentAction() == SkillEffectAction.PickCardForSwitch)
             {
                Debug.Log("Pick Card To Switch");
                List<Card> cardsOnField = dataModule.GetCardsFromField(otherPlayer);
                int choosenCardID = Bot_Behaviour.WhichCardToSwitch(cardsOnField);
                int deckCount = (dataModule.GetDeckAmount(otherPlayer)) - 1;
                gameState.SetSwitchType(CardListTarget.Deck);
                gameState.GetSkillModule().SwitchCard(otherPlayer, choosenCardID, deckCount);

                Debug.Log("Key: " + otherPlayer.ToString());
                Debug.Log("Deck: " + deckCount.ToString());
             }

             else if(gameState.GetCurrentAction() == SkillEffectAction.CheckWinnerAmount)
             {
                List<int> keyList = dataModule.GetOtherKeyList(currentKey);
                List<int> winAmounts = dataModule.GetOthersWinAmount(currentKey);

                int playerID = Bot_Behaviour.WhichPlayerToChoose(winAmounts);

                gameState.GetInputModule().ReportNameBoxTapping(keyList[playerID]);
             }

            //Ha kézből eldobásról van szó
            else if (gameState.GetCurrentAction() == SkillEffectAction.TossCard)
            {
                //Csak akkor dobunk el lapot, ha van a kezünkben
                if(cardsInHand.Count > 1)
                {
                    int cardID = Bot_Behaviour.WhomToToss(cardsInHand);
                    gameState.GetInputModule().HandleCardSelection(cardID, currentKey);
                }

                else 
                {
                    Debug.Log("Nincs mit eldobni");
                    gameState.ActionFinished();
                }
            }

            gameState.StartCoroutine(gameState.SkillFinished());
        }

        //Lekéri az aktuális számára is elérhető adatokat
        private void GetCurrentContext(int key)
        {
            //Default eset: Aktuális játékos lekérdezése
            if(key == -1)
            {
                //Aktuális adatok lekérése
                this.currentKey = gameState.GetCurrentKey();
            }

            //Megadott kulcsú játékos lekérdezése
            else 
            {
                this.currentKey = key;
            }

            this.cardsOnField = gameState.GetDataModule().GetPlayerWithKey(currentKey).GetCardsOnField();
            this.cardCount = cardsOnField.Count;
            this.opponentCards = gameState.GetDataModule().GetOpponentsCard(currentKey);
            this.cardsInHand = gameState.GetDataModule().GetPlayerWithKey(currentKey).GetCardsInHand();
            this.winAmount = gameState.GetDataModule().GetWinnerAmount(currentKey);
            this.currentStat = gameState.GetActiveStat();
        }
    }
}
