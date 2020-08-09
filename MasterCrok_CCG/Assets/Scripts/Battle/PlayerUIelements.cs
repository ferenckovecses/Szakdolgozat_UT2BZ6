using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
Feladata: A játék UI vezérlője kezeli, minden PlayerUIelements egy-egy játékosnak az aktuális játékbeli állását reprezentálja
és ezt jeleníti meg a képernyőn a játékos számára. 

Adatfolyam:
[PlayerUIelements] <- UI vezérlő <-> Játékvezérlő <-> Szerver 
*/


[System.Serializable]
public class PlayerUIelements : MonoBehaviour
{
    [Header("Játékos mező referenciák")]
    public GameObject handField;
    public GameObject activeCardField;
    public Image deckImage;
    public Image lostCardImage;
    public Image winnerCardImage;


    [Header("Kártyákkal kapcsolatos változók")]
    public List<GameObject> cards;
    int cardCount;
    public bool openHand;

    [Header("Kártya a kézben hatás")]
    float turningPerCard = 5f;
    float cardDistance = 50f;
    float cardHeightDiff = 5f;

    [Header("UI Pozícionálás")]
    int positionID;
    float x,y,rotationZ;

    void Start()
    {
        this.cards = new List<GameObject>();
        this.cardCount = cards.Count;
    }

    public void AddCard(GameObject card)
    {
        cards.Add(card);
        cardCount = cards.Count;
        SortCards();
    }

    public void RemoveCard(int index = 0)
    {
        cards.RemoveAt(index);
        cardCount = cards.Count;
        SortCards();
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

    public List<GameObject> GetCards()
    {
        return this.cards;
    }

    public GameObject GetACard(int index = 0)
    {
        return this.cards[index];
    }

    //Tárolja, hogy milyen pozícióban van a játékos mező
    //És frissíti, hogy ez milyen eltolásokkal/forgatásokkal jár
    public void SetPosition(int id, float x, float y, float rotZ)
    {
        this.positionID = id;
        this.x = x;
        this.y = y;
        this.rotationZ = rotZ;
    }

    //Elrendezi a kézben lévő kártyákat
    void SortCards()
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
            this.GetACard(i).transform.position = Vector3.zero;
            this.GetACard(i).transform.rotation = Quaternion.Euler(0f,0f,this.rotationZ);

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
                cardTurning = this.rotationZ + (-turningPerCard * (middleCardIndex-i));
            }
            else 
            {
                cardTurning = this.rotationZ + (turningPerCard * (middleCardIndex-i));
            }
            this.GetACard(i).transform.rotation = Quaternion.Euler(0f,0f,cardTurning);

        }
    }

}