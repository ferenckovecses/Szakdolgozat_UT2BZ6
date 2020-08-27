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
    public bool openHand;
    int cardCount;
    int cardsInDeck;

    [Header("Kártya a kézben hatás")]
    float turningPerCard = 5f;
    float cardDistance = 25f;
    float cardHeightDiff = 5f;

    [Header("UI Pozícionálás")]
    int positionID;
    float rotation;

    void Awake()
    {
        controller = GameObject.Find("UI_Controller").GetComponent<BattleUI_Controller>();
        this.cardsInHand = new List<GameObject>();
        this.cardCount = cardsInHand.Count;
        deckSize = null;
    }

    public void AddCardToHand(GameObject card)
    {
        cardsInHand.Add(card);
        cardCount = cardsInHand.Count;
        SortHand();
    }

    public void RemoveCardAtIndex(int index = 0)
    {
        cardsInHand.RemoveAt(index);
        cardCount = cardsInHand.Count;
        SortHand();
    }

    public void RemoveSpecificCard(GameObject card)
    {
        cardsInHand.Remove(card);
        cardCount = cardsInHand.Count;
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
    public void CreateCard(Card data, bool visible)
    {
        GameObject temp = Instantiate(cardPrefab, GetHandField().transform.position, Quaternion.identity, GetHandField().transform);
        temp.transform.rotation = Quaternion.Euler(0f,0f,rotation);

        //Kép kezelése, ha nem a miénk, vagy nincs open head státusz
        if(!visible)
        {
            temp.GetComponent<Image>().sprite = defaultCardImage;
        }
        else 
        {
            temp.GetComponent<CardBehaviour>().SetupCard(data);    
        }

        //Beállítjuk a kártya láthatóságát
        temp.GetComponent<CardBehaviour>().SetVisibility(visible);

        //Kártya objektum hozzáadása a kéz mezőhöz
        AddCardToHand(temp);
    }

    public void UpdateDeckCounter(int newDeckSize)
    {   
        deckSize.text = newDeckSize.ToString();
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
        RemoveSpecificCard(card);
        cardsInField.Add(card);

        //Itt még ad egy jelzést a UI controller felé, amit ő továbbít a Game Controllernek 
    }

    public void PutCardIntoWinners()
    {
        if(cardsInField.Any())
        {
            for(var i = 0; i < cardsInField.Count; i++)
            {
                if(i == cardsInField.Count - 1)
                {
                    winnerCardImage.sprite = cardsInField[i].GetComponent<Image>().sprite;
                }
                Destroy(cardsInField[i]);
                
            }

            cardsInField.Clear();
        }
    }

    public void PutCardIntoLosers()
    {
        if(cardsInField.Any())
        {
            for(var i = 0; i < cardsInField.Count; i++)
            {
                if(i == cardsInField.Count - 1)
                {
                    lostCardImage.sprite = cardsInField[i].GetComponent<Image>().sprite;
                }
                Destroy(cardsInField[i]);
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

    public void AddDeckSize(TMP_Text deckSizeIn, int cardsInDeckIn)
    {
        this.deckSize = deckSizeIn;
        this.cardsInDeck = cardsInDeckIn;
        deckSize.text = cardsInDeck.ToString();
    }

    public void DisplayDetails(Card data)
    {
        controller.DisplayCardDetail(data);
    }

    public void HideDetails()
    {
        controller.HideCardDetail();
    }

}