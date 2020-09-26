using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
    public class Input_Controller
    {
        GameState_Controller gameState;

        public Input_Controller(GameState_Controller in_gameState)
        {
            this.gameState = in_gameState;
        }

        //Az input alapján módosítjuk az aktív értéktípust az újra
        public void ReportStatChange(CardStatType newStat)
        {
            //Kitörés a wait fázisból
            gameState.ActionFinished();

            //Választott érték beállítása
            gameState.SetActiveStat(newStat);

            //Jelenítsük meg a változást
            gameState.GetClientModule().RefreshStatDisplay();
        }

        //Játékos kártyájának skill helyzetében történt válotoztatás kezelése
        public void ReportSkillStatusChange(SkillState state, int cardPosition)
        {
            switch (state)
            {
                case SkillState.Pass: gameState.Pass(); break;
                case SkillState.Use: gameState.SetActiveCardID(cardPosition); gameState.Use(gameState.GetActiveCardID()); break;
                case SkillState.Store: gameState.StartCoroutine(gameState.Store(cardPosition)); break;
                default: gameState.Pass(); break;
            }

            //Ha az összes pályán lévő kártya skilljéről nyilatkozott a játékos
            if (gameState.GetClientModule().AskSkillStatus(gameState.GetCurrentKey()))
            {
                //Ha van köztük stored érték, akkor még nem végeztünk
                if (gameState.GetStoreCount() == 0)
                {
                    gameState.GetDataModule().GetPlayerWithKey(gameState.GetCurrentKey()).SetStatus(PlayerTurnStatus.Finished);
                }
            }
        }

        //Játékos idézett
        public void ReportSummon(int indexInHand)
        {
            gameState.ActionFinished();
            gameState.GetDataModule().GetPlayerWithKey(gameState.GetCurrentKey()).PlayCardFromHand(indexInHand);
        }

        //Játékos kilépett
        public void ReportExit()
        {
            gameState.GetDataModule().ResetDecks();
        }

        //Játékos lapot választott a listáról
        public void ReportCardSelection(int id)
        {
            gameState.StartCoroutine(HandleCardSelection(id));
        }

        //Kezeljük a játékos kiválasztott kártyáját
        public IEnumerator HandleCardSelection(int selectedCard)
        {

            //Ha az akció tartalékolás volt
            if (gameState.GetCurrentAction() == SkillEffectAction.Store)
            {
                int currentKey = gameState.GetCurrentKey();

                //A modelben frissítjük a helyzetet
                gameState.GetDataModule().SacrificeWinnerCard(selectedCard, currentKey);

                //UI oldalon is frissítjük a helyzetet, hogy aktuális állapotot tükrözzön
                int winSize = gameState.GetDataModule().GetWinnerAmount(currentKey);
                int lostSize = gameState.GetDataModule().GetLostAmount(currentKey);
                Sprite win = gameState.GetDataModule().GetLastWinnerImage(currentKey);
                Sprite lost = gameState.GetDataModule().GetLastLostImage(currentKey);
                gameState.GetClientModule().ChangePileText(winSize, lostSize, currentKey, win, lost);
                gameState.ActionFinished();
            }

            //Ha az akció a csere volt
            else if (gameState.GetCurrentAction() == SkillEffectAction.Switch)
            {
                gameState.GetSkillModule().SwitchCard(gameState.GetActiveCardID(), selectedCard);
                gameState.ActionFinished();
            }

            //Ha az akció skill lopás volt
            else if (gameState.GetCurrentAction() == SkillEffectAction.SkillUse)
            {

                //Másik Devil crok képesség esetén passzolunk
                if (selectedCard == 3)
                {
                    gameState.Pass();
                    gameState.ActionFinished();
                }

                //Amúgy meg használjuk a másik lap képességét
                else
                {
                    gameState.GetSkillModule().UseSkill(selectedCard);
                    yield return gameState.WaitForEndOfAction();
                }

            }

            else if (gameState.GetCurrentAction() == SkillEffectAction.Revive)
            {
                gameState.GetSkillModule().ReviveCard(selectedCard);
                gameState.ActionFinished();
            }


            gameState.SetCurrentAction(SkillEffectAction.None);
        }

        //Játékos kilépett a kártya választó képernyőről
        public void ReportSelectionCancel()
        {
            gameState.GetClientModule().ResetCardSkill(gameState.GetCurrentKey(), gameState.GetActiveCardID());
        }

        public void ReportDisplayRequest(CardListTarget listType, int playerKey)
        {
            List<Card> cardList = new List<Card>();

            switch (listType)
            {
                case CardListTarget.Winners: cardList = gameState.GetDataModule().GetPlayerWithKey(playerKey).GetWinners(); break;
                case CardListTarget.Losers: cardList = gameState.GetDataModule().GetPlayerWithKey(playerKey).GetLosers(); break;
                default: break;
            }

            //Lélekrablás esetén saját vesztes lapokat nem választhatunk ki, illetve mások győztes lapjait sem
            if (playerKey == gameState.GetCurrentKey() && gameState.GetCurrentAction() == SkillEffectAction.SkillUse 
                || listType == CardListTarget.Winners)

            {
                gameState.GetClientModule().CardChoice(cardList, SkillEffectAction.None);
            }

            else
            {
                gameState.GetClientModule().CardChoice(cardList, gameState.GetCurrentAction());
            }

        }
    }
}
