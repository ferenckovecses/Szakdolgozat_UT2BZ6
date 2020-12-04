using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
    public class Input_Controller
    {
        private Module_Controller modules;

        public Input_Controller(Module_Controller in_modules)
        {
            this.modules = in_modules;
        }

        //Az input alapján módosítjuk az aktív értéktípust az újra
        public void ReportStatChange(CardStatType newStat)
        {
            //Választott érték beállítása
            modules.GetGameModule().SetActiveStat(newStat);

            //Jelenítsük meg a változást
            modules.GetClientModule().RefreshStatDisplay();

            //Ha stat választás fázisban vagyunk: Következő fázis jön
            if(modules.GetGameModule().GetGameState() == MainGameStates.SetStat)
            {
                //Kitörés a wait fázisból
                modules.GetGameModule().TurnFinished();
            }

            //Ellenkező esetben skill okozta a változtatást, jelezzük hogy a skillnek vége
            else
            {
                modules.GetGameModule().ActionFinished();    
            }
        }

        public void ReportSkillStatusChange(SkillState state, int cardPosition, bool needToWait = true)
        {
            modules.GetGameModule().StartCoroutine(HandleSkillStatusChange(state, cardPosition, needToWait));
        }

        //Játékos kártyájának skill helyzetében történt válotoztatás kezelése
        public IEnumerator HandleSkillStatusChange(SkillState state, int cardPosition, bool needToWait)
        {

            int key = modules.GetGameModule().GetCurrentKey();

            switch (state)
            {
                case SkillState.Pass:
                    modules.GetDataModule().GetCardFromPlayer(key, cardPosition, CardListTarget.Field).SetSkillState(state);
                    modules.GetGameModule().Pass();
                    break;

                case SkillState.Use: 
                    modules.GetGameModule().SetActiveCardID(cardPosition); 
                    modules.GetDataModule().GetCardFromPlayer(key, cardPosition, CardListTarget.Field).SetSkillState(state);
                    modules.GetClientModule().SetSkillState(key, cardPosition, state);
                    modules.GetGameModule().Use(cardPosition);
                    if(needToWait)
                    {
                        yield return modules.GetGameModule().WaitForEndOfSkill();
                    }
                    break;

                case SkillState.Store:
                    modules.GetDataModule().GetCardFromPlayer(key, cardPosition, CardListTarget.Field).SetSkillState(state);
                    modules.GetGameModule().StartCoroutine(modules.GetGameModule().Store(cardPosition)); 
                    break;

                default:
                    modules.GetDataModule().GetCardFromPlayer(key, cardPosition, CardListTarget.Field).SetSkillState(SkillState.Pass);
                    modules.GetGameModule().Pass(); 
                    break;
            }

            CheckIfFinished();   
        }

        public void CheckIfFinished()
        {
            //Ha az összes pályán lévő kártya skilljéről nyilatkozott a játékos
            if (modules.GetClientModule().AskSkillStatus(modules.GetGameModule().GetCurrentKey()))
            {
                //Ha nincs stored kártyánk, akkor végeztünk a skill fázissal
                if (modules.GetGameModule().GetStoreCount() == 0)
                {
                    modules.GetDataModule().GetPlayerWithKey(modules.GetGameModule().GetCurrentKey()).SetStatus(PlayerTurnStatus.Finished);
                }
            }
        }

        //Report: Az aktív játékod lapot idézett
        public void ReportSummon(int indexInHand)
        {
            //Frissítjük a modellt
            modules.GetDataModule().GetPlayerWithKey(modules.GetGameModule().GetCurrentKey()).PlayCardFromHand(indexInHand);

            //Elvesszük a játékostól a drag jogot
            modules.GetClientModule().SetDragStatus(modules.GetGameModule().GetCurrentKey(), false);

            //A klienst idézés vége fázisra állítjuk
            modules.GetClientModule().SummonEnded();

            //Ha az aktív player ember
            if(modules.GetGameModule().IsTheActivePlayerHuman())
            {
                //Megjelenítjük a kör vége gombot
                modules.GetClientModule().SetEndTurnButton(true);
            }
        }

        //Játékos kilépett
        public void ReportExit()
        {
            modules.GetDataModule().ResetDecks();
        }

        //Kezeljük a játékos kiválasztott kártyáját
        public void HandleCardSelection(int selectedCard, int currentKey)
        {

            //Ha az akció tartalékolás volt
            if (modules.GetGameModule().GetCurrentAction() == SkillEffectAction.Store)
            {
                //A modelben frissítjük a helyzetet
                modules.GetDataModule().SacrificeWinnerCard(selectedCard, currentKey);

                //UI oldalon is frissítjük a helyzetet, hogy aktuális állapotot tükrözzön
                int winSize = modules.GetDataModule().GetWinnerAmount(currentKey);
                int lostSize = modules.GetDataModule().GetLostAmount(currentKey);
                Sprite win = modules.GetDataModule().GetLastWinnerImage(currentKey);
                Sprite lost = modules.GetDataModule().GetLastLostImage(currentKey);
                modules.GetClientModule().ChangePileText(winSize, lostSize, currentKey, win, lost);
                modules.GetGameModule().SetCurrentAction(SkillEffectAction.None);
                modules.GetGameModule().ActionFinished();
            }

            else 
            {
                modules.GetSkillModule().HandleSelectionForSkill(selectedCard, currentKey);
            }
        }

        //Játékos kilépett a kártya választó képernyőről
        public void ReportSelectionCancel()
        {
            modules.GetClientModule().ResetCardSkill(modules.GetGameModule().GetCurrentKey(), modules.GetGameModule().GetActiveCardID());
        }

        public void ReportDisplayRequest(CardListTarget listType, int playerKey)
        {
            List<Card> cardList = new List<Card>();
            string msg;

            switch (listType)
            {
                case CardListTarget.Winners: 
                    cardList = modules.GetDataModule().GetPlayerWithKey(playerKey).GetWinners();
                    msg = "Győztesek:"; 
                    break;

                case CardListTarget.Losers: 
                    cardList = modules.GetDataModule().GetPlayerWithKey(playerKey).GetLosers(); 
                    msg = "Vesztesek:";
                    break;
                default:
                    msg = "Hibás kérés"; 
                    break;
            }

            //Lélekrablás esetén saját vesztes lapokat nem választhatunk ki, illetve mások győztes lapjait sem
            if (playerKey == modules.GetGameModule().GetCurrentKey() 
                && modules.GetGameModule().GetCurrentAction() == SkillEffectAction.SkillUse 
                && listType == CardListTarget.Losers)

            {
                msg = "Válaszd ki, hogy melyik lap képességét használod fel!";
                modules.GetClientModule().CardChoice(cardList, SkillEffectAction.None, modules.GetGameModule().GetCurrentKey(), msg);
            }

            else
            {
                modules.GetClientModule().CardChoice(cardList, modules.GetGameModule().GetCurrentAction(), playerKey, msg);
            }

        }

        public void ReportNameBoxTapping(int playerKey)
        {
            //Nem vált ki akciót magunkra tappelve
            if(playerKey != modules.GetGameModule().GetCurrentKey())
            {
                //Kivégzés képesség
                if(modules.GetGameModule().GetCurrentAction() == SkillEffectAction.Execute)
                {

                    //Csak akkor dob le lapot, ha legalább 1 van még a kezében azon kívül
                    if(modules.GetDataModule().GetHandCount(playerKey) > 1)
                    {
                        modules.GetGameModule().SetCurrentAction(SkillEffectAction.TossCard);
                        modules.GetGameModule().SetSwitchType(CardListTarget.Hand);

                        //Ha ember a választott játékos
                        if (modules.GetGameModule().IsThisPlayerHuman(playerKey))
                        {
                            modules.GetGameModule().StartCoroutine(modules.GetClientModule().DisplayCardList(playerKey, CardListFilter.None, 0));
                        }

                        //Ha bot, akkor dönt ő arról, hogy melyik lapot dobja el magától.
                        else
                        {
                            modules.GetGameModule().StartCoroutine(modules.GetAImodule().CardSelectionEffect(playerKey, CardListFilter.None, 0));
                        }
                    }

                    else
                    {
                        ReportActionEnd();
                        modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
                    }
                }

                //Dühöngés képesség
                else if(modules.GetGameModule().GetCurrentAction() == SkillEffectAction.CheckWinnerAmount)
                {
                    //Szükséges adatok lekérése
                    int cardPosition = modules.GetGameModule().GetActiveCardID();
                    int ownKey = modules.GetGameModule().GetCurrentKey();
                    int ownWinners = modules.GetDataModule().GetWinnerAmount(ownKey);
                    int opponentWinners = modules.GetDataModule().GetWinnerAmount(playerKey);
                    Card cardToStrengthen = modules.GetDataModule().GetCardFromPlayer(ownKey, cardPosition, CardListTarget.Field);

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
                        if(modules.GetGameModule().IsThisPlayerHuman(ownKey))
                        {
                            modules.GetGameModule().StartCoroutine(modules.GetClientModule().DisplayNotification("A választott játékos nem rendelkezik több győztessel!\nBónusz nem lett aktiválva!"));                              
                        }
                    }
                    modules.GetGameModule().SetSelectionAction(SkillEffectAction.None);
                    ReportActionEnd();
                }

                //"Van másik!" képesség
                else if(modules.GetGameModule().GetCurrentAction() == SkillEffectAction.SwitchOpponentCard)
                {
                    int ownKey = modules.GetGameModule().GetCurrentKey();
                    //A kiválasztott játékosnak van a paklijában lap
                    if(modules.GetDataModule().GetDeckAmount(playerKey) > 0)
                    {
                        modules.GetGameModule().SetSwitchType(CardListTarget.Field);

                        //Ha emberek a választó fél
                        if (modules.GetGameModule().IsThisPlayerHuman(ownKey))
                        {
                            modules.GetGameModule().StartCoroutine(modules.GetClientModule().DisplayCardList(playerKey, CardListFilter.None, 0));
                        }

                        //Ha bot
                        else
                        {
                            modules.GetGameModule().SetSelectionAction(SkillEffectAction.PickCardForSwitch);
                            modules.GetGameModule().StartCoroutine(modules.GetAImodule().CardSelectionEffect(ownKey, CardListFilter.None, 0, playerKey));
                        }
                    }

                    //Ha nincs elég lapja a választott játékosnak
                    else 
                    {
                        //Ha játékos, akkor erről tájékoztatást kap
                        if(modules.GetGameModule().IsThisPlayerHuman(ownKey))
                        {
                            modules.GetGameModule().StartCoroutine(modules.GetClientModule().DisplayNotification("A választott játékos nem rendelkezik elég lappal a pakliban!"));                              
                        }
  
                    }
                    
                }
               
            }
        }

        public void HandleChangeOfCardOrder(List<Card> cards, int playerKey)
        {
            //Ha az akció tartalékolás volt
            if (modules.GetGameModule().GetCurrentAction() == SkillEffectAction.Reorganize)
            {
                modules.GetDataModule().ChangeCardOrderInDeck(cards, playerKey, 0);
            }
        }

        public void ReportActionEnd()
        {
            modules.GetGameModule().ActionFinished();
        }

        public void ReportTurnEnd()
        {
            modules.GetGameModule().TurnFinished();
        }
    }
}
