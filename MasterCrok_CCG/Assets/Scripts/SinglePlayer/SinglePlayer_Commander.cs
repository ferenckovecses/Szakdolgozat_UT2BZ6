using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.SinglePlayer
{
    class SinglePlayer_Commander
    {
        SinglePlayer_Controller controller;
        SinglePlayer_Client client;
        SinglePlayer_Report report;

        public SinglePlayer_Commander(SinglePlayer_Controller controller_m, SinglePlayer_Client client_m, SinglePlayer_Report report_m)
        {
            this.controller = controller_m;
            this.client = client_m;
            this.report = report_m;
        }

        //Legenerálja a UI felületet
        public void GenerateUI(int numberOfPlayers, List<int> playerKeys, int playerDeckSize)
        {
            client.SetReportAgent(report);
            client.CreatePlayerFields(numberOfPlayers, playerKeys, playerDeckSize);
        }
        //Beállíthatjuk, hogy az adott játékos rakhat-e kártyát most, vagy sem
        public void SetDragStatus(int playerKey, bool newStatus)
        {
            client.GetFieldFromKey(playerKey).SetDraggableStatus(newStatus);
        }

        //Beállítjuk, hogy a megadott játékos rendelkezhet-e képességeiről
        public void SetSkillStatus(int playerKey, bool newStatus)
        {
            client.GetFieldFromKey(playerKey).SetSkillStatus(newStatus);
        }

        //Botok UI-jának küld egy parancsot egy megadott indexű kártya lerakására/aktiválására
        public void SummonCard(int playerKey, int cardHandIndex)
        {
            client.GetFieldFromKey(playerKey).SummonCard(cardHandIndex);
        }

        //Frissíti a játékos paklijának méretét a modell szerinti aktuális értékre
        public void RefreshDeckSize(int playerKey, int newDeckSize)
        {
            client.GetFieldFromKey(playerKey).UpdateDeckCounter(newDeckSize);
        }

        //Felfedi a lerakott kártyát
        public void RevealCards(int playerKey)
        {
            client.GetFieldFromKey(playerKey).RevealCardsOnField();
        }

        //Elrakatja a UI-on megjelenített kártyákat a megfelelő helyre
        public IEnumerator PutCardsAway(int playerKey, bool isWinner)
        {
            yield return new WaitForSeconds(1f);
            client.GetFieldFromKey(playerKey).PutCardsAway(isWinner);
        }

        //Frissítést küld a játékpanelen az aktuális harctípus szövegére
        public void RefreshStatDisplay()
        {
            string statText;

            switch (controller.GetActiveStat())
            {
                case ActiveStat.Power: statText = "Erő"; break;
                case ActiveStat.Intelligence: statText = "Intelligencia"; break;
                case ActiveStat.Reflex: statText = "Reflex"; break;
                default: statText = "Harctípus"; break;
            }

            client.ChangeStatText(statText);
        }

        //Megjeleníti a játékos számára a harctípus választó képernyőt
        public IEnumerator DisplayStatBox()
        {
            while (controller.IsMessageOnScreen())
            {
                yield return null;
            }

            client.DisplayStatBox();
        }

        //Értesítő üzenetet jelenít meg a UI felületen
        public IEnumerator DisplayNotification(string msg)
        {

            //Várakozunk a megjelenítéssel, amíg az előző üzenet eltűnik
            while (controller.IsMessageOnScreen())
            {
                yield return null;
            }

            //Megjelenítjük az üzenetet
            client.DisplayMessage(msg);
            controller.SetMessageStatus(true);

            //Adunk időt a játékosoknak, hogy elolvassák
            yield return new WaitForSeconds(1.2f);

            //Eltüntetjük az üzenetet
            client.HideMessage();
            controller.SetMessageStatus(false);
        }

        public void DisplayWinnerCards(List<Card> cardData)
        {
            client.DisplayListOfCards(cardData);
        }

        //Frissíti a győztes és vesztes halmok képét és darabszámát
        public void ChangePileText(int winSize, int lostSize, int playerKey, Sprite win, Sprite lost)
        {
            client.GetFieldFromKey(playerKey).ChangePileText(winSize, lostSize, win, lost);
        }

        //Új Skill ciklus: A tartalékolt képességekről újra lehet dönteni
        public void NewSkillCycle(int playerKey)
        {
            client.GetFieldFromKey(playerKey).NewSkillCycle();
        }

        //A megadott játékosnál lévő és megadott pozícióban található kártya képességét reseteli
        public void ResetCardSkill(int playerKey, int cardPosition)
        {
            client.GetFieldFromKey(playerKey).ResetCardSkill(cardPosition);
        }

    }
}
