using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class BattleUI_Controller : MonoBehaviour
{

    [Header("Játékos UI prefabok")]
    public GameObject prefabPlayerUI;
    public List<PlayerUIelements> playerFields;
    public GameObject canvas;

    [Header("Teszt gombok")]
    public Button drawButton;
    public Button discardButton;

	[Header("Játékos oldala")]
	public GameObject cardPrefab;

    [Header("Kártya a kézben hatás")]
	float turningPerCard = 5f;
	float cardDistance = 50f;
	float cardLevelDifference = 5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*Cheat sheet:
    P1: 0 0 R:0
    P2: 0 0 R:180
    P3: -280 0 R:-90
    P4: 280 0 R:90
    */

    //Elhelyezi a megadott számú játékosnak megfelelő pályaelemet
    public void CreatePlayerFields(int playerNumber)
    { 
        float x,y,rotation;
        for(var i = 0; i<playerNumber; i++)
        {
            //Pozíció és beállítások meghatározása
            switch (i) 
            {
                case 0: x = 0; y = 0f; rotation = 0f; break;
                case 1: x = 0; y = 0f; rotation = 180f; break;
                case 2: x = -280; y = 0f; rotation = -90f; break;
                case 3: x = 280; y = 0f; rotation = 90f; break;
                default: x = 0f; y = 0f; rotation = 0f; break;
            }

            //Létrehozás
            GameObject temp =Instantiate(prefabPlayerUI, Vector3.zero, Quaternion.identity, canvas.transform);
            temp.transform.position = canvas.transform.position + new Vector3(x,y,0f);
            temp.transform.rotation = Quaternion.Euler(0f,0f,rotation);

            //Elnevezés
            if(i == 0)
            {
                temp.name = "Player";
            }
            else 
            {
                temp.name = "Opponent " + i.ToString();    
            }

            //Eltárolás későbbi referenciához
            PlayerUIelements newElement = new PlayerUIelements(temp);
            playerFields.Add(newElement);
        }
    }

    public void CreateCard()
    {
    	GameObject temp = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, playerFields[0].GetField().transform);
    	temp.transform.position = playerFields[0].GetField().transform.position;

        //Színezés a változatosabb teszteléshez
    	if(playerFields[0].GetCardCount() % 2 == 0)
        {
    		temp.GetComponent<Image>().color = Color.blue;
        }
        //Kártya hozzáadása a játékos profiljához
    	playerFields[0].AddCard(temp);
    	SortCards(playerFields[0]);
    }

    public void DiscardCard()
    {
    	if(playerFields[0].GetCards().Any())
    	{
    		Destroy(playerFields[0].GetACard());
    		playerFields[0].RemoveCard();
    		SortCards(playerFields[0]);
    	}
    }

    void SortCards(PlayerUIelements player)
    {
    	int middleCardIndex;
    	//Páros lapok esetén
    	if(player.GetCardCount() % 2 == 0)
    	{
    		middleCardIndex = (player.GetCardCount() / 2) -1;
    		int secondMiddleCardIndex = middleCardIndex + 1;
    	}

    	//Páratlan lapok esetén
    	else 
    	{
    		middleCardIndex = (player.GetCardCount() / 2);
    	}

    	
    	float lastPositionDistance = -cardDistance * middleCardIndex;
    	float cardHeight = -cardLevelDifference * middleCardIndex;
    	for(var i = 0; i < player.GetCardCount(); i++)
    	{
    		//Pozíciók módosítása
    		player.GetACard(i).transform.position = player.GetField().transform.position +
    		new Vector3(lastPositionDistance,cardHeight.RandomLevelDifference(),0f);

    		lastPositionDistance += cardDistance;

    		if(i <= middleCardIndex)
    		{
    			cardHeight += cardLevelDifference;
    		}
    		else 
    		{
    			cardHeight -= cardLevelDifference;	
    		}

    		//Alap rotáció
    		player.GetACard(i).transform.rotation = Quaternion.Euler(0f,0f,turningPerCard * (player.GetCardCount() -i));
    	}
    }
}

public class PlayerUIelements
{
    GameObject field;
    List<GameObject> cards;
    int cardCount;

    public PlayerUIelements(GameObject field)
    {
        this.field = field;
        cards = new List<GameObject>();
        cardCount = cards.Count;
    }

    public void AddCard(GameObject card)
    {
        cards.Add(card);
        cardCount = cards.Count;
    }

    public void RemoveCard(int index = 0)
    {
        cards.RemoveAt(index);
        cardCount = cards.Count;
    }

    public GameObject GetField()
    {
        return this.field;
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
        return this.cards[0];
    }


}
