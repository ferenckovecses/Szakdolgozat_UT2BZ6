using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Main_Menu_Controller : MonoBehaviour
{

    [Header("UI Elem Referenciák")]
    public GameObject displayPanel;
    public TMP_Text titleText;
    public GameObject profilePanel;
    public GameObject mainCanvas;
    public TMP_Text profileStatus;

    [Header("Prefab Elemek")]
    public Button buttonPrefab;
    public GameObject profileSelectPrefab;
    public GameObject notificationPanel;

    [Header("Adattárolók")]
    public List<Button> menuButtons;
    GameSettings_Controller dataController;
    MenuState currentState;
    bool needsUpdate;
    bool isLoggedIn = false;

    void Start()
    {
        dataController = GameObject.Find("GameData_Controller").GetComponent<GameSettings_Controller>();
        menuButtons = new List<Button>();
        currentState = MenuState.MainMenu;
        CheckIfAlreadyLoggedIn();
        needsUpdate = true;
    }

    void Update()
    {
        if(needsUpdate)
        {
            needsUpdate = false;

            //Menügombok változtatása menüfázis szerint
            ResetButtons();
            switch (currentState) 
            {
                case MenuState.MainMenu: SetupMainMenu(); break;
                case MenuState.GameTypeSelect: SetupGameSelect(); break;
                case MenuState.PlayerNumberSelect: SetupPlayerNumberSelect(); break;
                case MenuState.MultiplayerSettings: SetupMultiplayerMode(); break;
                default: break;
            }

            //Profil státusz üzenetének változtatása
            if(!isLoggedIn)
            {
                profileStatus.text = "Nincs Aktív Profil!";
            }
            else 
            {
                profileStatus.text = "Üdv, " + dataController.GetActivePlayer().GetUsername() + "!";    
            }
        }
    }

    void SetupMainMenu()
    {
        titleText.text = "Master Crok";
        var titles = new List<string> {"Új Játék", "Egyéb Opciók", "Információk","Kártyatár", "Kilépés"};
        var functions = new List<Action> {StartNewGame, Other, HelpSection, OpenEncyclopedia, ExitGame};
        CreateButtons(titles, functions); 
    }

    void SetupGameSelect()
    {
        titleText.text = "Válassz játékmódot!";
        var titles = new List<string> {"Egyjátékos mód", "Többjátékos mód", "Vissza"};
        var functions = new List<Action> {SinglePlayer, Multiplayer, Back};
        CreateButtons(titles, functions); 
    }

    void SetupPlayerNumberSelect()
    {
        var titles = new List<string> {"1v1", "1v2", "1v3", "Vissza"};
        var functions = new List<Action>();
        for(var i = 0; i < 3; i++)
        {
            int temp = i+1;
            functions.Add(new Action(() => StartSinglePlayer(temp)));
        }
        functions.Add(new Action(() => Back()));
        CreateButtons(titles, functions); 
    }

    void SetupMultiplayerMode()
    {
        var titles = new List<string> {"Hosztolás", "Csatlakozás", "Vissza"};
        var functions = new List<Action>();
        for(var i = 0; i < titles.Count-1; i++)
        {
            functions.Add(new Action(() => StartMultiplayer()));
        }
        functions.Add(new Action(() => Back()));
        CreateButtons(titles, functions); 
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenEncyclopedia()
    {
        SceneManager.LoadScene("Card_Encyclopedia");
    }

    public void StartNewGame()
    {
        if(isLoggedIn)
        {
            currentState = MenuState.GameTypeSelect;
            needsUpdate = true;
        }
        else 
        {
            string msg = "Ehhez az akcióhoz be kell jelentkezned egy játékos profilba!";
            MakeNotification(msg);    
        }
    }

    public void OpenShop()
    {
        Debug.Log("Majd...");
    }

    public void HelpSection()
    {
        Debug.Log("TODO");
    }

    public void Credits()
    {
        Debug.Log("TODO");
    }

    public void Other()
    {

    }

    public void OpenSettings()
    {
        currentState = MenuState.Settings;
        needsUpdate = true;
    }

    public void SinglePlayer()
    {
        currentState = MenuState.PlayerNumberSelect;
        needsUpdate = true;
    }

    public void Multiplayer()
    {
        currentState = MenuState.MultiplayerSettings;
        needsUpdate = true;
    }

    public void Back()
    {
        switch (currentState) 
        {
            case MenuState.GameTypeSelect: currentState = MenuState.MainMenu; break;
            case MenuState.PlayerNumberSelect: currentState = MenuState.GameTypeSelect; break;
            case MenuState.MultiplayerSettings: currentState = MenuState.GameTypeSelect; break;
            default: break;
        }
        needsUpdate = true;
    }

    public void StartSinglePlayer(int playerNumber)
    {
        dataController.SetOpponents(playerNumber);
        SceneManager.LoadScene("SinglePlayer_Battle");
    }

    public void StartMultiplayer()
    {
        Debug.Log("Multiplayer coming soon...");
    }

    void CreateButtons(List<string> titles, List<Action> functions)
    {
        if(titles.Count != functions.Count)
        {
            return;
        }
        var i = 0;
        foreach(Action function in functions) 
        {
            Button temp = Instantiate(buttonPrefab,displayPanel.transform);
            temp.name = titles[i];
            temp.GetComponentInChildren<TMP_Text>().text = titles[i];
            temp.onClick.AddListener(delegate{function();});
            i++;
            menuButtons.Add(temp);
        }
    }

    void ResetButtons()
    {
        if(menuButtons.Count > 0)
        {
            for(var i = 0; i < menuButtons.Count; i++) 
            {
                Destroy(menuButtons[i].gameObject);
            }
        }
        menuButtons.Clear();
    }

    public void ProfileSelect()
    {
        Instantiate(profileSelectPrefab, mainCanvas.transform.position, Quaternion.identity, mainCanvas.transform);
    }

    public void PlayerLoginStatus(bool status)
    {
        needsUpdate = true;
        isLoggedIn = status;
    }

    void MakeNotification(string msg)
    {
        GameObject temp = Instantiate(notificationPanel,mainCanvas.transform.position,Quaternion.identity,mainCanvas.transform);
        temp.transform.Find("Notification_Controller").GetComponent<Notification_Controller>().ChangeText(msg);
    }

    void CheckIfAlreadyLoggedIn()
    {
        if(dataController.GetActivePlayer() != default(Player))
        {
            isLoggedIn = true;
        }   
    }


}
