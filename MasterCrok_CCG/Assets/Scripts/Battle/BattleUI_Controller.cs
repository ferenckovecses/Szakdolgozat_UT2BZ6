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
    public List<GameObject> playerFields;
    public GameObject playerFieldCanvas;
    public GameObject HUDcanvas;
    public TMP_Text deckSize;

    [Header("Teszt funkció gombok")]
    public Button drawButton;
    public Button discardButton;
    public Button winButton;
    public Button loseButton;
    int cardsInDeck = 55;


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
        for(var i = 0; i<playerNumber+1; i++)
        {
            //Pozíció és beállítások meghatározása
            switch (i) 
            {
                case 0: x = 0; y = 0f; rotation = 0f; break;
                case 1: x = 1280f; y = 720f; rotation = 180f; break;
                case 2: x = 0f; y = 1000f; rotation = -90f; break;
                case 3: x = 1280f; y = -360f; rotation = 90f; break;
                default: x = 0f; y = 0f; rotation = 0f; break;
            }

            //Létrehozás
            GameObject temp = Instantiate(prefabPlayerUI, Vector3.zero, Quaternion.identity, playerFieldCanvas.transform);
            temp.transform.position = new Vector3(x,y,0f);
            temp.transform.rotation = Quaternion.Euler(0f,0f,rotation);

            //Eltárolás későbbi referenciához
            playerFields.Add(temp);

            //A játékos
            if(i == 0)
            {
                temp.name = "Player";
                GameObject.Find("ActiveCardField").gameObject.tag = "PlayerField";
                GetScript(i).SetPlayerIdentity(true);
                GetScript(i).ChangeOpenHand(true);
                //A játékosnak adunk egy pakli amount trackert
                TMP_Text deckNumber = Instantiate(deckSize,GameObject.Find("Deck").transform.position, Quaternion.identity, GameObject.Find("Deck").transform);
                GetScript(i).AddDeckSize(deckNumber, cardsInDeck);
            }
            //Az ellenfelek
            else 
            {
                temp.name = "Opponent " + i.ToString();
                GetScript(i).SetPlayerIdentity(false);
                GetScript(i).ChangeOpenHand(false);  
            }

            //Eltároljuk, hogy melyik pozícióban van a játékos mező
            GetScript(i).SetPosition(i,x,y,rotation);

        }
        //A mi oldalunk legyen az utolsó a hierarchiában
        playerFields[0].transform.SetAsLastSibling();
    }

    public void DrawCardForAll()
    {
        for(var i=0; i < playerFields.Count; i++)
        {
            GetScript(i).CreateCard();
        }
    }

    public void DiscardCardForAll()
    {
        for(var i=0; i < playerFields.Count; i++)
        {
            GetScript(i).DiscardCard();
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
    PlayerUIelements GetScript(int index)
    {
        return this.playerFields[index].GetComponent<PlayerUIelements>();
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

}
