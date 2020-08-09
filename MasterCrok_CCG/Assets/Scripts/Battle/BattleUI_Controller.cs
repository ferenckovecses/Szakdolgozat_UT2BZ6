using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;


/*
Feladata: Kapcsolatban van a játék vezérlővel, informálják egymást és a játékos számára frissíti a harci UI-t,
hogy az aktuális játék állapotokat tükrözze,
illetve továbbítja a vezérlőnek a játékos inputjait.
*/

public class BattleUI_Controller : MonoBehaviour
{
    [Header("Játékvezérlő")]
    public BattleController controller;

    [Header("Játékos UI prefabok")]
    public GameObject prefabPlayerUI;
    public List<GameObject> playerFields;
    public GameObject canvas;

    [Header("Teszt funkció gombok")]
    public Button drawButton;
    public Button discardButton;

	[Header("Kártya elemek")]
	public GameObject cardPrefab;

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
        playerFields = new List<GameObject>();
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
            GameObject temp = Instantiate(prefabPlayerUI, Vector3.zero, Quaternion.identity, canvas.transform);
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
            playerFields.Add(temp);

            //Eltároljuk, hogy melyik pozícióban van a játékos mező
            GetScript(i).SetPosition(i,x,y,rotation);
        }
    }

    //Létrehoz egy kártya prefabot a kézbe
    public void CreateCard()
    {

        for(var i = 0; i<playerFields.Count; i++)
        {
            GameObject temp = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, GetScript(i).GetHandField().transform);
            temp.transform.position = GetScript(i).GetHandField().transform.position;

            /*
            //Színezés a változatosabb teszteléshez
            if(GetScript(i).GetCardCount() % 2 == 0)
            {
                temp.GetComponent<Image>().color = Color.blue;
            }
            */

            //Kártya hozzáadása a játékos kezéhez
            GetScript(i).AddCard(temp);
        }

    }

    //Eltávolít egy kártya prefabot a kézből
    public void DiscardCard()
    {
        for(var i = 0; i < playerFields.Count; i++)
        {
            //Ha van eltávolítandó kártya
            if(GetScript(i).GetCards().Any())
            {
                //Akkor távolítsa el
                Destroy(GetScript(i).GetACard());
                GetScript(i).RemoveCard();
            }
        }

    }

    //Shortcut: Visszaadja a listában szereplő gameObjecten lévő scriptet, hogy közvetlenül referálhassuk 
    PlayerUIelements GetScript(int index)
    {
        return this.playerFields[index].GetComponent<PlayerUIelements>();
    }
}
