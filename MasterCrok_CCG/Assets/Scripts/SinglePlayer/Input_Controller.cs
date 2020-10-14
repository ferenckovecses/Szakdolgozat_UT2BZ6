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

        public void ReportSkillStatusChange(SkillState state, int cardPosition, bool needToWait = true)
        {
            gameState.StartCoroutine(HandleSkillStatusChange(state, cardPosition, needToWait));
        }

        //Játékos kártyájának skill helyzetében történt válotoztatás kezelése
        public IEnumerator HandleSkillStatusChange(SkillState state, int cardPosition, bool needToWait)
        {
            switch (state)
            {
                case SkillState.Pass: 
                    gameState.Pass(); 
                    break;

                case SkillState.Use: 
                    gameState.SetActiveCardID(cardPosition); 
                    gameState.Use(cardPosition);
                    if(needToWait)
                    {
                        yield return gameState.WaitForEndOfSkill();
                    }
                    break;

                case SkillState.Store: 
                    gameState.StartCoroutine(gameState.Store(cardPosition)); 
                    break;

                default: 
                    gameState.Pass(); 
                    break;
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
        public void HandleCardSelection(int selectedCard, int currentKey)
        {
            Data_Controller data = gameState.GetDataModule();

            //Ha az akció tartalékolás volt
            if (gameState.GetCurrentAction() == SkillEffectAction.Store)
            {

                //A modelben frissítjük a helyzetet
                gameState.GetDataModule().SacrificeWinnerCard(selectedCard, currentKey);

                //UI oldalon is frissítjük a helyzetet, hogy aktuális állapotot tükrözzön
                int winSize = data.GetWinnerAmount(currentKey);
                int lostSize = data.GetLostAmount(currentKey);
                Sprite win = data.GetLastWinnerImage(currentKey);
                Sprite lost = data.GetLastLostImage(currentKey);
                gameState.GetClientModule().ChangePileText(winSize, lostSize, currentKey, win, lost);
                gameState.SetCurrentAction(SkillEffectAction.None);
                gameState.ActionFinished();
            }

            //Ha az akció a csere volt
            else if (gameState.GetCurrentAction() == SkillEffectAction.Switch)
            {
                gameState.GetSkillModule().SwitchCard(currentKey, gameState.GetActiveCardID(), selectedCard);
                gameState.SetCurrentAction(SkillEffectAction.None);
                gameState.ActionFinished();
            }

            //Ha az akció skill lopás volt
            else if (gameState.GetCurrentAction() == SkillEffectAction.SkillUse)
            {
                Card otherCard = data.GetCardFromLosers(currentKey, selectedCard);

                Debug.Log("Választott játékos kulcsa: " + currentKey.ToString());
                Debug.Log("Választott kártya pozíciója: " + selectedCard.ToString());

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

                    gameState.GetSkillModule().UseSkill(ownCard.GetCardID());
                    Debug.Log("Használt Skill ID: " + ownCard.GetCardID().ToString());
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
                Debug.Log("Card needs to be tossed from: " + currentKey.ToString());
                data.TossCardFromHand(currentKey, selectedCard);
                gameState.GetClientModule().TossCard(currentKey, selectedCard);
                gameState.SetCurrentAction(SkillEffectAction.None);
                gameState.ActionFinished();
            }

            else if(gameState.GetCurrentAction() == SkillEffectAction.SwitchOpponentCard)
            {
                gameState.SetSwitchType(CardListTarget.Deck);
                Debug.Log("Card will be switched for: " + currentKey.ToString());
                int deckCardID = (data.GetDeckAmount(currentKey)) - 1 ;
                gameState.GetSkillModule().SwitchCard(currentKey, selectedCard, deckCardID);
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
                gameState.GetClientModule().CardChoice(cardList, gameState.GetCurrentAction(), playerKey);
            }

        }

        public void ReportNameBoxTapping(int playerKey)
        {
            //Nem vált ki akciót magunkra tappelve
            if(playerKey != gameState.GetCurrentKey())
            {
                //Kivégzés képesség
                if(gameState.GetCurrentAction() == SkillEffectAction.Execute)
                {

                    //Csak akkor dob le lapot, ha legalább 1 van még a kezében azon kívül
                    if(gameState.GetDataModule().GetHandCount(playerKey) > 1)
                    {
                        gameState.SetCurrentAction(SkillEffectAction.TossCard);
                        gameState.SetSwitchType(CardListTarget.Hand);

                        //Ha ember a választott játékos
                        if (gameState.IsThisPlayerHuman(playerKey))
                        {
                            gameState.StartCoroutine(gameState.GetClientModule().DisplayCardList(playerKey, CardListFilter.None, 0));
                        }

                        //Ha bot, akkor dönt ő arról, hogy melyik lapot dobja el magától.
                        else
                        {
                            gameState.StartCoroutine(gameState.GetAImodule().CardSelectionEffect(playerKey, CardListFilter.None, 0));
                        }
                    }
                }

                //Dühöngés képesség
                else if(gameState.GetCurrentAction() == SkillEffectAction.CheckWinnerAmount)
                {
                    //Szükséges adatok lekérése
                    int cardPosition = gameState.GetActiveCardID();
                    int ownKey = gameState.GetCurrentKey();
                    Data_Controller data = gameState.GetDataModule();
                    int ownWinners = data.GetWinnerAmount(ownKey);
                    int opponentWinners = data.GetWinnerAmount(playerKey);
                    Card cardToStrengthen = data.GetCardFromField(ownKey, cardPosition);

                    //Ha olyat választottunk, akinek több győztese van, bónuszt kapunk
                    if(ownWinners < opponentWinners)
                    {
                        int difference = opponentWinners - ownWinners;
                        cardToStrengthen.AddBonus(new StatBonus(difference,difference,difference));
                    }
                    gameState.SetSelectionAction(SkillEffectAction.None);
                    ReportActionEnd();
                }

                //"Van másik!" képesség
                else if(gameState.GetCurrentAction() == SkillEffectAction.SwitchOpponentCard)
                {
                    Debug.Log("Van másik! ág");
                    gameState.SetSwitchType(CardListTarget.Field);

                    //Ha emberek a választó fél
                    if (gameState.IsThisPlayerHuman(gameState.GetCurrentKey()))
                    {
                        gameState.StartCoroutine(gameState.GetClientModule().DisplayCardList(playerKey, CardListFilter.None, 0));
                    }

                    //Ha bot
                    else
                    {
                        Debug.Log("Bot path");
                        int ownKey = gameState.GetCurrentKey();
                        gameState.SetSelectionAction(SkillEffectAction.PickCardForSwitch);
                        gameState.StartCoroutine(gameState.GetAImodule().CardSelectionEffect(ownKey, CardListFilter.None, 0, playerKey));
                    }
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
