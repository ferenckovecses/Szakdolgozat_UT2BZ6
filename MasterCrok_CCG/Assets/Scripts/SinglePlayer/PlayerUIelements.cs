using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

/*
Feladata: A játék UI vezérlője kezeli, minden PlayerUIelements egy-egy játékosnak az aktuális játékbeli állását reprezentálja
és ezt jeleníti meg a képernyőn a játékos számára. 

Adatfolyam:
[PlayerUIelements] <- UI vezérlő <-> Játékvezérlő <-> Szerver 
*/

[System.Serializable]
public class PlayerUIelements : MonoBehaviour
{

    [Header("UI Vezérlő")]
    BattleUI_Controller controller;

    [Header("Játékos mező referenciák")]
    public GameObject handField;
    public GameObject activeCardField;
    public Image deckImage;
    public Image lostCardImage;
    public Image winnerCardImage;
    public Sprite defaultCardImage;
    public TMP_Text deckSize;


    [Header("Kártyákkal kapcsolatos változók")]
    public GameObject cardPrefab;
    public List<GameObject> cardsInHand;
    public List<GameObject> cardsInField;

    //Kártya a kézben hatás
    private float turningPerCard = 5f;
    private float cardDistance = 25f;
    private float cardHeightDiff = 5f;

    //Adattárolók
    private int positionID;
    private float rotation;
    private int uniqueID;
    private bool openHand;
    private bool draggableCards;
    private int cardCount;
    private int cardsInDeck;

    void Awake()
    {
        controller = GameObject.Find("UI_Controller").GetComponent<BattleUI_Controller>();
        this.cardsInHand = new List<GameObject>();
        this.cardsInField = new List<GameObject>();
        this.cardCount = cardsInHand.Count;
        deckSize = null;
        draggableCards = false;
    }

    public void AddCardToHand(GameObject card)
    {
        this.cardsInHand.Add(card);
        this.cardCount = cardsInHand.Count;
        SortHand();
    }

    public void RemoveCardAtIndex(int index = 0)
    {
        this.cardsInHand.RemoveAt(index);
        this.cardCount = cardsInHand.Count;
        SortHand();
    }

    public void RemoveSpecificCard(GameObject card)
    {
        this.cardsInHand.Remove(card);
        this.cardCount = cardsInHand.Count;
        SortHand();
    }

    public GameObject GetActiveField()
    {
        return this.activeCardField;
    }

    public GameObject GetHandField()
    {
        return this.handField;
    }

    public int GetCardCount()
    {
        return this.cardCount;
    }

    public List<GameObject> GetcardsInHand()
    {
        return this.cardsInHand;
    }

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
    public GameObject CreateCard(Card data, bool visible)
    {
        GameObject temp = Instantiate(cardPrefab, GetHandField().transform.position, Quaternion.identity, GetHandField().transform);
        temp.transform.rotation = Quaternion.Euler(0f,0f,rotation);

        //Kép kezelése, ha nem a miénk, vagy nincs open head státusz
        if(!visible)
        {
            temp.GetComponent<Image>().sprite = defaultCardImage;
        }

        //Csatoljuk a GameObjecthez a Card adatokat
        temp.GetComponent<CardBehaviour>().SetupCard(data);    

        //Beállítjuk a kártya láthatóságát
        temp.GetComponent<CardBehaviour>().SetVisibility(visible);

        return temp;
    }

    public void UpdateDeckCounter(int newDeckSize)
    {   
        this.deckSize.text = newDeckSize.ToString();
    }

    //Eltávolít egy kártya prefabot a kézből
    public void DiscardCard()
    {
            //Ha van eltávolítandó kártya
            if(GetcardsInHand().Any())
            {
                //Akkor távolítsa el
                Destroy(GetACard());
                RemoveCardAtIndex();
            }
    }

    //Elrendezi a kézben lévő kártyákat
    public void SortHand()
    {
        //A középső lap(ok) listabeli indexe
        int middleCardIndex;
        int secondMiddleCardIndex;
        Vector3 changeFromPreviousCard;

        //Páros lapok esetén
        if(this.GetCardCount() % 2 == 0)
        {
            middleCardIndex = (this.GetCardCount() / 2) -1;
            secondMiddleCardIndex = middleCardIndex + 1;
        }

        //Páratlan lapok esetén
        else 
        {
            middleCardIndex = (this.GetCardCount() / 2);
        }

        //A középső lap(ok)tól számított legtávolabbi lap távolsága
        float lastPositionDistance = -cardDistance * middleCardIndex;

        //A középső laphoz képest a magasság változásának mennyisége távolodáskor
        float cardHeight = -cardHeightDiff * middleCardIndex;

        //A kártyákon végigmenve változtatjuk azok tulajdonságait a kívánt effekt elérésének érdekében
        for(var i = 0; i < this.GetCardCount(); i++)
        {
            ///Kártyák pozícionálása játékostér szerint
            this.GetACard(i).transform.position = this.GetHandField().transform.position;

            //Pozíciók módosítása
            if(this.positionID < 2)
            {
                changeFromPreviousCard = new Vector3(lastPositionDistance,cardHeight.RandomLevelDifference(),0f);
            }

            else 
            {
                changeFromPreviousCard = new Vector3(cardHeight.RandomLevelDifference(),lastPositionDistance,0f);
            }

            this.GetACard(i).transform.position = this.GetHandField().transform.position + changeFromPreviousCard;
            lastPositionDistance += cardDistance;

            //Kártya magasságok állítása
            if(i <= middleCardIndex)
            {
                cardHeight += cardHeightDiff;
            }

            else 
            {
                cardHeight -= cardHeightDiff;   
            }

            //Kártyák forgatása a kézben
            float cardTurning;

            if(positionID == 1 || positionID == 2)
            {
                cardTurning = -turningPerCard * (middleCardIndex-i);
            }
            else 
            {
                cardTurning = turningPerCard * (middleCardIndex-i);
            }

            this.GetACard(i).transform.rotation = Quaternion.Euler(0f,0f,cardTurning + rotation);

            //Helyrerakjuk a hierarchiában
            this.GetACard(i).transform.SetSiblingIndex(i);

        }
    }

    public void ChangeOpenHand(bool newValue)
    {
        this.openHand = newValue;
    }

    public bool GetOpenHandStatus()
    {
        return this.openHand;
    }

    public void PutCardOnField(GameObject card)
    {
        int handIndex = cardsInHand.IndexOf(card);
        RemoveSpecificCard(card);
        this.cardsInField.Add(card);

        //Itt még ad egy jelzést a UI controller felé, amit ő továbbít a Game Controllernek
        controller.ReportSummon(handIndex, this.positionID);
    }

    public void SummonCard(int index)
    {
        GameObject card = cardsInHand[index];

        //Áthelyezzük a kártyát az aktív mezőre
        card.transform.SetParent(GetActiveField().transform);
        card.transform.rotation = Quaternion.Euler(0f,0f, rotation);
        
        //A gameobject listákat frissítjük
        RemoveSpecificCard(card);
        this.cardsInField.Add(card);
    }

    public void BlindSummon(GameObject card)
    {
       //Áthelyezzük a kártyát az aktív mezőre
        card.transform.SetParent(GetActiveField().transform);
        this.cardsInField.Add(card);
    }

    public void PutCardIntoWinners()
    {
        if(this.cardsInField.Any())
        {
            foreach (GameObject card in cardsInField) 
            {
                winnerCardImage.sprite = card.GetComponent<CardBehaviour>().GetArt();
                card.GetComponent<CardBehaviour>().TerminateCard();
                Destroy(card);
            }

            cardsInField.Clear();
        }
    }

    public void PutCardIntoLosers()
    {
        if(this.cardsInField.Any())
        {
            foreach (GameObject card in cardsInField) 
            {
                lostCardImage.sprite = card.GetComponent<CardBehaviour>().GetArt();
                card.GetComponent<CardBehaviour>().TerminateCard();
                Destroy(card);
            }

            cardsInField.Clear();
        }
    }

    public void RevealCardsInHand()
    {
        if(this.openHand)
        {
            //Végigmegy a kártyákon és felfedi azokat
        }
    }

    public void RevealCards()
    {
        foreach (GameObject card in cardsInField) 
        {
            card.GetComponent<CardBehaviour>().SetVisibility(true);
        }
    }

    public void AddDeckSize(TMP_Text deckSizeIn, int cardsInDeckIn)
    {
        this.deckSize = deckSizeIn;
        this.cardsInDeck = cardsInDeckIn;
        this.deckSize.text = cardsInDeck.ToString();
    }

    public void DisplayDetails(Card data)
    {
        this.controller.DisplayCardDetail(data);
    }

    public void HideDetails()
    {
        this.controller.HideCardDetail();
    }

    public void SetUniqueID(int newID)
    {
        this.uniqueID = newID;
    }

    public void SetDraggableStatus(bool status)
    {
        this.draggableCards = status;
    }

    public bool GetDraggableStatus()
    {
        return this.draggableCards;
    }

    public void PutCardsAway(bool isWinner)
    {
        if(isWinner)
        {
            PutCardIntoWinners();
        }

        else 
        {
            PutCardIntoLosers();
        }
    }

}