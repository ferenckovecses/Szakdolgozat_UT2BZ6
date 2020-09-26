using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
    public class AI_Controller
    {
        private GameState_Controller gameState;

        //AI Context
        List<Card> cardsOnField;
        List<List<Card>> opponentCards;
        List<Card> cardsInHand;
        int winAmount;
        int currentKey;
        int cardCount;
        CardStatType currentStat;


        public AI_Controller(GameState_Controller cont)
        {
            this.gameState = cont;
            List<Card> cardsOnField = new List<Card>();
            List<List<Card>> opponentCards = new List<List<Card>>();
            List<Card> cardsInHand = new List<Card>();
        }

        //Dönt a képességeiről
        public IEnumerator DecideSkill()
        {
            int id = 0;
            GetCurrentContext();

            //Addig megy a ciklus újra és újra, amíg minden lapjáról nem döntött az AI
            while (true)
            {

                //Ha a field szerint nincs több inaktív lap, akkor végeztünk
                if (gameState.GetClientModule().AskSkillStatus(currentKey))
                {
                    break;
                }

                //Ha a vizsgált kártya skill státusza Not Decided
                if (gameState.GetClientModule().AskCardSkillStatus(currentKey, id) == SkillState.NotDecided)
                {
                    gameState.SetActiveCardID(id);
                    //AI agy segítségét hívjuk a döntésben, átadjuk neki a szükséges infót és választ kapunk cserébe
                    SkillChoises response = Bot_Behaviour.ChooseSkill(id, cardsInHand, cardsOnField, opponentCards, winAmount);

                    //Kis várakozás, gondolkodási idő imitálás a döntés meghozása előtt, hogy ne történjen minden túl hirtelen
                    yield return new WaitForSeconds(GameSettings_Controller.drawTempo * UnityEngine.Random.Range(5, 15));

                    //Az AI döntése alapján itt végzi el a játék a megfelelő akciókat
                    switch (response)
                    {
                        case SkillChoises.Pass: 
                            gameState.Use(id);
                            gameState.GetClientModule().SetSkillState(currentKey, id, SkillState.Pass); 
                            break;
                        case SkillChoises.Store: 
                            gameState.Use(id);
                            gameState.GetClientModule().SetSkillState(currentKey, id, SkillState.Store); 
                            break;
                        case SkillChoises.Use: 
                            gameState.Use(id);
                            gameState.GetClientModule().SetSkillState(currentKey, id, SkillState.Use); 
                            break;
                        default: gameState.Pass(); break;
                    }

                    //Újrakezdjük a ciklust
                    id = 0;
                }

                //Ellenkező esetben ha megtehetjük, léptetjük a ciklust
                else
                {
                    if (id + 1 < cardCount)
                    {
                        id++;
                    }
                }
            }

            //Az AI végzett a körével a döntések után
            gameState.GetDataModule().GetPlayerWithKey(currentKey).SetStatus(PlayerTurnStatus.Finished);
            gameState.ActionFinished();
        }

        //Dönt az új statról
        public void DecideNewStat()
        {
            GetCurrentContext();

            //AI agy segítségét hívjuk a döntésben
            CardStatType newStat = Bot_Behaviour.ChangeFightType(cardsOnField,
                opponentCards, currentStat);

            gameState.SetActiveStat(newStat);

            //Jelenítsük meg a változást
            gameState.GetClientModule().RefreshStatDisplay();
        }

        //Kártyacsere vagy választás
        public void CardSelectionEffect(CardListFilter filter, int limit)
        {
            GetCurrentContext();
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
            }

            //Ha felélesztésről van szó
            else if (gameState.GetCurrentAction() == SkillEffectAction.Revive)
            {
                List<Card> cards = dataModule.GetLostList(currentKey);
                int cardID = Bot_Behaviour.WhomToRevive(cards);
                gameState.GetSkillModule().ReviveCard(cardID);
            }

            gameState.SetCurrentAction(SkillEffectAction.None);
        }

        //Lekéri az aktuális számára is elérhető adatokat
        private void GetCurrentContext()
        {
            //Aktuális adatok lekérése
            this.currentKey = gameState.GetCurrentKey();
            this.cardsOnField = gameState.GetDataModule().GetPlayerWithKey(currentKey).GetCardsOnField();
            this.cardCount = cardsOnField.Count;
            this.opponentCards = gameState.GetDataModule().GetOpponentsCard(currentKey);
            this.cardsInHand = gameState.GetDataModule().GetPlayerWithKey(currentKey).GetCardsInHand();
            this.winAmount = gameState.GetDataModule().GetWinnerAmount(currentKey);
            this.currentStat = gameState.GetActiveStat();
        }
    }
}
