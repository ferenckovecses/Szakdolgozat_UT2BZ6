using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public void SetDragStatus(int playerKey, bool status)
        {
            client.SetDragStatus(playerKey, status);
        }

        //Beállítjuk, hogy az adott játékos dönthet-e a képességeiről most, vagy sem
        public void SetSkillStatus(int playerKey, bool status)
        {
            client.SetSkillStatus(playerKey, status);
        }
        //Kártya idézés: A megfelelő játékospanel felé továbbítjuk a megfelelő kártya kézbeli azonosítóját
        public void SummonCard(int playerKey, int handIndex)
        {
            client.SummonCard(playerKey, handIndex);
        }

        //Felfedi a lerakott kártyát
        public void RevealCards(int key)
        {
            client.RevealCards(key);
        }

        //Elrakatja a UI-on megjelenített kártyákat a megfelelő helyre
        public IEnumerator PutCardsAway(int key, bool isWinner)
        {
            yield return new WaitForSeconds(1f);
            client.PutCardsAway(key, isWinner);
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

    }
}
