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
            //Választott érték beállítása
            gameState.SetActiveStat(newStat);

            //Jelenítsük meg a változást
            gameState.GetClientModule().RefreshStatDisplay();

            if(gameState.GetGameState() == MainGameStates.SetStat)
            {
                //Kitörés a wait fázisból
                gameState.TurnFinished();
            }

            else
            {
                gameState.ActionFinished();    
            }
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

            CheckIfFinished();   
        }

        public void CheckIfFinished()
        {
            //Ha az összes pályán lévő kártya skilljéről nyilatkozott a játékos
            if (gameState.GetClientModule().AskSkillStatus(gameState.GetCurrentKey()))
            {
                //Ha nincs stored kártyánk, akkor végeztünk a skill fázissal
                if (gameState.GetStoreCount() == 0)
                {
                    gameState.GetDataModule().GetPlayerWithKey(gameState.GetCurrentKey()).SetStatus(PlayerTurnStatus.Finished);
                }
            }
        }

        //Játékos idézett
        public void ReportSummon(int indexInHand)
        {
            //Frissítjük a modellt
            gameState.GetDataModule().GetPlayerWithKey(gameState.GetCurrentKey()).PlayCardFromHand(indexInHand);

            //Elvesszük a játékostól a drag jogot
            gameState.GetClientModule().SetDragStatus(gameState.GetCurrentKey(), false);

            //Megjelenítjük a kör vége gombot
            gameState.GetClientModule().SetEndTurnButton(true);
        }

        //Játékos kilépett
        public void ReportExit()
        {
            gameState.GetDataModule().ResetDecks();
        }

        //Kezeljük a játékos kiválasztott kártyáját
        public void HandleCardSelection(int selectedCard, int playerKey = -1)
        {
            int currentKey;
            
            //Default eset: Az aktuális játékosra hatunk ki
            if(playerKey == -1)
            {
                currentKey = gameState.GetCurrentKey();
            }

            else 
            {
                currentKey = playerKey;    
            }



            //Ha az akció tartalékolás volt
            if (gameState.GetCurrentAction() == SkillEffectAction.Store)
            {

                //A modelben frissítjük a helyzetet
                gameState.GetDataModule().SacrificeWinnerCard(selectedCard, currentKey);

                //UI oldalon is frissítjük a helyzetet, hogy aktuális állapotot tükrözzön
                int winSize = gameState.GetDataModule().GetWinnerAmount(currentKey);
                int lostSize = gameState.GetDataModule().GetLostAmount(currentKey);
                Sprite win = gameState.GetDataModule().GetLastWinnerImage(currentKey);
                Sprite lost = gameState.GetDataModule().GetLastLostImage(currentKey);
                gameState.GetClientModule().ChangePileText(winSize, lostSize, currentKey, win, lost);
                gameState.SetCurrentAction(SkillEffectAction.None);
                gameState.ActionFinished();
            }

            //Ha az akció a csere volt
            else if (gameState.GetCurrentAction() == SkillEffectAction.Switch)
            {
                gameState.GetSkillModule().SwitchCard(gameState.GetActiveCardID(), selectedCard);
                gameState.SetCurrentAction(SkillEffectAction.None);
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
                }

            }

            else if (gameState.GetCurrentAction() == SkillEffectAction.Revive)
            {
                gameState.GetSkillModule().ReviveCard(selectedCard);
                gameState.SetCurrentAction(SkillEffectAction.None);
                gameState.ActionFinished();
            }

            else if(gameState.GetCurrentAction() == SkillEffectAction.TossCard)
            {
                gameState.GetDataModule().TossCardFromHand(currentKey, selectedCard);
                gameState.GetClientModule().TossCard(currentKey, selectedCard);
                gameState.SetCurrentAction(SkillEffectAction.None);
                gameState.ActionFinished();
            }
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
                gameState.GetClientModule().CardChoice(cardList, SkillEffectAction.None, gameState.GetCurrentKey());
            }

            else
            {
                gameState.GetClientModule().CardChoice(cardList, gameState.GetCurrentAction(), gameState.GetCurrentKey());
            }

        }

        public void ReportNameBoxTapping(int playerKey)
        {
            //Ha akkor választottunk játékost, amikor kivégzés akcióról van szó
            if(playerKey != gameState.GetCurrentKey() && gameState.GetCurrentAction() == SkillEffectAction.Execute)
            {

                gameState.SetCurrentAction(SkillEffectAction.TossCard);
                gameState.SetSwitchType(CardListTarget.Hand);

                //Ha ember a választott játékos
                if (gameState.IsThisPlayerHuman(playerKey))
                {
                    gameState.StartCoroutine(gameState.GetClientModule().DisplayCardList(CardListFilter.None, 0, playerKey));
                }

                //Ha bot, akkor dönt ő arról, hogy melyik lapot dobja el magától.
                else
                {
                    gameState.StartCoroutine(gameState.GetAImodule().CardSelectionEffect(CardListFilter.None, 0, playerKey));
                }
            }
        }

        public void ReportActionEnd()
        {
            gameState.ActionFinished();
        }

        public void ReportTurnEnd()
        {
            gameState.TurnFinished();
        }
    }
}
