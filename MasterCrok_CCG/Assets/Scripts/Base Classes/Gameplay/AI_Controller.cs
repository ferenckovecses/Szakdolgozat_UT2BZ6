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
        public IEnumerator DecideSkill(int key = -1)
        {
            GetCurrentContext(key);

            for(var id = 0; id < cardCount; id++)
            {
                if(gameState.GetClientModule().AskCardSkillStatus(currentKey, id) == SkillState.NotDecided)
                {

                    gameState.GetInputModule().ReportSkillStatusChange(SkillState.Use, id);
                    yield return gameState.WaitForEndOfSkill();
                    gameState.GetClientModule().SetSkillState(currentKey, id, SkillState.Use);
                    
                }
            }

            //Az AI végzett a körével a döntések után
            gameState.GetDataModule().GetPlayerWithKey(currentKey).SetStatus(PlayerTurnStatus.Finished);
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
        public IEnumerator CardSelectionEffect(CardListFilter filter, int limit, int key = -1)
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

                    gameState.GetSkillModule().SwitchCard(gameState.GetActiveCardID(), handID);
                }
            }

            //Ha skill lopásról van szó
            else if (gameState.GetCurrentAction() == SkillEffectAction.SkillUse)
            {
                List<Card> cards = dataModule.GetOtherLosers(currentKey);
                int cardID = Bot_Behaviour.WhichSkillToUse(cards);
                gameState.GetSkillModule().UseSkill(cardID);
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

            //Ha kézből eldobásról van szó
            else if (gameState.GetCurrentAction() == SkillEffectAction.TossCard)
            {
                //Csak akkor dobunk el lapot, ha van a kezünkben
                if(cardsInHand.Count > 0)
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
