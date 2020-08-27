using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/*
Feladata: Kapcsolatban van a játék vezérlővel, informálják egymást és a játékos számára frissíti a harci UI-t,
hogy az aktuális játék állapotokat tükrözze,
illetve továbbítja a vezérlőnek a játékos inputjait.
*/

public class BattleUI_Controller : MonoBehaviour
{
    [Header("Játékvezérlő")]
    public BattleController controller;

    [Header("UI prefabok")]
    public GameObject prefabPlayerUI;
    public GameObject prefabExitPanel;
    public GameObject prefabDetailedView;
    public GameObject cardWithDetails;

    [Header("Adattárolók és referenciák")]
    public List<GameObject> positions;
    public Dictionary <int, GameObject> playerFields;
    public GameObject playerFieldCanvas;
    public GameObject HUDcanvas;
    public TMP_Text deckSize;
    GameObject cardDetailWindow;

    [Header("Teszt funkció elemek")]
    public Button drawButton;
    public Button discardButton;
    public Button winButton;
    public Button loseButton;


    // Start is called before the first frame update
    void Start()
    {
        playerFields = new Dictionary<int, GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //Elhelyezi a megadott számú játékosnak megfelelő pályaelemet
    public void CreatePlayerFields(int playerNumber, List<int> keys, int playerDeckSize)
    {
        for(var i = 0; i<playerNumber+1; i++)
        {

            //Létrehozás
            GameObject temp = Instantiate(prefabPlayerUI, positions[i].transform.position, positions[i].transform.rotation, positions[i].transform);

            //Eltárolás későbbi referenciához
            playerFields.Add(keys[i], temp);

            //A játékos beállítása
            if(i == 0)
            {
                temp.name = "Player";
                GameObject.Find("ActiveCardField").gameObject.tag = "PlayerField";
                GetScript(keys[i]).ChangeOpenHand(true);
            }
            //Az ellenfelek
            else 
            {
                temp.name = "Opponent " + i.ToString();
                GetScript(keys[i]).ChangeOpenHand(false);  
            }

            //Eltároljuk, hogy melyik pozícióban van a játékos mező
            GetScript(keys[i]).SetPosition(i);
        }

        //A mi oldalunk legyen az utolsó a hierarchiában
        positions[0].transform.SetAsLastSibling();

        //A játékosnak adunk egy deck számlálót
        GameObject deck = GameObject.Find("Field1/Player/Deck");
        TMP_Text deckNumber = Instantiate(deckSize,deck.transform.position, Quaternion.identity, deck.transform);
        deckNumber.name = "DeckCounter";
        GetScript(keys[0]).AddDeckSize(deckNumber, playerDeckSize);
        
    }

    public void DrawCardForAll()
    {
        for(var i=0; i < playerFields.Count; i++)
        {
            //GetScript(i).CreateCard();
        }
    }

    public void DiscardCardForAll()
    {
        for(var i=0; i < playerFields.Count; i++)
        {
            //GetScript(i).DiscardCard();
        }
    }

    public void WinRound()
    {
        GetScript(0).PutCardIntoWinners();
    }

    public void LoseRound()
    {
        GetScript(0).PutCardIntoLosers();
    }

    //Shortcut: Visszaadja a listában szereplő gameObjecten lévő scriptet, hogy közvetlenül referálhassuk 
    PlayerUIelements GetScript(int key)
    {
        return this.playerFields[key].GetComponent<PlayerUIelements>();
    }

    public void ExitBattleButton()
    {
        //Megjeleníti a kérdőablakot
        GameObject exitPanel = Instantiate(prefabExitPanel, HUDcanvas.transform.position, Quaternion.identity, HUDcanvas.transform);
        //If yes: ExitBattle()
        Button yesBtn = GameObject.Find("YesButton").GetComponent<Button>();
        yesBtn.onClick.AddListener(delegate{ExitBattle();});
        //If no: Destroy window, continue game
        Button noBtn = GameObject.Find("NoButton").GetComponent<Button>();
        noBtn.onClick.AddListener(delegate{ContinueBattle(exitPanel);});        
    }

    public void ExitBattle()
    {
        //Send signal to the game manager and server
        //Replace player with a bot

        controller.ResetDecks();

        //Visszatérés a menübe
        SceneManager.LoadScene("Main_Menu");
    }

    public void ContinueBattle(GameObject exitPanel)
    {
        Destroy(exitPanel);
    }

    public void CreateCardDetailScreen()
    {
        GameObject detailedView = Instantiate(prefabDetailedView, HUDcanvas.transform.position,
                                Quaternion.identity, HUDcanvas.transform);
    }

    //Továbbítja a kapott új kártya adatokat a megfelelő játékos oldalra
    public void DisplayNewCard(Card data, int key, bool visible)
    {
        playerFields[key].GetComponent<PlayerUIelements>().CreateCard(data, visible);
    }

    public void RefreshDeckSize(int key, int newDeckSize)
    {   
        playerFields[key].GetComponent<PlayerUIelements>().UpdateDeckCounter(newDeckSize);
    }

    public void DisplayCardDetail(Card data)
    {
        cardDetailWindow = Instantiate(cardWithDetails, playerFieldCanvas.transform.position, Quaternion.identity, playerFieldCanvas.transform);
        cardDetailWindow.transform.Find("CardDisplay_Controller").GetComponent<CardDisplay_Controller>().SetupDisplay(data);

    }

    public void HideCardDetail()
    {
        Destroy(cardDetailWindow);
    }

}
