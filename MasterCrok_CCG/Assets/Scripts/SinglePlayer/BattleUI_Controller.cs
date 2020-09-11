using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleUI_Controller : MonoBehaviour
{
    [Header("Játékvezérlő")]
    [SerializeField]
    private BattleController controller;

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
    public TMP_Text activeStatText;
    public GameObject activeStatPanel;

    //Adattárolók
    private Dictionary <int, GameObject> playerFields;
    private bool isMessageOnScreen;
    private bool isDetailsShowed;
    private bool isDetailsFixed;
    private GameObject cardDetailWindow;
    private GameObject statPanel;
    private GameObject messageText;

    // Start is called before the first frame update
    void Awake()
    {
        playerFields = new Dictionary<int, GameObject>();
        isMessageOnScreen = false;
        isDetailsShowed = false;
        isDetailsFixed = false;
    }

    //Elhelyezi a megadott számú játékosnak megfelelő pályaelemet a megadott pozíciókba
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
                GetFieldFromKey(keys[i]).ChangeOpenHand(true);
            }
            //Az ellenfelek
            else 
            {
                temp.name = "Opponent " + i.ToString();
                GetFieldFromKey(keys[i]).ChangeOpenHand(false);  
            }

            //Eltároljuk, hogy melyik pozícióban van a játékos mező
            GetFieldFromKey(keys[i]).SetPosition(i);
        }

        //A mi oldalunk legyen az utolsó a hierarchiában
        positions[0].transform.SetAsLastSibling();

        //A játékosnak adunk egy deck számlálót
        GameObject deck = GameObject.Find("Field1/Player/Deck");
        TMP_Text deckNumber = Instantiate(deckSize,deck.transform.position, Quaternion.identity, deck.transform);
        deckNumber.name = "DeckCounter";
        GetFieldFromKey(keys[0]).AddDeckSize(deckNumber, playerDeckSize);
        
    }


    //Shortcut: Visszaadja a listában szereplő gameObjecten lévő scriptet, hogy közvetlenül referálhassuk 
    PlayerUIelements GetFieldFromKey(int key)
    {
        return this.playerFields[key].GetComponent<PlayerUIelements>();
    }

    //A csatából kilépés gombjának függvénye
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

    //Kilépés
    public void ExitBattle()
    {
        controller.ResetDecks();

        //Visszatérés a menübe
        SceneManager.LoadScene("Main_Menu");
    }

    //Játék folytatása
    public void ContinueBattle(GameObject exitPanel)
    {
        Destroy(exitPanel);
    }

    public void CreateCardDetailScreen()
    {
        GameObject detailedView = Instantiate(prefabDetailedView, HUDcanvas.transform.position,
                                Quaternion.identity, HUDcanvas.transform);
    }

    //Továbbítja a kapott új kártya adatokat a megfelelő játékos oldalra, hogy vizuálisan megjelenítse azokat
    public void DisplayNewCard(Card data, int key, bool visible, bool blindDraw)
    {
        //Ha nem vakon húztuk
        if(!blindDraw)
        {
            GameObject temp = GetFieldFromKey(key).CreateCard(data, visible);

            //Kártya objektum hozzáadása a kéz mezőhöz
            GetFieldFromKey(key).AddCardToHand(temp);
        }

        else 
        {
            GameObject temp = GetFieldFromKey(key).CreateCard(data, false);
            GetFieldFromKey(key).BlindSummon(temp);
        }
    }

    //Frissíti a játékos paklijának méretét a modell szerinti aktuális értékre
    public void RefreshDeckSize(int key, int newDeckSize)
    {   
        GetFieldFromKey(key).UpdateDeckCounter(newDeckSize);
    }

    //Megjeleníti a kijelölt kártya részletes nézetét
    public void DisplayCardDetail(Card data, int displayedCardId, bool skillDecision, int position)
    {
        cardDetailWindow = Instantiate(cardWithDetails, HUDcanvas.transform.position, Quaternion.identity, HUDcanvas.transform);
        cardDetailWindow.GetComponent<CardDisplay_Controller>().SetupDisplay(data, skillDecision);
        //Ha a gombok is megjelennek, akkor be kell konfigurálni is őket
        if(skillDecision)
        {
            SetFixedDetails(true);
            //Eltároljuk, hogy melyik kulccsal rendelkező pozícióról jött a kérés
            int tempKey = playerFields.Keys.ElementAt(position);
            cardDetailWindow.GetComponent<CardDisplay_Controller>().SetupButtons(data, this, displayedCardId, tempKey);
        }
    }

    //Eltünteti a kártya részletes nézetet
    public void HideCardDetail()
    {
        Destroy(cardDetailWindow);

        //Ha fix megjelenítés volt akkor kapcsoljuk ki
        if(GetFixedDetailsStatus())
        {
            SetFixedDetails(false);
        }
    }

    //Megváltoztatja a harctípus szöveget az épp aktuális, játékvezérlő szerinti értékre
    public void ChangeStatText(string newType)
    {
        this.activeStatText.text = newType;
    }

    //Megjeleníti a játékosnak a harctípus választó ablakot
    public void DisplayStatBox()
    {
        activeStatPanel.SetActive(false);
        statPanel = Instantiate(statChoiceBox, HUDcanvas.transform.position, Quaternion.identity, HUDcanvas.transform);
        statPanel.GetComponent<StatChoice_Controller>().SetupPanel();
    }

    //A Harctípus választó ablak gombjainak függvénye
    public void ChooseStat(ActiveStat stat)
    {
        activeStatPanel.SetActive(true);
        controller.ReportStatChange(stat);
        Destroy(statPanel);
    }

    //A veezérlőnek ad jelentést játékos általi kártya idézésről
    public void ReportSummon(int handIndex, int position)
    {
        int playerKey = playerFields.Keys.ElementAt(position);
        controller.ReportSummon(handIndex, playerKey);
    }

    //A megadott oldalú játékosnak engedélyezi/tiltja a kártya mozgatás/húzás/lerakás interakciókat
    public void SetDragStatus(int key, bool status)
    {
        GetFieldFromKey(key).SetDraggableStatus(status);
    }

    //Botok UI-jának küld egy parancsot egy megadott indexű kártya lerakására/aktiválására
    public void SummonCard(int key, int index)
    {
        GetFieldFromKey(key).SummonCard(index);
    }

    //Felfedi a megadott oldalú játékos lerakott kártyáit
    public void RevealCards(int key)
    {
        GetFieldFromKey(key).RevealCards();
    }

    //A játékmezőn megjelenít üzeneteket a játékosok számára (pl kinek a köre van, kör eredménye)
    public bool DisplayMessage(string msg)
    {
        //Ha nincs még megjelenített üzenet
        if(!isMessageOnScreen)
        {
            messageText = Instantiate(prefabMessageText,playerFieldCanvas.transform.position,Quaternion.identity,playerFieldCanvas.transform);
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

    public void HideMessage()
    {
        if(isMessageOnScreen)
        {
            Destroy(messageText);
            isMessageOnScreen = false;
        }
    }

    public bool GetMessageStatus()
    {
        return this.isMessageOnScreen;
    }

    //Kör végén a kör eredményével parancsot küld a megfelelő játékos UI-nak, hogy rakja el a lerakott lapokat
    public void PutCardsAway(int key, bool isWinner)
    {
        GetFieldFromKey(key).PutCardsAway(isWinner);
    }

    public void SetSkillStatus(int key, bool status)
    {
        GetFieldFromKey(key).SetSkillStatus(status);
    }

    public void SetFixedDetails(bool status)
    {
        this.isDetailsFixed = status;
    }

    public bool GetFixedDetailsStatus()
    {
        return this.isDetailsFixed;
    }

    public void SetDetailsStatus(bool status)
    {
        this.isDetailsShowed = status;
    }

    public bool GetDetailsStatus()
    {
        return this.isDetailsShowed;
    }

    public void SkillAction(SkillState state, int key, int cardFieldPosition, int cardTypeID)
    {
        HideCardDetail();
        bool isItTheLast = GetFieldFromKey(key).SwitchCardSkillStatus(state, cardFieldPosition);
        controller.ReportSkillStatusChange(state,key,cardTypeID, isItTheLast);
    }

    public void NewSkillCycle(int key)
    {
        GetFieldFromKey(key).NewSkillCycle();
    }

}
