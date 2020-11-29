using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameControll
{
    public class AI_Controller
    {
        private Module_Controller modules;

        //AI Context
        private List<Card> cardsOnField;
        private List<PlayerCardPairs> opponentCards;
        private List<Card> cardsInHand;
        private int winAmount;
        private int currentKey;
        private int cardCount;
        private CardStatType currentStat;


        public AI_Controller(Module_Controller in_modules)
        {
            this.modules = in_modules;
            List<Card> cardsOnField = new List<Card>();
            List<List<Card>> opponentCards = new List<List<Card>>();
            List<Card> cardsInHand = new List<Card>();
        }

        public void SummonCard(int playerKey)
        {
            GetCurrentContext(playerKey);

            //AI agy segítségét hívjuk a döntésben
            int cardIndex = Bot_Behaviour.ChooseRightCard(this.cardsInHand, this.currentStat);

            //Lerakatjuk a kártyát a UI-ban
            modules.GetGameModule().StartCoroutine(modules.GetClientModule().SummonCard(currentKey, cardIndex));

        }

        //Dönt a képességeiről
        public IEnumerator DecideSkill(int key)
        {
            GetCurrentContext(key);

            for(var id = 0; id < cardCount; id++)
            {
                if(modules.GetClientModule().AskCardSkillStatus(currentKey, id) == SkillState.NotDecided)
                {
                    //GetSkillChoiceFromAIBrain
                    modules.GetInputModule().ReportSkillStatusChange(SkillState.Use, id, false);
                    yield return modules.GetGameModule().WaitForEndOfSkill();
                }
            }
            yield return new WaitForSeconds(0.01f);

            //Az AI végzett a körével a döntések után
            modules.GetDataModule().GetPlayerWithKey(currentKey).SetStatus(PlayerTurnStatus.Finished);

            modules.GetGameModule().TurnFinished();
        }

        //Dönt az új statról
        public void DecideNewStat(int key)
        {
            GetCurrentContext(key);

            //AI agy segítségét hívjuk a döntésben
            CardStatType newStat = Bot_Behaviour.ChangeFightType(cardsOnField,
                opponentCards, currentStat);

            modules.GetGameModule().SetActiveStat(newStat);

            //Jelenítsük meg a változást
            modules.GetClientModule().RefreshStatDisplay();
        }

        //Kártyacsere vagy választás
        public IEnumerator CardSelectionEffect(int key, CardListFilter filter, int limit = 0, int otherPlayer = -1)
        {
            GetCurrentContext(key);

            //Ha cseréről van szó
            if (modules.GetGameModule().GetCurrentAction() == SkillEffectAction.Switch)
            {
                int handID = -1;

                //Ha kézből cserélünk
                if (modules.GetGameModule().GetCurrentListType() == CardListTarget.Hand)
                {
                    handID = Bot_Behaviour.HandSwitch(
                        modules.GetDataModule().GetCardsFromPlayer(currentKey, CardListTarget.Hand),
                        modules.GetDataModule().GetCardsFromPlayer(currentKey, CardListTarget.Field),
                        modules.GetDataModule().GetOpponentsCard(currentKey), 
                        currentStat);

                    modules.GetSkillModule().SwitchCard(currentKey, modules.GetGameModule().GetActiveCardID(), handID);
                }
            }

            //Ha skill lopásról van szó
            else if (modules.GetGameModule().GetCurrentAction() == SkillEffectAction.SkillUse)
            {
                //Adatgyűjtés
                Card ownCard = modules.GetDataModule().GetCardFromPlayer(currentKey, modules.GetGameModule().GetActiveCardID(), CardListTarget.Field);
                List<PlayerCardPairs> cards = modules.GetDataModule().GetOtherLosers(currentKey);
                int index = Bot_Behaviour.WhichSkillToUse(cards);

                modules.GetInputModule().HandleCardSelection(cards[index].cardPosition, cards[index].playerKey);
                yield break;
            }

            //Ha felélesztésről van szó
            else if (modules.GetGameModule().GetCurrentAction() == SkillEffectAction.Revive)
            {
                List<Card> cards = modules.GetDataModule().GetCardsFromPlayer(currentKey, CardListTarget.Losers);
                int cardID = Bot_Behaviour.WhomToRevive(cards);
                modules.GetInputModule().HandleCardSelection(cardID, currentKey);
            }

            else if(modules.GetGameModule().GetCurrentAction() == SkillEffectAction.Execute)
             {
                List<int> temp = modules.GetDataModule().GetOtherKeyList(currentKey);
                int choosenKey = Bot_Behaviour.WhichPlayerToExecute(temp);

                modules.GetInputModule().ReportNameBoxTapping(choosenKey);
                yield return modules.GetGameModule().WaitForEndOfAction();
             }

             else if(modules.GetGameModule().GetCurrentAction() == SkillEffectAction.SwitchOpponentCard)
             {
                List<int> temp = modules.GetDataModule().GetOtherKeyList(currentKey);
                int choosenKey = Bot_Behaviour.WhichPlayerToExecute(temp);

                modules.GetInputModule().ReportNameBoxTapping(choosenKey);
                yield return modules.GetGameModule().WaitForEndOfAction();
             }

             else if(modules.GetGameModule().GetCurrentAction() == SkillEffectAction.PickCardForSwitch)
             {
                List<Card> cardsOnField = modules.GetDataModule().GetCardsFromPlayer(otherPlayer, CardListTarget.Field);
                int choosenCardID = Bot_Behaviour.WhichCardToSwitch(cardsOnField);
                int deckCount = (modules.GetDataModule().GetDeckAmount(otherPlayer)) - 1;
                modules.GetGameModule().SetSwitchType(CardListTarget.Deck);
                modules.GetSkillModule().SwitchCard(otherPlayer, choosenCardID, deckCount);
             }

             else if(modules.GetGameModule().GetCurrentAction() == SkillEffectAction.CheckWinnerAmount)
             {
                List<int> keyList = modules.GetDataModule().GetOtherKeyList(currentKey);
                List<int> winAmounts = modules.GetDataModule().GetOthersWinAmount(currentKey);

                int playerID = Bot_Behaviour.WhichPlayerToChoose(winAmounts);

                modules.GetInputModule().ReportNameBoxTapping(keyList[playerID]);
             }

            //Ha kézből eldobásról van szó
            else if (modules.GetGameModule().GetCurrentAction() == SkillEffectAction.TossCard)
            {
                //Csak akkor dobunk el lapot, ha van a kezünkben
                if(cardsInHand.Count > 1)
                {
                    int cardID = Bot_Behaviour.WhomToToss(cardsInHand);
                    modules.GetInputModule().HandleCardSelection(cardID, currentKey);
                }

                else 
                {
                    Debug.Log("Nincs mit eldobni");
                    modules.GetGameModule().ActionFinished();
                }
            }

            else if(modules.GetGameModule().GetCurrentAction() == SkillEffectAction.SacrificeFromHand)
            {
                int cardID = Bot_Behaviour.WhomToToss(cardsInHand);
                modules.GetInputModule().HandleCardSelection(cardID, currentKey);
            }

            else if(modules.GetGameModule().GetCurrentAction() == SkillEffectAction.SacrificeDoppelganger)
            {
                List<Card> cardList = modules.GetDataModule().GetCardsFromPlayer(currentKey, CardListTarget.Hand, filter);
                if(cardList.Count > 0)
                {
                    int cardID = Bot_Behaviour.WhomToToss(cardList);
                    modules.GetInputModule().HandleCardSelection(cardID, currentKey);
                }

                else 
                {
                    Debug.Log("Nincs megfelelő lap a kézben");
                    modules.GetGameModule().ActionFinished();
                }
            }

            else if(modules.GetGameModule().GetCurrentAction() == SkillEffectAction.Reorganize)
            {
                List<Card> cardsInDeck = modules.GetDataModule().GetCardsFromPlayer(currentKey, CardListTarget.Deck, CardListFilter.None, limit);
                cardsInDeck = Bot_Behaviour.OrganiseCardsInDeck(cardsInDeck, modules.GetDataModule().GetRNG());
                modules.GetInputModule().HandleChangeOfCardOrder(cardsInDeck, currentKey);
            }

            modules.GetGameModule().StartCoroutine(modules.GetGameModule().SkillFinished());
        }

        //Lekéri az aktuális számára is elérhető adatokat
        private void GetCurrentContext(int key)
        {
            this.currentKey = key;
            this.cardsOnField = modules.GetDataModule().GetCardsFromPlayer(currentKey, CardListTarget.Field);
            this.cardCount = cardsOnField.Count;
            this.opponentCards = modules.GetDataModule().GetOpponentsCard(currentKey);
            this.cardsInHand = modules.GetDataModule().GetCardsFromPlayer(currentKey, CardListTarget.Hand);
            this.winAmount = modules.GetDataModule().GetWinnerAmount(currentKey);
            this.currentStat = modules.GetGameModule().GetActiveStat();
        }
    }
}
