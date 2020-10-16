using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
    public class Input_Controller
    {
        private GameState_Controller gameState;
        private Client_Controller client;
        private Data_Controller data;

        public Input_Controller(GameState_Controller in_gameState)
        {
            this.gameState = in_gameState;
            this.data = gameState.GetDataModule();
        }

        public void AddClientModule(Client_Controller in_client)
        {
            this.client = in_client;
        }

        //Az input alapján módosítjuk az aktív értéktípust az újra
        public void ReportStatChange(CardStatType newStat)
        {
            //Választott érték beállítása
            gameState.SetActiveStat(newStat);

            //Jelenítsük meg a változást
            client.RefreshStatDisplay();

            //Ha stat választás fázisban vagyunk: Következő fázis jön
            if(gameState.GetGameState() == MainGameStates.SetStat)
            {
                //Kitörés a wait fázisból
                gameState.TurnFinished();
            }

            //Ellenkező esetben skill okozta a változtatást, jelezzük hogy a skillnek vége
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
            if (client.AskSkillStatus(gameState.GetCurrentKey()))
            {
                //Ha nincs stored kártyánk, akkor végeztünk a skill fázissal
                if (gameState.GetStoreCount() == 0)
                {
                    data.GetPlayerWithKey(gameState.GetCurrentKey()).SetStatus(PlayerTurnStatus.Finished);
                }
            }
        }

        //Report: Az aktív játékod lapot idézett
        public void ReportSummon(int indexInHand)
        {
            //Frissítjük a modellt
            data.GetPlayerWithKey(gameState.GetCurrentKey()).PlayCardFromHand(indexInHand);

            //Elvesszük a játékostól a drag jogot
            client.SetDragStatus(gameState.GetCurrentKey(), false);

            //A klienst idézés vége fázisra állítjuk
            client.SummonEnded();

            //Ha az aktív player ember
            if(gameState.IsTheActivePlayerHuman())
            {
                //Megjelenítjük a kör vége gombot
                client.SetEndTurnButton(true);
            }
        }

        //Játékos kilépett
        public void ReportExit()
        {
            data.ResetDecks();
        }

        //Kezeljük a játékos kiválasztott kártyáját
        public void HandleCardSelection(int selectedCard, int currentKey)
        {

            //Ha az akció tartalékolás volt
            if (gameState.GetCurrentAction() == SkillEffectAction.Store)
            {

                //A modelben frissítjük a helyzetet
                data.SacrificeWinnerCard(selectedCard, currentKey);

                //UI oldalon is frissítjük a helyzetet, hogy aktuális állapotot tükrözzön
                int winSize = data.GetWinnerAmount(currentKey);
                int lostSize = data.GetLostAmount(currentKey);
                Sprite win = data.GetLastWinnerImage(currentKey);
                Sprite lost = data.GetLastLostImage(currentKey);
                client.ChangePileText(winSize, lostSize, currentKey, win, lost);
                gameState.SetCurrentAction(SkillEffectAction.None);
                gameState.ActionFinished();
            }

            else 
            {
                gameState.GetSkillModule().HandleSelectionForSkill(selectedCard, currentKey);
            }
        }

        //Játékos kilépett a kártya választó képernyőről
        public void ReportSelectionCancel()
        {
            client.ResetCardSkill(gameState.GetCurrentKey(), gameState.GetActiveCardID());
        }

        public void ReportDisplayRequest(CardListTarget listType, int playerKey)
        {
            List<Card> cardList = new List<Card>();
            string msg;

            switch (listType)
            {
                case CardListTarget.Winners: 
                    cardList = data.GetPlayerWithKey(playerKey).GetWinners();
                    msg = "Győztesek:"; 
                    break;

                case CardListTarget.Losers: 
                    cardList = data.GetPlayerWithKey(playerKey).GetLosers(); 
                    msg = "Vesztesek:";
                    break;
                default:
                    msg = "Hibás kérés"; 
                    break;
            }

            //Lélekrablás esetén saját vesztes lapokat nem választhatunk ki, illetve mások győztes lapjait sem
            if (playerKey == gameState.GetCurrentKey() 
                && gameState.GetCurrentAction() == SkillEffectAction.SkillUse 
                && listType == CardListTarget.Losers)

            {
                msg = "Válaszd ki, hogy melyik lap képességét használod fel!";
                client.CardChoice(cardList, SkillEffectAction.None, gameState.GetCurrentKey(), msg);
            }

            else
            {
                client.CardChoice(cardList, gameState.GetCurrentAction(), playerKey, msg);
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
                    if(data.GetHandCount(playerKey) > 1)
                    {
                        gameState.SetCurrentAction(SkillEffectAction.TossCard);
                        gameState.SetSwitchType(CardListTarget.Hand);

                        //Ha ember a választott játékos
                        if (gameState.IsThisPlayerHuman(playerKey))
                        {
                            gameState.StartCoroutine(client.DisplayCardList(playerKey, CardListFilter.None, 0));
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
                    int ownWinners = data.GetWinnerAmount(ownKey);
                    int opponentWinners = data.GetWinnerAmount(playerKey);
                    Card cardToStrengthen = data.GetCardFromField(ownKey, cardPosition);

                    //Ha olyat választottunk, akinek több győztese van, bónuszt kapunk
                    if(ownWinners < opponentWinners)
                    {
                        int difference = opponentWinners - ownWinners;
                        cardToStrengthen.AddBonus(new StatBonus(difference,difference,difference));
                    }

                    //Ha olyat választott, akinek kevesebb vagy egyenlő a győzteseinek száma
                    else 
                    {
                        //Ha játékos, akkor erről tájékoztatást is kap
                        if(gameState.IsThisPlayerHuman(ownKey))
                        {
                            Client_Controller clientModule = client;
                            gameState.StartCoroutine(clientModule.DisplayNotification("A választott játékos nem rendelkezik több győztessel!\nBónusz nem lett aktiválva!"));                              
                        }
                    }
                    gameState.SetSelectionAction(SkillEffectAction.None);
                    ReportActionEnd();
                }

                //"Van másik!" képesség
                else if(gameState.GetCurrentAction() == SkillEffectAction.SwitchOpponentCard)
                {
                    int ownKey = gameState.GetCurrentKey();
                    //A kiválasztott játékosnak van a paklijában lap
                    if(data.GetDeckAmount(playerKey) > 0)
                    {
                        gameState.SetSwitchType(CardListTarget.Field);

                        //Ha emberek a választó fél
                        if (gameState.IsThisPlayerHuman(ownKey))
                        {
                            gameState.StartCoroutine(client.DisplayCardList(playerKey, CardListFilter.None, 0));
                        }

                        //Ha bot
                        else
                        {
                            gameState.SetSelectionAction(SkillEffectAction.PickCardForSwitch);
                            gameState.StartCoroutine(gameState.GetAImodule().CardSelectionEffect(ownKey, CardListFilter.None, 0, playerKey));
                        }
                    }

                    //Ha nincs elég lapja a választott játékosnak
                    else 
                    {
                        //Ha játékos, akkor erről tájékoztatást kap
                        if(gameState.IsThisPlayerHuman(ownKey))
                        {
                            Client_Controller clientModule = client;
                            gameState.StartCoroutine(clientModule.DisplayNotification("A választott játékos nem rendelkezik elég lappal a pakliban!"));                              
                        }
  
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
