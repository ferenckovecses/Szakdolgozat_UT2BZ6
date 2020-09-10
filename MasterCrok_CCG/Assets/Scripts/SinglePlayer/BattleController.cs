using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public enum GameMainPhase {Preparation, SetOrder, DrawPhase, SetRoles, Turn, Reveal, Skill, Compare, Result, BlindMatch};
public enum ActiveStat {NotDecided, Power, Intelligence, Reflex};

public class BattleController : MonoBehaviour
{
    [Header("Prefabok és referenciák")]
    public BattleUI_Controller UI;
    public CardFactory factory;

    [Header("Státusz változók")]
    public GameMainPhase currentPhase;
    public ActiveStat currentStat;

    //Adattárolók
    private int numberOfPlayers;
    private GameData_Controller dataController;
    private Dictionary<int, Player_Slot> players;
    private List<int> playerKeys;
    private float drawTempo = 0.1f;

    //Játékbeli fázisok követői
    private bool phaseChange;
    private bool turnChange;
    private bool firstRound;
    private bool playerReacted;
    private bool isMessageOn;
    private bool blindMatch;
    private bool actionFinished;

    //RNG generátor
    private System.Random rng;

    // Start is called before the first frame update
    private void Awake()
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
    private void Update()
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
                case GameMainPhase.Reveal: StartCoroutine(Reveal()); break;
                case GameMainPhase.Skill: StartCoroutine(Skill()); break;
                case GameMainPhase.Compare: StartCoroutine(Compare()); break;
                case GameMainPhase.BlindMatch: StartCoroutine(BlindMatch()); break;
                case GameMainPhase.Result: Result(); break;
                default: break;
            }
        }
    }

    #region Main Game Phases

    private void Preparation()
    {
        AddPlayer();
        GenerateOpponents();
        GenerateUI();

        ChangePhase(GameMainPhase.SetOrder);
    }

    //A játékos listában felállít egy sorrendet, amit követve jönnek a következő körben
    private void SetOrder(int winnerKey = -1)
    {
        //A legelső körnél nincs szükség különleges sorrendre
        if(firstRound)
        {
            CreateRandomOrder();
            firstRound = false;

            ChangePhase(GameMainPhase.DrawPhase);
        }

        //A többi esetben az előző meccs nyertese lesz az új támadó, azaz az első a listában
        else 
        {
            CreateRandomOrder();

            //Valós érték vizsgálat
            if(winnerKey != -1)
            {
                PutWinnerInFirstPlace(winnerKey);
            }

            ChangePhase(GameMainPhase.SetRoles);
        }
    }

    //A kezdőkártyák felhúzása a kézbe
    private void DrawPhase()
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
    private void SetRoles()
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

        ChangePhase(GameMainPhase.Turn);
    }

    //A kör fázis
    //Részei: Húzás, [Harctípus választás], Idézés
    private IEnumerator Turn()
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

            //Csak akkor kell kézből idézni, ha nincs vakharc
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
                }  
            }

            //Átállítjuk a játékos státuszát: A képesség kiválasztására vár
            players[key].SetStatus(PlayerTurnStatus.ChooseSkill); 
        }

        ChangePhase(GameMainPhase.Reveal);
    }

    //Pályára rakott kártyák felfedése sorban
    private IEnumerator Reveal()
    {
        foreach (int key in playerKeys) 
        {
            RevealCards(key);
            yield return new WaitForSeconds(drawTempo * 5);
        }

        ChangePhase(GameMainPhase.Skill);
    }

    //Skill fázis: Körbemegyünk és mindenkit megkérdezünk, hogy mit akar a képességével kezdeni
    private IEnumerator Skill()
    {
        //Ciklusváltozó a while-hoz: Akkor lesz true, ha mindenki döntött a képességéről
        bool everyoneDecided = false;

        //Ha mindenki döntött, akkor break
        int playerMadeDecision = 0;

        //Addig megyünk, amíg mindenki el nem használja vagy passzolja a képességét
        while(!everyoneDecided)
        {

            foreach (int key in playerKeys) 
            {
                 //Ha a játékos státusza szerint a skill eldöntésére vár
                if(players[key].GetStatus() == PlayerTurnStatus.ChooseSkill)
                {

                    //Ha ember, akkor várunk a döntésre
                    if(players[key].GetPlayerStatus())
                    {
                        //Várunk a visszajelzésére
                        //yield return WaitForPlayerInput();
                        Pass(key);
                        playerMadeDecision++;
                    }

                    //Ellenkező esetben az AI dönt a képességről
                    else 
                    {
                        //A többi játékos pályán lévő kártyáinak adatait fetcheljük, mivel ezekről tudhat az AI
                        List<List<Card>> opponentCards = GetOpponentsCard(key);

                        //AI agy segítségét hívjuk a döntésben, átadjuk neki a szükséges infót és választ kapunk cserébe
                        SkillResponse response = Bot_Behaviour.ChooseSkill(players[key].GetCardsInHand(), players[key].GetCardsOnField(), opponentCards);


                        //Kis várakozás, gondolkodási idő imitálás a döntés meghozása előtt, hogy ne történjen minden túl hirtelen
                        yield return new WaitForSeconds(drawTempo * UnityEngine.Random.Range(10,20));

                        //Az AI döntése alapján itt végzi el a játék a megfelelő akciókat
                        switch (response) 
                        {
                            case SkillResponse.Pass: Pass(key);playerMadeDecision++; break;
                            case SkillResponse.Store: break;
                            case SkillResponse.Use: playerMadeDecision++; break;
                            default: Pass(key); playerMadeDecision++; break;
                        }
                    }
                }

                //Ha egyszerre mindenki volt, vagy már nem talált olyat, akinek a státusza 
                if(playerMadeDecision == numberOfPlayers)
                {
                    everyoneDecided = true;
                }
            }
        }

        ChangePhase(GameMainPhase.Compare);
    } 

    //Összehasonlítás fázis: Összehasonlítjuk az aktív kártyák értékét
    private IEnumerator Compare()
    {

        //Adunk időt az üzeneteknek, illetve a játékosoknak hogy megnézzék a pályát
        yield return new WaitForSeconds(drawTempo * 10);

        //Kiértékeléshez szükséges változók
        List<int> values = new List<int>();
        int max = 0;
        int maxId = -1;

        foreach (int key in playerKeys) 
        {
            //Hozzáadjuk minden mező értékét
            values.Add(players[key].GetActiveCardsValue(currentStat));


            //Eltároljuk a legnagyobb értéket menet közben
            if(players[key].GetActiveCardsValue(currentStat) > max)
            {
                max = players[key].GetActiveCardsValue(currentStat);
                maxId = key;
            }
        }

        //Ha több ember is rendelkezik a max value-val: Vakharc
        if(values.Count(p => p == max) > 1)
        {

            StartCoroutine(DisplayNotification("Döntetlen!\nVakharc következik!"));
            this.blindMatch = true;
            currentPhase = GameMainPhase.BlindMatch;
            phaseChange = true;
        }

        //Ellenkező esetben eldönthető, hogy ki a győztes
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

            yield return new WaitForSeconds(drawTempo * 5f);

            //Beállítjuk a győztes kulcsával az új sorrendet
            SetOrder(maxId);

        }

    }

    //Vakharc: Döntetlen esetén mindegyik lap vesztes lesz, majd lefordítva a pakli felső lapját teszik ki a pájára    
    private IEnumerator BlindMatch()
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
    private void Result()
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


    #region Commands

    private void ChangePhase(GameMainPhase nextPhase)
    {
        currentPhase = nextPhase;
        phaseChange = true;
    }

    //Felhúz a játékosok kezébe 4 lapot kezdésnél
    private IEnumerator DrawStarterCards()
    {
        for(var i = 0; i < 4; i++)
        {
            foreach (int key in playerKeys) 
            {
                DrawTheCard(key);
                yield return new WaitForSeconds(drawTempo);
            }
        }

        ChangePhase(GameMainPhase.SetRoles);
    }

    //Felhúz megadott számú lapot a megadott játékosnak
    private IEnumerator DrawCardsUp(int key, int amount = 1)
    {
        for(var i = 0; i < amount; i++)
        {
            DrawTheCard(key);
            yield return new WaitForSeconds(drawTempo);
        }
    }


    //A játékos referenciájának eltárolása
    private void AddPlayer()
    {
        int id = dataController.GetActivePlayer().GetUniqueID();
        players.Add(id, new Player_Slot(dataController.GetActivePlayer(),true));
        playerKeys.Add(id);
    }

    //Generál ellenfeleket
    private void GenerateOpponents()
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
    private void GenerateUI()
    {
        UI.CreatePlayerFields(numberOfPlayers, playerKeys, players[playerKeys[0]].GetDeckSize());
    }

    //Megkeveri a játékos pakliját
    private void ShuffleDeck(Player_Slot player, System.Random rng)
    {
        player.ShuffleDeck(rng);
    }

    //Kártya húzás: Modellből adatok lekérése és továbbítása a megfelelő játékos oldalnak.
    private void DrawTheCard(int key)
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

    private void SetDragStatus(int key, bool status)
    {
        UI.SetDragStatus(key, status);
    }

    //Kártya idézés: A megfelelő játékospanel felé továbbítjuk a megfelelő kártya kézbeli azonosítóját
    private void SummonCard(int key, int handIndex)
    {
        UI.SummonCard(key, handIndex);
    }

    //Felfedi a lerakott kártyát
    private void RevealCards(int key)
    {
        UI.RevealCards(key);
    }

    //Elrakatja a UI-on megjelenített kártyákat a megfelelő helyre
    private IEnumerator PutCardsAway(int key, bool isWinner)
    {
        yield return new WaitForSeconds(1f);
        UI.PutCardsAway(key, isWinner);
    }

    //Frissítést küld a játékpanelen az aktuális harctípus szövegére
    private void RefreshStatDisplay()
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
    private IEnumerator DisplayStatBox()
    {
        while(isMessageOn)
        {
            yield return null;
        }

        UI.DisplayStatBox();
    }

    //Értesítő üzenetet jelenít meg a UI felületen
    private IEnumerator DisplayNotification(string msg)
    {

        //Várakozunk a megjelenítéssel, amíg az előző üzenet eltűnik
        while(isMessageOn)
        {
            yield return null;
        }

        //Megjelenítjük az üzenetet
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

    private List<List<Card>> GetOpponentsCard(int currentPlayer)
    {
        List<List<Card>> activeCardsOnTheField = new List<List<Card>>();
        foreach (int key in playerKeys) 
        {
            if(key != currentPlayer)
            {
                activeCardsOnTheField.Add(players[key].GetCardsOnField());
            }
        }

        return activeCardsOnTheField;
    }

    private void Pass(int key)
    {
        StartCoroutine(DisplayNotification(players[key].GetUsername() + " passzolta a képességet!"));
        players[key].SetStatus(PlayerTurnStatus.Finished);
    }

    #endregion

    #region Incoming Messages

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

    #endregion

    #region Tools

    //Megkeveri a kulcsokat tartalmazó listát, ezzel a játékosok kezdő sorrendjét kialakítva
    private void CreateRandomOrder()
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
    private void PutWinnerInFirstPlace(int key)
    {
        playerKeys.Remove(key);
        playerKeys.Insert(0,key);
    }
    #endregion


}

