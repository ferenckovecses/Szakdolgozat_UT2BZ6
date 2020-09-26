using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;


namespace ClientControll
{
    [System.Serializable]
    public class Player_UI : MonoBehaviour
    {

        [Header("UI Vezérlő")]
        private Client client = default;

        [Header("Játékos mező referenciák")]
        public GameObject handField;
        public GameObject activeCardField;
        public Image deckImage;
        public Image lostCardImage;
        public Image winnerCardImage;
        public Sprite defaultCardImage;
        public TMP_Text deckSize;
        public TMP_Text winSize;
        public TMP_Text lostSize;


        [Header("Kártyákkal kapcsolatos változók")]
        public GameObject cardPrefab;
        private List<GameObject> cardsInHand;
        public List<GameObject> cardsOnField;

        //Kártya a kézben hatás
        private float turningPerCard = 5f;
        private float cardDistance = 25f;
        private float cardHeightDiff = 5f;

        //Adattárolók
        private int positionID;
        private float rotation;
        private int uniqueID;
        private bool openHandStatus;
        private bool draggableCards;
        private bool skillDecision;
        private int cardCount;
        private int cardsInDeck;
        private int winAmount;
        private int lostAmount;

        //Létrehozáskor hívódik meg
        void Awake()
        {
            this.cardsInHand = new List<GameObject>();
            this.cardsOnField = new List<GameObject>();
            this.cardCount = cardsInHand.Count;
            deckSize = null;
            draggableCards = false;
            skillDecision = false;
            winAmount = 0;
            lostAmount = 0;
        }

        //Kliens referencia csatolása
        public void SetClient(Client clientRef)
        {
            this.client = clientRef;
        }

        //Kártya hozzáadása a kéz mezőhöz
        public void AddCardToHand(GameObject card)
        {
            this.cardsInHand.Add(card);
            this.cardCount = cardsInHand.Count;
            SortHand();
        }

        //Kártya eltávolítása a kéz megadottt pozíciójból
        public void RemoveCardFromHandAtIndex(int index = 0)
        {
            this.cardsInHand.RemoveAt(index);
            this.cardCount = cardsInHand.Count;
            SortHand();
        }

        //Megadott kártya eltávolítása kézből
        public void RemoveSpecificCardFromHand(GameObject card)
        {
            this.cardsInHand.Remove(card);
            this.cardCount = cardsInHand.Count;
            SortHand();
        }

        //Visszaadja az aktivált kártyák játékmezejét
        public GameObject GetActiveField()
        {
            return this.activeCardField;
        }

        //Visszaadja a kézben lévő kártyák játékmezejét
        public GameObject GetHandField()
        {
            return this.handField;
        }

        //Visszaadja a kézben lévő lapok számát
        public int GetHandCount()
        {
            return this.cardCount;
        }

        //Visszaadja a kézben lévő lap objektumok listáját
        public List<GameObject> GetHandList()
        {
            return this.cardsInHand;
        }

        //Visszaad egy megadott indexű kézben lévő lapot
        public GameObject GetACard(int index = 0)
        {
            return this.cardsInHand[index];
        }

        //Tárolja, hogy milyen pozícióban van a játékos mező
        //És frissíti, hogy ez milyen eltolásokkal/forgatásokkal jár
        public void SetPosition(int id)
        {
            this.positionID = id;
            switch (positionID)
            {
                case 0: this.rotation = 0f; break;
                case 1: this.rotation = 180f; break;
                case 2: this.rotation = -90f; break;
                case 3: this.rotation = 90f; break;
                default: this.rotation = 0f; break;

            }
        }


        //Létrehoz egy kártya prefabot a kézbe
        public GameObject CreateCard(Card data, bool visibleForPlayer)
        {
            GameObject temp = Instantiate(cardPrefab, GetHandField().transform.position, Quaternion.identity, GetHandField().transform);
            temp.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

            //Kép kezelése, ha nem a miénk, vagy nincs open head státusz
            if (!visibleForPlayer)
            {
                temp.GetComponent<Image>().sprite = defaultCardImage;
            }

            //Csatoljuk a GameObjecthez a Card adatokat
            temp.GetComponent<CardBehaviour>().SetupCard(data);

            //Beállítjuk a kártya láthatóságát
            temp.GetComponent<CardBehaviour>().SetVisibility(visibleForPlayer);

            return temp;
        }

        //Frissíti a játékos deck számlálóját
        public void UpdateDeckCounter(int newDeckSize)
        {
            this.deckSize.text = newDeckSize.ToString();
        }

        //Elrendezi a kézben lévő kártyákat
        public void SortHand()
        {
            //A középső lap(ok) listabeli indexe
            int middleCardIndex;
            int secondMiddleCardIndex;
            Vector3 changeFromPreviousCard;

            //Páros lapok esetén
            if (this.GetHandCount() % 2 == 0)
            {
                middleCardIndex = (this.GetHandCount() / 2) - 1;
                secondMiddleCardIndex = middleCardIndex + 1;
            }

            //Páratlan lapok esetén
            else
            {
                middleCardIndex = (this.GetHandCount() / 2);
            }

            //A középső lap(ok)tól számított legtávolabbi lap távolsága
            float lastPositionDistance = -cardDistance * middleCardIndex;

            //A középső laphoz képest a magasság változásának mennyisége távolodáskor
            float cardHeight = -cardHeightDiff * middleCardIndex;

            //A kártyákon végigmenve változtatjuk azok tulajdonságait a kívánt effekt elérésének érdekében
            for (var i = 0; i < this.GetHandCount(); i++)
            {
                ///Kártyák pozícionálása játékostér szerint
                this.GetACard(i).transform.position = this.GetHandField().transform.position;

                //Pozíciók módosítása
                if (this.positionID < 2)
                {
                    changeFromPreviousCard = new Vector3(lastPositionDistance, cardHeight.RandomLevelDifference(), 0f);
                }

                else
                {
                    changeFromPreviousCard = new Vector3(cardHeight.RandomLevelDifference(), lastPositionDistance, 0f);
                }

                this.GetACard(i).transform.position = this.GetHandField().transform.position + changeFromPreviousCard;
                lastPositionDistance += cardDistance;

                //Kártya magasságok állítása
                if (i <= middleCardIndex)
                {
                    cardHeight += cardHeightDiff;
                }

                else
                {
                    cardHeight -= cardHeightDiff;
                }

                //Kártyák forgatása a kézben
                float cardTurning;

                if (positionID == 1 || positionID == 2)
                {
                    cardTurning = -turningPerCard * (middleCardIndex - i);
                }
                else
                {
                    cardTurning = turningPerCard * (middleCardIndex - i);
                }

                this.GetACard(i).transform.rotation = Quaternion.Euler(0f, 0f, cardTurning + rotation);

                //Helyrerakjuk a hierarchiában
                this.GetACard(i).transform.SetSiblingIndex(i);

            }
        }

        //Az Open Hand státuszt  változtatja meg
        public void ChangeOpenHand(bool newStatus)
        {
            this.openHandStatus = newStatus;
        }

        //Az Open Hand státuszt adja vissza
        public bool GetOpenHandStatus()
        {
            return this.openHandStatus;
        }

        //Megadott lap objektumot a field mezőre rak
        public void PutCardOnField(GameObject card)
        {
            int handIndex = cardsInHand.IndexOf(card);
            RemoveSpecificCardFromHand(card);
            this.cardsOnField.Add(card);

            //Itt még ad egy jelzést a kliens felé, amit ő továbbít a Game Controllernek
            client.ReportSummon(handIndex, this.positionID);
        }

        //Bot: Megadott kéz indexű lapot megidéz
        public void SummonCard(int handIndex)
        {
            GameObject card = cardsInHand[handIndex];

            //Áthelyezzük a kártyát az aktív mezőre
            card.transform.SetParent(GetActiveField().transform);
            card.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

            //A gameobject listákat frissítjük
            RemoveSpecificCardFromHand(card);
            this.cardsOnField.Add(card);
        }

        //Vak idézés: Nem kézben lévő lapot idézünk meg
        public void BlindSummon(GameObject card)
        {
            //Áthelyezzük a kártyát az aktív mezőre
            card.transform.SetParent(GetActiveField().transform);
            this.cardsOnField.Add(card);
            card.GetComponent<CardBehaviour>().ActivateCard(true);
        }

        //Győztesek közé rakjuk aktív lapjainkat
        public void PutCardIntoWinners()
        {
            if (this.cardsOnField.Any())
            {
                foreach (GameObject card in cardsOnField)
                {
                    winnerCardImage.sprite = card.GetComponent<CardBehaviour>().GetArt();
                    card.GetComponent<CardBehaviour>().TerminateCard();
                    Destroy(card);
                    winAmount += 1;
                    this.winSize.text = winAmount.ToString();
                }

                cardsOnField.Clear();
            }
        }

        //Vesztesek közé rakjuk aktív lapjainkat
        public void PutCardIntoLosers()
        {
            if (this.cardsOnField.Any())
            {
                foreach (GameObject card in cardsOnField)
                {
                    lostCardImage.sprite = card.GetComponent<CardBehaviour>().GetArt();
                    card.GetComponent<CardBehaviour>().TerminateCard();
                    Destroy(card);
                    lostAmount += 1;
                    this.lostSize.text = lostAmount.ToString();
                }

                cardsOnField.Clear();
            }
        }

        //A kézben lévő lapokat felfedi
        public void RevealCardsInHand()
        {
            if (this.openHandStatus)
            {
                //Végigmegy a kártyákon és felfedi azokat
            }
        }

        //Az aktív mezőn lévő lapokat felfedi
        public void RevealCardsOnField()
        {
            foreach (GameObject card in cardsOnField)
            {
                card.GetComponent<CardBehaviour>().SetVisibility(true);
            }
        }

        //Hozzáadjuk a pakli méretét a paklihoz
        public void AddDeckSize(TMP_Text deckSizeIn, int cardsInDeckIn)
        {
            this.deckSize = deckSizeIn;
            this.cardsInDeck = cardsInDeckIn;
            this.deckSize.text = cardsInDeck.ToString();
        }

        //A klienst kéri, hogy jelenítsen meg egy megadott lapot részletes nézetben
        public void DisplayCardDetails(Card data, int activeID, bool skillButtonsRequired)
        {
            this.client.DisplayCardDetaislWindow(data, activeID, skillButtonsRequired, this.positionID);
        }

        //A klienst megkéri, hogy távolítsa el a részletes nézetet
        public void HideCardDetails()
        {
            //Ha nincs fix megjelenítés mód, akkor küldünk eltüntetés kérést
            if (!(this.client.GetFixedDetailsStatus()))
            {
                this.client.HideCardDetailsWindow();
            }
        }

        //Beállítja, hogy van-e lehetőség kártya draggolásra
        public void SetDraggableStatus(bool status)
        {
            this.draggableCards = status;
        }

        //Visszaadja, hogy draggelhetünk-e lapokat
        public bool GetDraggableStatus()
        {
            return this.draggableCards;
        }

        //Elrakjuk a kártyákat a megfelelő helyre a helyezésünk szerint
        public void PutCardsAway(bool isWinner)
        {
            if (isWinner)
            {
                PutCardIntoWinners();
            }

            else
            {
                PutCardIntoLosers();
            }
        }

        //Visszaadja, hogy dönthetünk-e a skillek helyzetéről
        public bool GetSkillStatus()
        {
            return this.skillDecision;
        }

        //Megváltoztatja a skillek döntéséről szóló helyzetet
        public void SetSkillStatus(bool newStatus)
        {
            this.skillDecision = newStatus;
        }

        public bool GetDetailsStatus()
        {
            return this.client.GetCardDetailWindowStatus();
        }

        public void SetDetailsStatus(bool status)
        {
            this.client.SetDetailsStatus(status);
        }

        //Beállítja egy megadott kártya skill státuszát döntés után
        public void SwitchCardSkillStatus(SkillState state, int cardPosition)
        {
            cardsOnField[cardPosition].GetComponent<CardBehaviour>().SetSkillState(state);
        }

        //Visszaadja, hogy van-e még eldöntetlen skill helyzetű kártya a mezőn
        public bool GetCardSkillStatusFromField()
        {
            foreach (GameObject card in cardsOnField)
            {
                //Ha találunk a lerakott kártyák között olyat, akinek a képessége még nem eldöntött
                if (card.GetComponent<CardBehaviour>().GetSkillState() == SkillState.NotDecided)
                {
                    return false;
                }
            }
            return true;
        }

        public SkillState GetSpecificCardSkillStatus(int cardID)
        {
            return cardsOnField[cardID].GetComponent<CardBehaviour>().GetSkillState();
        }

        public void SetSpecificCardSkillStatus(int cardID, SkillState newState)
        {
            cardsOnField[cardID].GetComponent<CardBehaviour>().SetSkillState(newState);
        }

        public void ResetCardSkill(int cardPosition)
        {
            cardsOnField[cardPosition].GetComponent<CardBehaviour>().ResetSkill();
        }

        public void NewSkillCycle()
        {
            foreach (GameObject card in cardsOnField)
            {
                card.GetComponent<CardBehaviour>().NewSkillCycle();
            }
        }

        public void RefreshPileSize()
        {
            this.winSize.text = winAmount.ToString();
            this.lostSize.text = lostAmount.ToString();
        }

        public void ChangePileText(int newWinSize, int newLostSize, Sprite win, Sprite lost)
        {
            this.winAmount = newWinSize;
            this.lostAmount = newLostSize;

            lostCardImage.sprite = lost;
            winnerCardImage.sprite = win;
            RefreshPileSize();
        }

        public void SwitchHandFromField(Card fieldData, int fieldId, Card handData, int handId, bool visibility)
        {
            cardsOnField[fieldId].GetComponent<CardBehaviour>().SetupCard(handData);
            cardsOnField[fieldId].GetComponent<CardBehaviour>().SetVisibility(true);
            cardsOnField[fieldId].GetComponent<CardBehaviour>().SetSkillState(SkillState.Pass);


            cardsInHand[handId].GetComponent<CardBehaviour>().SetupCard(fieldData);
            cardsInHand[handId].GetComponent<CardBehaviour>().SetVisibility(visibility);
        }

        public void DisplayCards(int type)
        {
            //Csak egyszerre egyszer lehessen megjeleníteni
            if (!client.GetCardListStatus())
            {
                CardListTarget listType = CardListTarget.None;

                switch (type)
                {
                    case 0: listType = CardListTarget.Losers; break;
                    case 1: listType = CardListTarget.Winners; break;
                    default: break;
                }

                //Csak akkor jelenítsük meg, ha van mit megjeleníteni.
                if ((listType == CardListTarget.Winners && winAmount > 0) || (listType == CardListTarget.Losers && lostAmount > 0))
                {
                    client.ReportDisplayRequest(listType, positionID);
                }
            }

        }

    }
}