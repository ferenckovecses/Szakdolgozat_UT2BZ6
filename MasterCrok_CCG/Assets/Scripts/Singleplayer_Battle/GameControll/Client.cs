using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using GameControll;

namespace ClientControll
{
    public class Client : MonoBehaviour
    {
        [Header("Prefabok")]
        public GameObject prefabPlayerUI;
        public GameObject prefabExitPanel;
        public GameObject cardWithDetails;
        public GameObject statChoiceBox;
        public GameObject prefabMessageText;
        public TMP_Text deckSize;
        public GameObject cardListPrefab;
        public GameObject cardOrganizePrefab;
        public GameObject aiAgent;

        [Header("Referenciák")]
        public List<GameObject> positions;
        public GameObject playerFieldCanvas;
        public GameObject HUDcanvas;
        public GameObject notificationCanvas;
        public GameObject ingamePanelCanvas;
        public TMP_Text activeStatText;
        public GameObject activeStatPanel;
        public Button endTurnButton;

        //Adattárolók
        private Dictionary<int, GameObject> playerFields;
        private Input_Controller inputModule;
        private bool isMessageOnScreen;
        private bool isDetailsShowed;
        private bool isDetailsFixed;
        private bool isExitPanelExist;
        private bool isCardListActive;
        private GameObject cardDetailWindow;
        private GameObject statPanel;
        private GameObject messageText;
        private GameObject cardList;
        private ClientStates currentState;
        private int keyOfTarget;

        //Létrehozáskor fut le
        private void Awake()
        {
            SetTurnButtonStatus(false);
            playerFields = new Dictionary<int, GameObject>();
            isMessageOnScreen = false;
            isDetailsShowed = false;
            isDetailsFixed = false;
            isExitPanelExist = false;
            isCardListActive = false;
        }

        #region Report

        public void ReportStatChange(CardStatType stat)
        {
            inputModule.ReportStatChange(stat);
        }

        //A veezérlőnek ad jelentést játékos általi kártya idézésről
        public void ReportSummon(int handIndex, int position)
        {
            inputModule.ReportSummon(handIndex);
        }

        //Jelez a vezérlőnek a felhasználó döntéséről a megadott kártyát illetően, Gombok használják
        public void ReportSkillDecision(SkillState state, int key, int cardFieldPosition)
        {
            HideCardDetailsWindow();
            GetFieldFromKey(key).SwitchCardSkillStatus(state, cardFieldPosition);

            inputModule.ReportSkillStatusChange(state, cardFieldPosition);
        }
        //A vezérlőnek jelzünk a kilépésről, ő pedig ellátja a szükséges feladatokat
        public void ReportPlayerExit()
        {
            inputModule.ReportExit();
        }

        public void ReportCardSelection(int id)
        {
            Destroy(cardList);
            isCardListActive = false;
            inputModule.HandleCardSelection(id, keyOfTarget);
        }

        public void ReportSelectionCancel()
        {
            Destroy(cardList);
            isCardListActive = false;
            inputModule.ReportSelectionCancel();
        }

        public void ReportDisplayRequest(CardListTarget listType, int positionID)
        {
            inputModule.ReportDisplayRequest(listType, playerFields.Keys.ElementAt(positionID));
        }

        public void ReportNameBoxTapping(int positionID)
        {
            inputModule.ReportNameBoxTapping(playerFields.Keys.ElementAt(positionID));
        }

        public void ReportEndOfTurn()
        {
            if(!isMessageOnScreen)
            {

                if(GetCardDetailWindowStatus())
                {
                    HideCardDetailsWindow();
                }

                inputModule.ReportTurnEnd();
                SetTurnButtonStatus(false);
            }
        }

        public void ReportEndOfAction()
        {
            inputModule.ReportActionEnd();
        }

        public void ReportOrderChange(List<Card> cards)
        {
            Destroy(cardList);
            isCardListActive = false;
            inputModule.HandleChangeOfCardOrder(cards, keyOfTarget);
        }

        #endregion

        #region Common Instruction

        //Elhelyezi a megadott számú játékosnak megfelelő pályaelemet a megadott pozíciókba
        public void CreatePlayerFields(int playerAmount, List<int> playerKeys, List<string> playerNames, int displayedDeckSize)
        {
            for (var i = 0; i < playerAmount + 1; i++)
            {
                //Létrehozás
                GameObject temp = Instantiate(prefabPlayerUI, positions[i].transform.position, positions[i].transform.rotation, positions[i].transform);

                //Eltárolás későbbi referenciához
                playerFields.Add(playerKeys[i], temp);

                //Kliens referencia eltárolása
                GetFieldFromKey(playerKeys[i]).SetClient(this);
                GetFieldFromKey(playerKeys[i]).SetName(playerNames[i]);

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
                    GameObject ai_agent = Instantiate(aiAgent, temp.transform.position, Quaternion.identity, temp.transform);
                    ai_agent.GetComponent<AI>().AddKey(playerKeys[i]);
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
        //Továbbítja a kapott új kártya adatokat a megfelelő játékos oldalra, hogy vizuálisan megjelenítse azokat
        public void DisplayNewCard(Card data, int playerKey, bool visibleForThePlayer, DrawType drawType, DrawTarget target, SkillState skillState)
        {
            //Ha nem vakon húztuk
            if (drawType == DrawType.Normal)
            {
                //Létrehozás
                GameObject temp = GetFieldFromKey(playerKey).CreateCard(data, visibleForThePlayer, skillState);

                //Elhelyezés kézbe
                if (target == DrawTarget.Hand)
                {
                    //Kártya objektum hozzáadása a kéz mezőhöz
                    GetFieldFromKey(playerKey).AddCardToHand(temp);
                }

                //Elhelyezés a mezőre
                else
                {
                    GetFieldFromKey(playerKey).BlindSummon(temp);
                    GetFieldFromKey(playerKey).RevealCardsOnField();
                }
            }

            //Vakon húzás a mezőre
            else if(drawType == DrawType.Blind)
            {
                if(target == DrawTarget.Field)
                {
                    GameObject temp = GetFieldFromKey(playerKey).CreateCard(data, false, skillState);
                    GetFieldFromKey(playerKey).BlindSummon(temp);
                }
            }


        }

        #endregion

        public void SetTurnButtonStatus(bool newValue)
        {
            //endTurnButton.interactable = newValue;
            endTurnButton.gameObject.SetActive(newValue);
        }

        public void SetClientStates(ClientStates newState)
        {
            this.currentState = newState;
        }

        //Játék folytatása
        public void ContinueBattle(GameObject exitPanel)
        {
            Destroy(exitPanel);
            isExitPanelExist = false;
        }

        #region Player Specific Methods

        //A Harctípus választó ablak gombjainak függvénye
        public void ChooseStatButton(CardStatType stat)
        {
            activeStatPanel.SetActive(true);
            Destroy(statPanel);
            ReportStatChange(stat);
        }

        //HUD exit gomb
        public void ExitBattleButton()
        {
            if (!isExitPanelExist)
            {
                isExitPanelExist = true;
                //Megjeleníti a kérdőablakot
                GameObject exitPanel = Instantiate(prefabExitPanel);
                //If yes: ExitBattle()
                Button yesBtn = GameObject.Find("YesButton").GetComponent<Button>();
                yesBtn.onClick.AddListener(delegate { ReportPlayerExit(); });
                //If no: Destroy window, continue game
                Button noBtn = GameObject.Find("NoButton").GetComponent<Button>();
                noBtn.onClick.AddListener(delegate { ContinueBattle(exitPanel); });
            }
        }

        //Megjeleníti a kijelölt kártya részletes nézetét
        public void DisplayCardDetaislWindow(Card data, int displayedCardId, bool skillDecision, int position)
        {
            cardDetailWindow = Instantiate(cardWithDetails);
            cardDetailWindow.GetComponent<DetailedCardDisplay>().SetupDisplay(data, skillDecision);
            isDetailsShowed = true;
            //Ha a gombok is megjelennek, akkor be kell konfigurálni is őket
            if (skillDecision)
            {
                SetFixedDetails(true);
                //Eltároljuk, hogy melyik kulccsal rendelkező pozícióról jött a kérés
                int tempKey = playerFields.Keys.ElementAt(position);
                cardDetailWindow.GetComponent<DetailedCardDisplay>().SetupButtons(data, this, displayedCardId, tempKey);
            }
        }

        //Eltünteti a kártya részletes nézetet
        public void HideCardDetailsWindow()
        {
            Destroy(cardDetailWindow);
            isDetailsShowed = false;
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
            statPanel.GetComponent<CardStatSelectorDisplay>().SetupPanel(this);
        }

        public void DisplayListOfCards(List<Card> cards, SkillEffectAction action, int key, string msg)
        {
            keyOfTarget = key;

            //Ha sorrend változtatásról van szó
            if(action == SkillEffectAction.Reorganize)
            {
                
                cardList = Instantiate(cardOrganizePrefab, ingamePanelCanvas.transform.position, Quaternion.identity, ingamePanelCanvas.transform);
                cardList.GetComponent<CardOrganizeDisplay>().SetupScreen(this, msg);
                foreach (Card card in cards) 
                {
                    cardList.GetComponent<CardOrganizeDisplay>().AddCard(card);
                }
                
            }

            //Ha egy lap kiválasztásáról van szó
            else 
            {
                cardList = Instantiate(cardListPrefab, ingamePanelCanvas.transform.position, Quaternion.identity, ingamePanelCanvas.transform);
                cardList.GetComponent<CardListDisplay>().SetupList(this, cards, action, msg);
            }

            isCardListActive = true;
        }

        public void CloseCardList()
        {
            Destroy(cardList);
            isCardListActive = false;
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
        public Player_UI GetFieldFromKey(int key)
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

        public void SetReportAgent(Input_Controller rep)
        {
            this.inputModule = rep;
        }

        public bool GetCardListStatus()
        {
            return this.isCardListActive;
        }

        #endregion

    }
}