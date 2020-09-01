using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public enum GameMainPhase {Preparation, SetOrder, DrawPhase, SetRoles, Turn, Compare, Result};
public enum ActiveStat {NotDecided, Power, Intelligence, Reflex};

public class BattleController : MonoBehaviour
{
    [Header("Adattárolók")]
    int numberOfPlayers;
    GameData_Controller dataController;
    Dictionary<int, Player_Slot> players;
    List<int> playerKeys;
    public float drawTempo = 0.1f;

    [Header("Prefabok és referenciák")]
    public BattleUI_Controller UI;
    public CardFactory factory;

    [Header("Státusz változók")]
    public GameMainPhase currentPhase;
    public ActiveStat currentStat;

    [Header("Játék fázis követők")]
    bool phaseChange;
    bool turnChange;
    bool firstRound;

    //A játékosnak a reakcióit jelzi
    bool playerReacted;


    // Start is called before the first frame update
    void Awake()
    {
        dataController = GameObject.Find("GameData_Controller").GetComponent<GameData_Controller>();
        playerKeys = new List<int>();
        players = new Dictionary<int,Player_Slot>();

        currentPhase = GameMainPhase.Preparation;
        currentStat = ActiveStat.NotDecided;

        phaseChange = true;
        firstRound = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Játék fő fázisainak változásai
        if(phaseChange)
        {
            phaseChange = false;
            switch (currentPhase) 
            {
                case GameMainPhase.Preparation: Preparation(); break;
                case GameMainPhase.SetOrder: SetOrder(); break;
                case GameMainPhase.DrawPhase: DrawPhase(); break;
                case GameMainPhase.SetRoles: SetRoles(); break;
                case GameMainPhase.Turn: StartCoroutine(Turn()); break;
                case GameMainPhase.Compare: StartCoroutine(Compare()); break;
                default: break;
            }
        }


    }

    #region Main Game Phases

    void Preparation()
    {
        AddPlayer();
        GenerateOpponents();
        GenerateUI();

        currentPhase = GameMainPhase.SetOrder;
        phaseChange = true;
    }

    //A játékos listában felállít egy sorrendet, amit követve jönnek a következő körben
    void SetOrder()
    {
        if(firstRound)
        {
            CreateRandomOrder();
            firstRound = false;
            currentPhase = GameMainPhase.DrawPhase;
        }

        else 
        {
            //Check for order modifier effects
            //Change order
            currentPhase = GameMainPhase.SetRoles;
        }

        phaseChange = true;
    }

    //A kezdőkártyák felhúzása a kézbe
    void DrawPhase()
    {
        //Paklik megkeverése
        foreach (int key in playerKeys) 
        {
            ShuffleDeck(players[key]);
        }

        //Kezdő 4 lap felhúzása
        StartCoroutine(DrawStarterCards());
    }

    //Beállítjuk a Támadó-Védő szerepeket
    //Mindig a player lista első eleme a Támadó, a többiek védők
    void SetRoles()
    {
        var i = 0;
        foreach (int key in playerKeys) 
        {
            if(i == 0)
            {
                players[key].SetRole(PlayerTurnRole.Attacker);
            }

            else 
            {
                players[key].SetRole(PlayerTurnRole.Defender);   
            }

            players[key].SetStatus(PlayerTurnStatus.ChooseCard);
            i += 1;
        }

        currentPhase = GameMainPhase.Turn;
        phaseChange = true;
    }

    //A kör fázis
    //Részei: Húzás, [Harctípus választás], Idézés
    IEnumerator Turn()
    {
        //Minden játékoson végigmegy
        foreach (int key in playerKeys) 
        {

            Debug.Log(players[key].GetUsername() + "'s turn!");

            //Húzunk fel a játékosnak 1 lapot a kör elején
            StartCoroutine(DrawCardsUp(key));

            //Ha a játékos támadó, akkor döntenie kell a harctípusról
            if(players[key].GetRole() == PlayerTurnRole.Attacker)
            {
                //Ha ember, akkor dobjuk fel neki az értékválasztó képernyőt
                if(players[key].GetPlayerStatus())
                {
                    DisplayStatBox();
                    //Várunk a visszajelzésére
                    yield return WaitForPlayerInput();
                }

                //Ellenkező esetben az AI dönt
                else 
                {
                    //AI agy segítségét hívjuk a döntésben
                    currentStat = Bot_Behaviour.ChooseFightType(players[key].GetCardsInHand());

                    //Jelenítsük meg a változást
                    RefreshStatDisplay();
                }
            }

            //Ha a játékos státusza szerint a kártyahúzásra vár
            if(players[key].GetStatus() == PlayerTurnStatus.ChooseCard)
            {
                SetDragStatus(key, true);
                //Ha ember, akkor várunk az idézésre
                if(players[key].GetPlayerStatus())
                {
                    Debug.Log("Waiting For The Player To Summon!");
                    //Várunk a visszajelzésére
                    yield return WaitForPlayerInput();
                    SetDragStatus(key, false);
                }

                //Ellenkező esetben az AI-al rakatunk le kártyát
                else 
                {
                    //AI agy segítségét hívjuk a döntésben
                    int index = Bot_Behaviour.ChooseRightCard(players[key].GetCardsInHand(), currentStat);

                    players[key].PlayCardFromHand(index);
                    SummonCard(key, index);
                    yield return new WaitForSeconds(drawTempo * UnityEngine.Random.Range(15,30));
                }

                SetDragStatus(key, false);
                players[key].SetStatus(PlayerTurnStatus.ChooseSkill);
            }


        }

        //Továbblépés a következő fázisra
        currentPhase = GameMainPhase.Compare;
        phaseChange = true;
    }

    //Összehasonlítás fázis: Felfedjük a kirakott lapokat és összehasonlítjuk értékeiket
    IEnumerator Compare()
    {

        List<int> values = new List<int>();
        int max = 0;
        //Felfedés
        foreach (int key in playerKeys) 
        {
            RevealCards(key);
            yield return new WaitForSeconds(drawTempo * 5);

            //Hozzáadjuk minden mező értékét
            values.Add(players[key].GetActiveCardsValue(currentStat));

            //Eltároljuk a legnagyobb értéket menet közben
            if(players[key].GetActiveCardsValue(currentStat) > max)
            {
                max = players[key].GetActiveCardsValue(currentStat);
            }

        }

        if(values.Count(p => p == max) > 1)
        {
            Debug.Log("Döntetlen!");
        }

        else
        {
            int index = values.IndexOf(max);
            Debug.Log("A kör győztese: " + players.Values.ElementAt(index).GetUsername()); 
        }





    }



    #endregion


    #region Interactions

    //Felhúz a játékosok kezébe 4 lapot kezdésnél
    IEnumerator DrawStarterCards()
    {
        for(var i = 0; i < 4; i++)
        {
            foreach (int key in playerKeys) 
            {
                DrawTheCard(key);
                yield return new WaitForSeconds(drawTempo);
            }
        }

        currentPhase = GameMainPhase.SetRoles;
        phaseChange = true;
    }

    //Felhúz megadott számú lapot a megadott játékosnak
    IEnumerator DrawCardsUp(int key, int amount = 1)
    {
        for(var i = 0; i < amount; i++)
        {
            DrawTheCard(key);
            yield return new WaitForSeconds(drawTempo);
        }
    }


    //A játékos referenciájának eltárolása
    void AddPlayer()
    {
        int id = dataController.GetActivePlayer().GetUniqueID();
        players.Add(id, new Player_Slot(dataController.GetActivePlayer(),true));
        playerKeys.Add(id);
    }

    //Generál ellenfeleket
    void GenerateOpponents()
    {
        numberOfPlayers = dataController.GetOpponents();
        for(var i = 0; i < numberOfPlayers; i++)
        {
            Player temp = new Player("#Bot" +(i+1).ToString());
            temp.SetUniqueID(i);
            temp.AddActiveDeck(factory.GetStarterDeck());
            Player_Slot bot = new Player_Slot(temp);
            players.Add(i, bot);
            playerKeys.Add(i);
        }
    }

    //Legenerálja a UI felületet
    void GenerateUI()
    {
        UI.CreatePlayerFields(numberOfPlayers, playerKeys, players[playerKeys[0]].GetDeckSize());
    }

    //Megkeveri a kulcsokat tartalmazó listát, ezzel a játékosok kezdő sorrendjét kialakítva
    void CreateRandomOrder()
    {
        System.Random rng = new System.Random();

        int n = playerKeys.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);
            int tmp = playerKeys[k];
            playerKeys[k] = playerKeys[n];
            playerKeys[n] = tmp;  
        }  
    }

    //Megkeveri a játékos pakliját
    void ShuffleDeck(Player_Slot player)
    {
        player.ShuffleDeck();
    }

    //Kártya húzás: Modellből adatok lekérése és továbbítása a megfelelő játékos oldalnak.
    void DrawTheCard(int key)
    {
        Card cardData = players[key].DrawCard();

        if(cardData != null)
        {
            UI.DisplayNewCard(cardData, key, players[key].GetPlayerStatus());
        }

        //Ha a játékosról van szó továbbítjuk megjelenítésre a deck új méretét
        if(players[key].GetPlayerStatus())
        {
            int remainingDeckSize = players[key].GetDeckSize();
            UI.RefreshDeckSize(key, remainingDeckSize);
        }
    }

    void SetDragStatus(int key, bool status)
    {
        UI.SetDragStatus(key, status);
    }

    //Kártya idézés: A megfelelő játékospanel felé továbbítjuk a megfelelő kártya kézbeli azonosítóját
    void SummonCard(int key, int handIndex)
    {
        UI.SummonCard(key, handIndex);
    }

    void RevealCards(int key)
    {
        UI.RevealCards(key);
    }

    //Visszapakolja a kártyákat a játékosok paklijába
    public void ResetDecks()
    {
        foreach (int keys in playerKeys) 
        {
            if(players[keys].GetPlayerStatus())
            {
                players[keys].PutEverythingBack();
            }
        }
    }

    //Frissítést küld a játékpanelen az aktuális harctípus szövegére
    void RefreshStatDisplay()
    {
        string statText;

        switch (currentStat) 
        {
            case ActiveStat.Power: statText = "Erő"; break;
            case ActiveStat.Intelligence: statText = "Intelligencia"; break;
            case ActiveStat.Reflex: statText = "Reflex"; break;
            default: statText = "Harctípus"; break;
        }

        UI.ChangeStatText(statText);
    }

    //Megjeleníti a játékos számára a harctípus választó képernyőt
    void DisplayStatBox()
    {
        UI.DisplayStatBox();
    }

    //Várakozás, amíg a játékos nem ad inputot
    private IEnumerator WaitForPlayerInput()
    {
        playerReacted = false;
        while(!playerReacted)
        {
            yield return null;
        }
    }

    #endregion

    #region Report

    //A játékos által kiválasztott harctípus beállítása
    public void ReportStatChange(ActiveStat newStat)
    {   
        //Kitörés a wait fázisból
        playerReacted = true;

        //Választott érték beállítása
        this.currentStat = newStat;

        //Jelenítsük meg a változást
        RefreshStatDisplay();

    }

    //Játékos idézéséről a visszajelzés
    public void ReportSummon(int indexInHand, int playerKey)
    {
        playerReacted = true;
        players[playerKey].PlayCardFromHand(indexInHand);
    }

    #endregion


}
