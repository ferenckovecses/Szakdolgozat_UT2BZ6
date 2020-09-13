using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Assets.Scripts.Interface;
using Assets.Scripts.SinglePlayer;
using System.Runtime.CompilerServices;

public class SinglePlayer_Client : MonoBehaviour, IClient
{
    [Header("Játékvezérlő")]
    [SerializeField]
    private SinglePlayer_Controller controller;

    [Header("UI prefabok")]
    public GameObject prefabPlayerUI;
    public GameObject prefabExitPanel;
    public GameObject prefabDetailedView;
    public GameObject cardWithDetails;
    public GameObject statChoiceBox;
    public GameObject prefabMessageText;
    public TMP_Text deckSize;

    [Header("Referenciák")]
    public List<GameObject> positions;
    public GameObject playerFieldCanvas;
    public GameObject HUDcanvas;
    public GameObject notificationCanvas;
    public GameObject ingamePanelCanvas;
    public TMP_Text activeStatText;
    public GameObject activeStatPanel;

    //Adattárolók
    private Dictionary <int, GameObject> playerFields;
    private SinglePlayer_Report reportModule;
    private bool isMessageOnScreen;
    private bool isDetailsShowed;
    private bool isDetailsFixed;
    private bool isExitPanelExist;
    private GameObject cardDetailWindow;
    private GameObject statPanel;
    private GameObject messageText;

    //Létrehozáskor fut le
    private void Awake()
    {
        playerFields = new Dictionary<int, GameObject>();
        isMessageOnScreen = false;
        isDetailsShowed = false;
        isDetailsFixed = false;
        isExitPanelExist = false;
    }

    #region Report
    //A veezérlőnek ad jelentést játékos általi kártya idézésről
    public void ReportSummon(int handIndex, int position)
    {
        int playerKey = playerFields.Keys.ElementAt(position);
        reportModule.ReportSummon(handIndex, playerKey);
    }
    //Jelez a vezérlőnek a felhasználó döntéséről a megadott kártyát illetően, Gombok használják
    public void ReportSkillDecision(SkillState state, int key, int cardFieldPosition, int cardTypeID)
    {
        HideCardDetailsWindow();
        bool isItTheLast = GetFieldFromKey(key).SwitchCardSkillStatus(state, cardFieldPosition);
        reportModule.ReportSkillStatusChange(state, key, isItTheLast, cardFieldPosition);
    }
    //A vezérlőnek jelzünk a kilépésről, ő pedig ellátja a szükséges feladatokat
    public void ReportPlayerExit()
    {
        reportModule.ReportExit();
    }

    #endregion

    #region Common Instruction


    //Elhelyezi a megadott számú játékosnak megfelelő pályaelemet a megadott pozíciókba
    public void CreatePlayerFields(int playerAmount, List<int> playerKeys, int displayedDeckSize)
    {
        for (var i = 0; i < playerAmount + 1; i++)
        {
            //Létrehozás
            GameObject temp = Instantiate(prefabPlayerUI, positions[i].transform.position, positions[i].transform.rotation, positions[i].transform);
            
            //Eltárolás későbbi referenciához
            playerFields.Add(playerKeys[i], temp);

            //Kliens referencia eltárolása
            GetFieldFromKey(playerKeys[i]).SetClient(this);

            //A játékos beállítása
            if (i == 0)
            {
                temp.name = "Player";
                GameObject.Find("ActiveCardField").gameObject.tag = "PlayerField";
                GetFieldFromKey(playerKeys[i]).ChangeOpenHand(true);
            }
            //Az ellenfelek
            else
            {
                temp.name = "Opponent " + i.ToString();
                GetFieldFromKey(playerKeys[i]).ChangeOpenHand(false);
            }

            //Eltároljuk, hogy melyik pozícióban van a játékos mező
            GetFieldFromKey(playerKeys[i]).SetPosition(i);
        }

        //A mi oldalunk legyen az utolsó a hierarchiában
        positions[0].transform.SetAsLastSibling();

        //A játékosnak adunk egy deck számlálót
        GameObject deck = GameObject.Find("Field1/Player/Deck");
        TMP_Text deckNumber = Instantiate(deckSize, deck.transform.position, Quaternion.identity, deck.transform);
        deckNumber.name = "DeckCounter";
        GetFieldFromKey(playerKeys[0]).AddDeckSize(deckNumber, displayedDeckSize);

    }
    //Kilépés
    public void Exit()
    {
        //A vezérlőnek jelzünk a kilépésről
        ReportPlayerExit();

        //Visszatérés a menübe
        SceneManager.LoadScene("Main_Menu");
    }

    //Játék folytatása
    public void ContinueBattle(GameObject exitPanel)
    {
        Destroy(exitPanel);
        isExitPanelExist = false;
    }

    //Továbbítja a kapott új kártya adatokat a megfelelő játékos oldalra, hogy vizuálisan megjelenítse azokat
    public void DisplayNewCard(Card data, int playerKey, bool visibleForThePlayer, bool isABlindDraw)
    {
        //Ha nem vakon húztuk
        if (!isABlindDraw)
        {
            GameObject temp = GetFieldFromKey(playerKey).CreateCard(data, visibleForThePlayer);

            //Kártya objektum hozzáadása a kéz mezőhöz
            GetFieldFromKey(playerKey).AddCardToHand(temp);
        }

        else
        {
            GameObject temp = GetFieldFromKey(playerKey).CreateCard(data, false);
            GetFieldFromKey(playerKey).BlindSummon(temp);
        }
    }
    //A megadott oldalú játékosnak engedélyezi/tiltja a kártya mozgatás/húzás/lerakás interakciókat
    public void SetDragStatus(int playerKey, bool newStatus)
    {
        GetFieldFromKey(playerKey).SetDraggableStatus(newStatus);
    }
    //Felfedi a megadott oldalú játékos lerakott kártyáit
    public void RevealCards(int playerKey)
    {
        GetFieldFromKey(playerKey).RevealCardsOnField();
    }
    //A megadott játékosnál lévő és megadott pozícióban található kártya képességét reseteli
    public void ResetCardSkill(int playerKey, int cardPosition)
    {
        GetFieldFromKey(playerKey).ResetCardSkill(cardPosition);
    }
    //Új Skill ciklus: A tartalékolt képességekről újra lehet dönteni
    public void NewSkillCycle(int playerKey)
    {
        GetFieldFromKey(playerKey).NewSkillCycle();
    }
    //Beállítjuk, hogy a megadott játékos rendelkezhet-e képességeiről
    public void SetSkillStatus(int playerKey, bool newStatus)
    {
        GetFieldFromKey(playerKey).SetSkillStatus(newStatus);
    }
    //Kör végén a kör eredményével parancsot küld a megfelelő játékos UI-nak, hogy rakja el a lerakott lapokat
    public void PutCardsAway(int playerKey, bool isWinner)
    {
        GetFieldFromKey(playerKey).PutCardsAway(isWinner);
    }

    #endregion

    #region Player Specific Methods
    //A Harctípus választó ablak gombjainak függvénye
    public void ChooseStatButton(ActiveStat stat)
    {
        activeStatPanel.SetActive(true);
        reportModule.ReportStatChange(stat);
        Destroy(statPanel);
    }
    //HUD exit gomb
    public void ExitBattleButton()
    {
        if (!isExitPanelExist)
        {
            isExitPanelExist = true;
            //Megjeleníti a kérdőablakot
            GameObject exitPanel = Instantiate(prefabExitPanel, HUDcanvas.transform.position, Quaternion.identity, HUDcanvas.transform);
            //If yes: ExitBattle()
            Button yesBtn = GameObject.Find("YesButton").GetComponent<Button>();
            yesBtn.onClick.AddListener(delegate { Exit(); });
            //If no: Destroy window, continue game
            Button noBtn = GameObject.Find("NoButton").GetComponent<Button>();
            noBtn.onClick.AddListener(delegate { ContinueBattle(exitPanel); });
        }
    }
    //Frissíti a játékos paklijának méretét a modell szerinti aktuális értékre
    public void RefreshDeckSize(int key, int newDeckSize)
    {
        GetFieldFromKey(key).UpdateDeckCounter(newDeckSize);
    }

    //Megjeleníti a kijelölt kártya részletes nézetét
    public void DisplayCardDetaislWindow(Card data, int displayedCardId, bool skillDecision, int position)
    {
        cardDetailWindow = Instantiate(cardWithDetails, ingamePanelCanvas.transform.position, Quaternion.identity, ingamePanelCanvas.transform);
        cardDetailWindow.GetComponent<CardDisplay_Controller>().SetupDisplay(data, skillDecision);
        //Ha a gombok is megjelennek, akkor be kell konfigurálni is őket
        if (skillDecision)
        {
            SetFixedDetails(true);
            //Eltároljuk, hogy melyik kulccsal rendelkező pozícióról jött a kérés
            int tempKey = playerFields.Keys.ElementAt(position);
            cardDetailWindow.GetComponent<CardDisplay_Controller>().SetupButtons(data, this, displayedCardId, tempKey);
        }
    }

    //Eltünteti a kártya részletes nézetet
    public void HideCardDetailsWindow()
    {
        Destroy(cardDetailWindow);

        //Ha fix megjelenítés volt akkor kapcsoljuk ki
        if (GetFixedDetailsStatus())
        {
            SetFixedDetails(false);
        }
    }
    //Megváltoztatja a harctípus szöveget az épp aktuális, játékvezérlő szerinti értékre
    public void ChangeStatText(string newType)
    {
        this.activeStatText.text = newType;
    }
    //A játékmezőn megjelenít üzeneteket a játékosok számára (pl kinek a köre van, kör eredménye)
    public bool DisplayMessage(string msg)
    {
        //Ha nincs még megjelenített üzenet
        if (!isMessageOnScreen)
        {
            messageText = Instantiate(prefabMessageText, notificationCanvas.transform.position, Quaternion.identity, notificationCanvas.transform);
            messageText.GetComponent<TMP_Text>().text = msg;
            isMessageOnScreen = true;
            return true;

        }

        //Ha van már épp megjelenített üzenet, akkor nem jeleníti meg rajta 
        else
        {
            return false;
        }
    }
    //Eltünteti a megjelenített üzenetet
    public void HideMessage()
    {
        if (isMessageOnScreen)
        {
            Destroy(messageText);
            isMessageOnScreen = false;
        }
    }
    //Megjeleníti a játékosnak a harctípus választó ablakot
    public void DisplayStatBox()
    {
        activeStatPanel.SetActive(false);
        statPanel = Instantiate(statChoiceBox, ingamePanelCanvas.transform.position, Quaternion.identity, ingamePanelCanvas.transform);
        statPanel.GetComponent<StatChoice_Controller>().SetupPanel(this);
    }
    #endregion

    #region Bot Specific Methods
    //Botok UI-jának küld egy parancsot egy megadott indexű kártya lerakására/aktiválására
    public void SummonCard(int playerKey, int cardHandIndex)
    {
        GetFieldFromKey(playerKey).SummonCard(cardHandIndex);
    }

    #endregion

    #region Getter
    //Visszaadja, hogy jelenleg van-e megjelenített részletes kártya nézet ablak
    public bool GetCardDetailWindowStatus()
    {
        return this.isDetailsShowed;
    }
    //Visszaadja, hogy jelenleg fix részletes nézet van-e
    public bool GetFixedDetailsStatus()
    {
        return this.isDetailsFixed;
    }
    //Shortcut: Visszaadja a listában szereplő gameObjecten lévő scriptet, hogy közvetlenül referálhassuk 
    private Player_UI GetFieldFromKey(int key)
    {
        return this.playerFields[key].GetComponent<Player_UI>();
    }
    #endregion

    #region Setter
    //Beállíthatjuk a részletes kártya nézet ablak státuszát
    public void SetDetailsStatus(bool newStatus)
    {
        this.isDetailsShowed = newStatus;
    }
    //Beállítjuk, hogy fix módban van-e a részletes ablak
    public void SetFixedDetails(bool newStatus)
    {
        this.isDetailsFixed = newStatus;
    }

    public void SetReportAgent(SinglePlayer_Report rep)
    {
        this.reportModule = rep;
    }

    #endregion

}
