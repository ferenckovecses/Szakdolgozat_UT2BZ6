using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.SinglePlayer;

public enum GameMainPhase {Preparation, SetOrder, DrawPhase, SetRoles, Summon, 
    Reveal, Skill, Compare, Result, BlindMatch, Awards};

public enum ActiveStat {NotDecided, Power, Intelligence, Reflex};


public class SinglePlayer_Controller : MonoBehaviour
{
    [Header("Prefabok és referenciák")]
    public CardFactory factory;
    public SkillFactory skills;

    //Adattárolók
    private float drawTempo = 0.1f;

    //Vezérlő modulok
    private SinglePlayer_Data dataModule;
    private SinglePlayer_Report reportModule;
    private SinglePlayer_Commander commandModule;
    public SinglePlayer_Client client;

    //Játékbeli fázisok követői
    private bool phaseChange;
    private bool firstRound;
    private bool playerReacted;
    private bool displayedMessageStatus;
    private bool blindMatch;
    private int storeCount;
    private bool finishedWithTurn;
    private int currentKey;

    private GameMainPhase currentPhase;
    private ActiveStat currentStat;

    //Játék győzelmi paraméterei
    public static int winsNeeded = 5;


    //RNG generátor
    private System.Random rng;

    // Start is called before the first frame update
    private void Awake()
    {
        dataModule = new SinglePlayer_Data(this, factory);
        reportModule = new SinglePlayer_Report(this);
        commandModule = new SinglePlayer_Commander(this, client, reportModule);
        skills = new SkillFactory(this);

        currentPhase = GameMainPhase.Preparation;
        currentStat = ActiveStat.NotDecided;

        rng = new System.Random();

        phaseChange = true;
        firstRound = true;
        displayedMessageStatus = false;
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
                case GameMainPhase.Summon: StartCoroutine(Summon()); break;
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
        dataModule.AddPlayer();
        dataModule.GenerateOpponents();
        commandModule.GenerateUI(dataModule.GetNumberOfOpponents(), dataModule.GetKeyList(), dataModule.GetPlayerAtIndex(0).GetDeckSize());
        ChangePhase(GameMainPhase.SetOrder);
    }

    //A játékos listában felállít egy sorrendet, amit követve jönnek a következő körben
    private void SetOrder(int winnerKey = -1)
    {
        dataModule.CreateRandomOrder();

        //A legelső körnél nincs szükség különleges sorrendre
        if (firstRound)
        {
            firstRound = false;
            ChangePhase(GameMainPhase.DrawPhase);
        }

        //A többi esetben az előző meccs nyertese lesz az új támadó, azaz az első a listában
        else 
        {
            //Valós érték vizsgálat
            if(winnerKey != -1)
            {
                dataModule.PutWinnerInFirstPlace(winnerKey);
            }

            ChangePhase(GameMainPhase.SetRoles);
        }
    }

    //A kezdőkártyák felhúzása a kézbe
    private void DrawPhase()
    {
        //Paklik megkeverése
        foreach (int key in dataModule.GetKeyList()) 
        {
            dataModule.ShuffleDeck(key);
        }

        //Kezdő 4 lap felhúzása
        StartCoroutine(DrawStarterCards());
    }

    //Beállítjuk a Támadó-Védő szerepeket
    //Mindig a player lista első eleme a Támadó, a többiek védők
    private void SetRoles()
    {
        bool firstPlayer = true;
        foreach (int key in dataModule.GetKeyList()) 
        {
            if(firstPlayer)
            {
                firstPlayer = false;
                dataModule.GetPlayerWithKey(key).SetRole(PlayerTurnRole.Attacker);
            }

            else 
            {
                dataModule.GetPlayerWithKey(key).SetRole(PlayerTurnRole.Defender);   
            }

            dataModule.GetPlayerWithKey(key).SetStatus(PlayerTurnStatus.ChooseCard);
        }

        ChangePhase(GameMainPhase.Summon);
    }


    //Idézés fázis
    private IEnumerator Summon()
    {
        //Minden játékoson végigmegy
        foreach (int playerKey in dataModule.GetKeyList()) 
        {
            currentKey = playerKey;

            //Ezekre csak akkor van szükség, ha nincs vakharc
            if(!this.blindMatch)
            {
                //Értesítést adunk a kör kezdetéről
                StartCoroutine(commandModule.DisplayNotification(dataModule.GetPlayerWithKey(playerKey).GetUsername() + " következik!"));

                //Húzunk fel a játékosnak 1 lapot a kör elején
                StartCoroutine(DrawCardsUp(playerKey));
            }

            //Ha a játékos támadó, akkor döntenie kell a harctípusról
            if(dataModule.GetPlayerWithKey(playerKey).GetRole() == PlayerTurnRole.Attacker)
            {
                //Ha a játékosnak már nincs idézhető lapja: játék vége
                if(dataModule.GetPlayerWithKey(playerKey).GetHandCount() == 0)
                {
                    StartCoroutine(commandModule.DisplayNotification(dataModule.GetPlayerWithKey(playerKey).GetUsername() +" kifogyott a lapokból!\nJáték vége!"));
                    currentPhase = GameMainPhase.Result;
                    phaseChange = true;
                }

                //Ha ember, akkor dobjuk fel neki az értékválasztó képernyőt
                if(dataModule.GetPlayerWithKey(playerKey).GetPlayerStatus())
                {
                    StartCoroutine(commandModule.DisplayStatBox());

                    //Várunk a visszajelzésére
                    yield return WaitForPlayerInput();
                }

                //Ellenkező esetben az AI dönt
                else 
                {
                    //AI agy segítségét hívjuk a döntésben
                    currentStat = Bot_Behaviour.ChooseFightType(dataModule.GetPlayerWithKey(playerKey).GetCardsInHand());

                    //Jelenítsük meg a változást
                    commandModule.RefreshStatDisplay();
                }
            }

            //Csak akkor kell kézből idézni, ha nincs vakharc
            if(!this.blindMatch)
            {
                //Ha a játékos státusza szerint a kártyahúzásra vár
                if(dataModule.GetPlayerWithKey(playerKey).GetStatus() == PlayerTurnStatus.ChooseCard)
                {
                    commandModule.SetDragStatus(playerKey, true);
                    //Ha ember, akkor várunk az idézésre
                    if(dataModule.GetPlayerWithKey(playerKey).GetPlayerStatus())
                    {
                        //Várunk a visszajelzésére
                        yield return WaitForPlayerInput();
                        commandModule.SetDragStatus(playerKey, false);
                    }

                    //Ellenkező esetben az AI-al rakatunk le kártyát
                    else 
                    {
                        //AI agy segítségét hívjuk a döntésben
                        int index = Bot_Behaviour.ChooseRightCard(dataModule.GetPlayerWithKey(playerKey).GetCardsInHand(), currentStat);

                        dataModule.GetPlayerWithKey(playerKey).PlayCardFromHand(index);
                        yield return new WaitForSeconds(drawTempo * UnityEngine.Random.Range(15,30));
                        commandModule.SummonCard(playerKey, index);
                    }

                    commandModule.SetDragStatus(playerKey, false);
                }  
            }

            //Átállítjuk a játékos státuszát: A képesség kiválasztására vár
            dataModule.GetPlayerWithKey(playerKey).SetStatus(PlayerTurnStatus.ChooseSkill); 
        }
        ChangePhase(GameMainPhase.Reveal);
    }

    //Pályára rakott kártyák felfedése sorban
    private IEnumerator Reveal()
    {
        foreach (int key in dataModule.GetKeyList()) 
        {
            commandModule.RevealCards(key);
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
        int playerMadeDecision;

        //Addig megyünk, amíg mindenki el nem használja vagy passzolja a képességét
        while(!everyoneDecided)
        {
            playerMadeDecision = 0;
            foreach (int playerKey in dataModule.GetKeyList()) 
            {
                currentKey = playerKey;

                NewSkillCycle(playerKey);
                 //Ha a játékos státusza szerint a skill eldöntésére vár
                if(dataModule.GetPlayerWithKey(playerKey).GetStatus() == PlayerTurnStatus.ChooseSkill)
                {

                    //Ha ember, akkor várunk a döntésre
                    if(dataModule.GetPlayerWithKey(playerKey).GetPlayerStatus())
                    {
                        //Jelezzük a játékosnak, hogy itt az idő dönteni a képességről
                        StartCoroutine(commandModule.DisplayNotification("Dönts a képességekről!"));

                        //Megadjuk a lehetőséget a játékosnak, hogy döntsön a képességekről most
                        commandModule.SetSkillStatus(playerKey, true);

                        //Várunk a visszajelzésére
                        yield return WaitForPlayerInput();

                        //Ha döntött, akkor elvesszük tőle a lehetőséget, hogy módosítson rajta
                        commandModule.SetSkillStatus(playerKey, false);
                    }

                    //Ellenkező esetben az AI dönt a képességről
                    else 
                    {
                        //A többi játékos pályán lévő kártyáinak adatait fetcheljük, mivel ezekről tudhat az AI
                        List<List<Card>> opponentCards = dataModule.GetOpponentsCard(playerKey);

                        //AI agy segítségét hívjuk a döntésben, átadjuk neki a szükséges infót és választ kapunk cserébe
                        SkillResponse response = Bot_Behaviour.ChooseSkill(dataModule.GetPlayerWithKey(playerKey).GetCardsInHand(), dataModule.GetPlayerWithKey(playerKey).GetCardsOnField(), opponentCards);


                        //Kis várakozás, gondolkodási idő imitálás a döntés meghozása előtt, hogy ne történjen minden túl hirtelen
                        yield return new WaitForSeconds(drawTempo * UnityEngine.Random.Range(5,15));

                        //Az AI döntése alapján itt végzi el a játék a megfelelő akciókat
                        switch (response) 
                        {
                            case SkillResponse.Pass: Pass(); break;
                            case SkillResponse.Store: break;
                            case SkillResponse.Use: break;
                            default: Pass(); break;
                        }
                        
                        //TODO: A fenti döntés kártyánként szülessen meg

                        //Az AI végzett a körével a döntés után
                        dataModule.GetPlayerWithKey(playerKey).SetStatus(PlayerTurnStatus.Finished); 
                    }
                }

                //Ha az aktuális játékos végzett, növeljük a countert
                if(dataModule.GetPlayerWithKey(playerKey).GetStatus() == PlayerTurnStatus.Finished) {
                    playerMadeDecision++;
                }

            }
            //Ha mindenki végzett, akkor loop vége
            if(playerMadeDecision == dataModule.GetNumberOfOpponents() + 1)
            {
                everyoneDecided = true;
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

        foreach (int key in dataModule.GetKeyList()) 
        {

            //Hozzáadjuk minden mező értékét
            values.Add(dataModule.GetPlayerWithKey(key).GetActiveCardsValue(currentStat));


            //Eltároljuk a legnagyobb értéket menet közben
            if(dataModule.GetPlayerWithKey(key).GetActiveCardsValue(currentStat) > max)
            {
                max = dataModule.GetPlayerWithKey(key).GetActiveCardsValue(currentStat);
                maxId = key;
            }
        }

        //Ha több ember is rendelkezik a max value-val: Vakharc
        if(values.Count(p => p == max) > 1)
        {
            StartCoroutine(commandModule.DisplayNotification("Döntetlen!\nVakharc következik!"));
            this.blindMatch = true;
            currentPhase = GameMainPhase.BlindMatch;
            phaseChange = true;
        }

        //Ellenkező esetben eldönthető, hogy ki a győztes
        else
        {
            //Győzelmi üzenet megjelenítése
            StartCoroutine(commandModule.DisplayNotification("A kör győztese: " + dataModule.GetPlayerWithKey(maxId).GetUsername()));

            //Ha vakharcok voltak, akkor ezzel végetértek
            if(this.blindMatch)
            {
                blindMatch = false;
            }

            foreach (int key in dataModule.GetKeyList()) 
            {
                //Ha az első helyezettével megegyezik a kulcs
                if(key == maxId)
                {

                    dataModule.GetPlayerWithKey(key).SetResult(PlayerTurnResult.Win);

                    //Lapok elrakása a győztesek közé a UI-ban
                    StartCoroutine(commandModule.PutCardsAway(key, true));
                }

                //Ellenkező esetben vesztes
                else
                {
                    dataModule.GetPlayerWithKey(key).SetResult(PlayerTurnResult.Lose);

                    //Lapok elrakása a vesztesek közé a UI-ban
                    StartCoroutine(commandModule.PutCardsAway(key, false));
                }

                //Player modell frissítése, aktív mező elemeinek elrakása a győztes vagy vesztes tárolóba
                dataModule.GetPlayerWithKey(key).PutActiveCardAway();
            }

            yield return new WaitForSeconds(drawTempo * 5f);

             //Megnézzük, hogy van-e a győzelmi feltételeknek megfelelő játékos
            if(CheckForWinners())
            {
                currentPhase = GameMainPhase.Result;
                phaseChange = true;
            }

            //Ha nem, akkor megy minden tovább a következő körrel
            else
            {
                //Az utolsó védő lesz a következő támadó
                SetOrder(maxId);    
            }

        }

    }

    //Vakharc: Döntetlen esetén mindegyik lap vesztes lesz, majd lefordítva a pakli felső lapját teszik ki a pájára    
    private IEnumerator BlindMatch()
    {

        foreach (int key in dataModule.GetKeyList()) 
        {

            //Az aktív kártyákat a vesztesek közé rakjuk
            dataModule.GetPlayerWithKey(key).SetResult(PlayerTurnResult.Draw);
            StartCoroutine(commandModule.PutCardsAway(key, false));
            dataModule.GetPlayerWithKey(key).PutActiveCardAway();

            yield return new WaitForSeconds(1f);

            //A pakli felső lapját lerakjuk
            StartCoroutine(DrawCardsUp(key));

        }

        //Megnézzük, hogy van-e a győzelmi feltételeknek megfelelő játékos
        if(CheckForWinners())
        {
            currentPhase = GameMainPhase.Result;
            phaseChange = true;
        }

        //Ha nem, akkor megy minden tovább a következő körrel
        else
        {
            //Az utolsó védő lesz a következő támadó
            SetOrder(dataModule.GetKeyList()[dataModule.GetKeyList().Count - 1]);    
        }

    }

    //Eredmény: Győztes hirdetés.
    private void Result()
    {

        //Ha a lapokból kifogyott valamelyik játékos: Legtöbb nyeréssel rendelkező játékos(ok) viszik a győzelmet
        List<int> winnerCardCount = new List<int>();
        int maxWin = 0;
        int maxKey = -1;

        foreach (int key in dataModule.GetKeyList()) 
        {
            if(dataModule.GetPlayerWithKey(key).GetWinAmount() > maxWin)
            {
                winnerCardCount.Add(dataModule.GetPlayerWithKey(key).GetWinAmount());
                maxWin = dataModule.GetPlayerWithKey(key).GetWinAmount();
                maxKey = key;
            }
        }

        //Ha van holtverseny
        if(winnerCardCount.Count(p => p == maxWin) > 1)
        {
            StartCoroutine(commandModule.DisplayNotification("Játék Vége!\nAz eredmény döntetlen!"));
        }

        //Ha egyértelmű a győzelem
        else 
        {
            StartCoroutine(commandModule.DisplayNotification("Játék Vége!\nA győztes: " + dataModule.GetPlayerWithKey(maxKey).GetUsername()));       
        }
    }
    #endregion


    #region Commands

    //Felhúz a játékosok kezébe 4 lapot kezdésnél
    private IEnumerator DrawStarterCards()
    {
        for(var i = 0; i < 4; i++)
        {
            foreach (int key in dataModule.GetKeyList()) 
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

    //Kártya húzás: Modellből adatok lekérése és továbbítása a megfelelő játékos oldalnak.
    private void DrawTheCard(int key)
    {
        Card cardData = null; 

        if(!blindMatch)
        {
            cardData = dataModule.GetPlayerWithKey(key).DrawCardFromDeck();
        }

        else 
        {
            cardData = dataModule.GetPlayerWithKey(key).BlindDraw();
        }

        //Ha a felhúzás sikeres
        if(cardData != null)
        {
            client.DisplayNewCard(cardData, key, dataModule.GetPlayerWithKey(key).GetPlayerStatus(), this.blindMatch);

            //Ha a játékosról van szó továbbítjuk megjelenítésre a deck új méretét
            if(dataModule.GetPlayerWithKey(key).GetPlayerStatus())
            {
                int remainingDeckSize = dataModule.GetPlayerWithKey(key).GetDeckSize();
                commandModule.RefreshDeckSize(key, remainingDeckSize);
            }
        }

        //Ha vakharc esetén valaki már nem tud pakliból húzni: játék vége
        else if(cardData == null && blindMatch) 
        {
            StartCoroutine(commandModule.DisplayNotification(dataModule.GetPlayerWithKey(key).GetUsername() +" paklija elfogyott!\nJáték vége!"));
            currentPhase = GameMainPhase.Result;
            phaseChange = true;
        }

        //Amúgy addig megy a játék, amíg a kézből is elfogynak a lapok
    }



    private void Pass()
    {
        StartCoroutine(commandModule.DisplayNotification(dataModule.GetPlayerWithKey(currentKey).GetUsername() + " passzolta a képességet!"));
        dataModule.GetPlayerWithKey(currentKey).SetStatus(PlayerTurnStatus.Finished);
        playerReacted = true;
    }

    private IEnumerator Store(int cardPosition)
    {
        //Először megnézzük, hogy tud-e áldozni
        if(dataModule.GetPlayerWithKey(currentKey).GetWinAmount() > 0)
        {
            storeCount++;

            //Ha ember döntött úgy, hogy store-olja a képességet
            if(dataModule.GetPlayerWithKey(currentKey).GetPlayerStatus())
            {
                commandModule.DisplayWinnerCards(dataModule.GetWinnerList(currentKey));

                //Várunk a visszajelzésére
                yield return WaitForPlayerInput();
            }
            playerReacted = true;
        }

        //Ha nem, akkor figyelmeztetjük és reseteljük a kártyát
        else
        {
            StartCoroutine(commandModule.DisplayNotification(dataModule.GetPlayerWithKey(currentKey).GetUsername() + ", ehhez nincs elég győztes lapod!"));
            commandModule.ResetCardSkill(currentKey, cardPosition);
            finishedWithTurn = false;
        }
    }

    private void Use(int cardPosition)
    {
        this.skills.UseSkill(dataModule.GetPlayerWithKey(currentKey).GetCardsOnField()[cardPosition].GetCardID());
        playerReacted = true;
    }

    private void NewSkillCycle(int key)
    {
        storeCount = 0;
        commandModule.NewSkillCycle(key);
    }

    #endregion

    #region Incoming Messages

    //A játékos által kiválasztott harctípus beállítása
    public void HandleStatChange(ActiveStat newStat)
    {   
        //Kitörés a wait fázisból
        playerReacted = true;

        //Választott érték beállítása
        this.currentStat = newStat;

        //Jelenítsük meg a változást
        commandModule.RefreshStatDisplay();

    }

    //Játékos idézéséről a visszajelzés
    public void HandleSummon(int indexInHand)
    {
        playerReacted = true;
        dataModule.GetPlayerWithKey(currentKey).PlayCardFromHand(indexInHand);
    }

    public void HandleSkillStatusChange(SkillState state, bool haveFinished, int cardPosition)
    {
        finishedWithTurn = haveFinished;

        switch (state) 
        {
            case SkillState.Pass: Pass(); break;
            case SkillState.Use: Use(cardPosition); break;
            case SkillState.Store: StartCoroutine(Store(cardPosition)); break;
            default: Pass(); break;
        }


        //Ha az összes pályán lévő kártya skilljéről nyilatkozott a játékos
        if(finishedWithTurn)
        {
            //Ha van köztük stored érték, akkor még nem végeztünk
            if(storeCount == 0)
            {
                dataModule.GetPlayerWithKey(currentKey).SetStatus(PlayerTurnStatus.Finished); 
            }
        }
    }

    public void HandleExit()
    {
        dataModule.ResetDecks();
    }

    public void HandleCardSelection(int cardID)
    {
        playerReacted = true;
        dataModule.SacrificeWinnerCard(cardID, currentKey);
        int winSize = dataModule.GetWinnerAmount(currentKey);
        int lostSize = dataModule.GetLostAmount(currentKey);
        Sprite win = dataModule.GetLastWinnerImage(currentKey);
        Sprite lost = dataModule.GetLastLostImage(currentKey);
        commandModule.ChangePileText(winSize, lostSize, currentKey, win, lost);
    }

    #endregion

    #region Tools

    //Várakozás, amíg a játékos nem ad inputot
    private IEnumerator WaitForPlayerInput()
    {
        playerReacted = false;
        while(!playerReacted)
        {
            yield return null;
        }
    }

    public ActiveStat GetActiveStat()
    {
        return this.currentStat;
    }

    public void SetActiveStat(ActiveStat newStat)
    {
        this.currentStat = newStat;
        commandModule.RefreshStatDisplay();
    }

    public void SetMessageStatus(bool newStatus)
    {
        this.displayedMessageStatus = newStatus;
    }

    public bool IsMessageOnScreen()
    {
        return this.displayedMessageStatus;
    }

    private void ChangePhase(GameMainPhase nextPhase)
    {
        currentPhase = nextPhase;
        phaseChange = true;
    }

    private bool CheckForWinners()
    {
        foreach (int key in dataModule.GetKeyList()) 
        {
            if(dataModule.GetPlayerWithKey(key).GetWinAmount() == winsNeeded)
            {
                return true;
            }
        }

        return false;
    }
    #endregion


}

