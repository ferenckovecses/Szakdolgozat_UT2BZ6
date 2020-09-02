using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public enum GameMainPhase {Preparation, SetOrder, DrawPhase, SetRoles, Turn, Compare, Result, BlindMatch};
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

    //Játékbeli státuszok
    bool isMessageOn;
    bool blindMatch;
    bool actionFinished;

    //Random generátor
    System.Random rng;


    // Start is called before the first frame update
    void Awake()
    {
        dataController = GameObject.Find("GameData_Controller").GetComponent<GameData_Controller>();
        playerKeys = new List<int>();
        players = new Dictionary<int,Player_Slot>();

        currentPhase = GameMainPhase.Preparation;
        currentStat = ActiveStat.NotDecided;

        rng = new System.Random();

        phaseChange = true;
        firstRound = true;
        isMessageOn = false;
        blindMatch = false;
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
                case GameMainPhase.BlindMatch: StartCoroutine(BlindMatch()); break;
                case GameMainPhase.Result: Result(); break;
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
    void SetOrder(int winnerKey = -1)
    {



        if(firstRound)
        {
            CreateRandomOrder();
            firstRound = false;
            currentPhase = GameMainPhase.DrawPhase;
        }

        else 
        {
            CreateRandomOrder();

            //Valós érték vizsgálat
            if(winnerKey != -1)
            {
                PutWinnerInFirstPlace(winnerKey);
            }

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
            ShuffleDeck(players[key], rng);
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
            //Ezekre csak akkor van szükség, ha nincs vakharc
            if(!this.blindMatch)
            {
                //Értesítést adunk a kör kezdetéről
                StartCoroutine(DisplayNotification(players[key].GetUsername() + " következik!"));

                //Húzunk fel a játékosnak 1 lapot a kör elején
                StartCoroutine(DrawCardsUp(key));
            }

            //Ha a játékos támadó, akkor döntenie kell a harctípusról
            if(players[key].GetRole() == PlayerTurnRole.Attacker)
            {
                //Ha ember, akkor dobjuk fel neki az értékválasztó képernyőt
                if(players[key].GetPlayerStatus())
                {
                    StartCoroutine(DisplayStatBox());

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

            //Csak akkor kell húzni, ha nincs vakharc
            if(!this.blindMatch)
            {
                //Ha a játékos státusza szerint a kártyahúzásra vár
                if(players[key].GetStatus() == PlayerTurnStatus.ChooseCard)
                {
                    SetDragStatus(key, true);
                    //Ha ember, akkor várunk az idézésre
                    if(players[key].GetPlayerStatus())
                    {
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
                        yield return new WaitForSeconds(drawTempo * UnityEngine.Random.Range(15,30));
                        SummonCard(key, index);
                    }

                    SetDragStatus(key, false);
                    players[key].SetStatus(PlayerTurnStatus.ChooseSkill);
                }   
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
        int maxId = -1;

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
                maxId = key;
            }

        }

        if(values.Count(p => p == max) > 1)
        {

            StartCoroutine(DisplayNotification("Döntetlen!\nVakharc következik!"));
            this.blindMatch = true;
            currentPhase = GameMainPhase.BlindMatch;
            phaseChange = true;
        }

        else
        {
            //Győzelmi üzenet megjelenítése
            StartCoroutine(DisplayNotification("A kör győztese: " + players[maxId].GetUsername()));

            //Ha vakharcok voltak, akkor ezzel végetértek
            if(this.blindMatch)
            {
                blindMatch = false;
            }

            foreach (int key in playerKeys) 
            {
                //Ha az első helyezettével megegyezik a kulcs
                if(key == maxId)
                {

                    players[key].SetResult(PlayerTurnResult.Win);

                    //Lapok elrakása a győztesek közé a UI-ban
                    StartCoroutine(PutCardsAway(key, true));
                }

                //Ellenkező esetben vesztes
                else
                {
                    players[key].SetResult(PlayerTurnResult.Lose);

                    //Lapok elrakása a vesztesek közé a UI-ban
                    StartCoroutine(PutCardsAway(key, false));
                }

                //Player modell frissítése, aktív mező elemeinek elrakása a győztes vagy vesztes tárolóba
                players[key].PutActiveCardAway();
            }

            //Beállítjuk a győztes kulcsával az új sorrendet
            SetOrder(maxId);

        }

    }

    //Vakharc: Döntetlen esetén mindegyik lap vesztes lesz, majd lefordítva a pakli felső lapját teszik ki a pájára    
    IEnumerator BlindMatch()
    {

        foreach (int key in playerKeys) 
        {
            //Az aktív kártyákat a vesztesek közé rakjuk
            players[key].SetResult(PlayerTurnResult.Draw);
            StartCoroutine(PutCardsAway(key, false));
            players[key].PutActiveCardAway();

            yield return new WaitForSeconds(1f);

            //A pakli felső lapját lerakjuk
            StartCoroutine(DrawCardsUp(key));

        }

        //Az utolsó védő lesz a következő támadó
        SetOrder(playerKeys[playerKeys.Count - 1]);

    }

    //Eredmény: Győztes hirdetés.
    void Result()
    {
        List<int> winAmounts = new List<int>();
        int maxWin = 0;
        int maxKey = -1;
        foreach (int key in playerKeys) 
        {
            if(players[key].GetWinAmount() > maxWin)
            {
                winAmounts.Add(players[key].GetWinAmount());
                maxWin = players[key].GetWinAmount();
                maxKey = key;
            }
        }

        if(winAmounts.Count(p => p == maxWin) > 1)
        {
            StartCoroutine(DisplayNotification("Játék Vége!\nAz eredmény döntetlen!"));
        }

        else 
        {
            StartCoroutine(DisplayNotification("Játék Vége!\nA győztes: " + players[maxKey].GetUsername()));       
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
        List<string> names = new List<string> {"Bot Ond", "Andrew Id", "Robot Gida"};
        numberOfPlayers = dataController.GetOpponents();
        for(var i = 0; i < numberOfPlayers; i++)
        {
            Player temp = new Player(names[i]);
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

    //Megkeveri a játékos pakliját
    void ShuffleDeck(Player_Slot player, System.Random rng)
    {
        player.ShuffleDeck(rng);
    }

    //Kártya húzás: Modellből adatok lekérése és továbbítása a megfelelő játékos oldalnak.
    void DrawTheCard(int key)
    {
        Card cardData = null; 

        if(!blindMatch)
        {
            cardData = players[key].DrawCard();
        }

        else 
        {
            cardData = players[key].BlindDraw();
        }

        //Ha a felhúzás sikeres
        if(cardData != null)
        {
            UI.DisplayNewCard(cardData, key, players[key].GetPlayerStatus(), this.blindMatch);

            //Ha a játékosról van szó továbbítjuk megjelenítésre a deck új méretét
            if(players[key].GetPlayerStatus())
            {
                int remainingDeckSize = players[key].GetDeckSize();
                UI.RefreshDeckSize(key, remainingDeckSize);
            }
        }

        //Ha húzáskor kifogyunk a lapból: Játék vége, kiértékeljük a játékosok teljesítményét
        else 
        {
            currentPhase = GameMainPhase.Result;
            phaseChange = true;
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

    //Felfedi a lerakott kártyát
    void RevealCards(int key)
    {
        UI.RevealCards(key);
    }

    //Elrakatja a UI-on megjelenített kártyákat a megfelelő helyre
    IEnumerator PutCardsAway(int key, bool isWinner)
    {
        yield return new WaitForSeconds(1f);
        UI.PutCardsAway(key, isWinner);
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
    IEnumerator DisplayStatBox()
    {
        while(isMessageOn)
        {
            yield return null;
        }

        UI.DisplayStatBox();
    }

    //Értesítő üzenetet jelenít meg a UI felületen
    IEnumerator DisplayNotification(string msg)
    {
        //Megjelenítjük
        UI.DisplayMessage(msg);
        isMessageOn = true;

        //Adunk időt a játékosoknak, hogy elolvassák
        yield return new WaitForSeconds(1f);

        //Eltüntetjük az üzenetet
        UI.HideMessage();
        isMessageOn = false;
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

    #region Tools

    //Megkeveri a kulcsokat tartalmazó listát, ezzel a játékosok kezdő sorrendjét kialakítva
    void CreateRandomOrder()
    {
        int n = playerKeys.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);
            int tmp = playerKeys[k];
            playerKeys[k] = playerKeys[n];
            playerKeys[n] = tmp;  
        }  
    }

    //A sorrend/kulcs lista elejére helyezi a győztest
    void PutWinnerInFirstPlace(int key)
    {
        playerKeys.Remove(key);
        playerKeys.Insert(0,key);
    }



    #endregion


}
