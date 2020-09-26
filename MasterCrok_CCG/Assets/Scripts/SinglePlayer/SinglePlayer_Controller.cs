using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.SinglePlayer;

//Alap játékhoz
public enum GameMainPhase {Preparation, SetOrder, DrawPhase, SetRoles, Summon, 
    Reveal, Skill, Compare, Result, BlindMatch, Awards};

public enum DrawTarget {Hand, Field};
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
    private bool actionFinished;
    private bool displayedMessageStatus;
    private bool blindMatch;
    private int storeCount;
    private int currentKey;
    private int currentActiveCard;

    private GameMainPhase currentPhase;
    private ActiveStat currentStat;
    public CardSelectionAction currentAction;
    private CardListType currentSelectionType;

    //Játék győzelmi paraméterei
    public static int winsNeeded = 5;


    //RNG generátor
    private System.Random rng;

    #region MonoBehaviour

    // Start is called before the first frame update
    private void Awake()
    {
        dataModule = new SinglePlayer_Data(this, factory);
        reportModule = new SinglePlayer_Report(this);
        commandModule = new SinglePlayer_Commander(this, client, reportModule);
        skills = new SkillFactory(this);

        currentPhase = GameMainPhase.Preparation;
        currentStat = ActiveStat.NotDecided;
        currentAction = CardSelectionAction.None;
        currentSelectionType = CardListType.None;
        currentActiveCard = -1;

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

    #endregion

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
                    ShowStatBox();
                    yield return WaitForEndOfAction();
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

                    //Engedélyezzük neki a kártyák dragelését
                    commandModule.SetDragStatus(playerKey, true);

                    //Ha ember, akkor várunk az idézésre
                    if(dataModule.GetPlayerWithKey(playerKey).GetPlayerStatus())
                    {
                        //Jelzünk a játékosnak
                        StartCoroutine(commandModule.DisplayNotification("Rakj le egy kártyát, " + dataModule.GetPlayerWithKey(playerKey).GetUsername()));
                        
                        //Várunk a visszajelzésére
                        yield return WaitForEndOfAction();
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

                    //Visszavesszük tőle az opciót a kártya lerakásra
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
                        StartCoroutine(commandModule.DisplayNotification("Dönts a kártyáid képességeiről!"));

                        //Megadjuk a lehetőséget a játékosnak, hogy döntsön a képességekről most
                        commandModule.SetSkillStatus(playerKey, true);

                        //Várunk a visszajelzésére
                        yield return WaitForEndOfAction();

                        //Ha döntött, akkor elvesszük tőle a lehetőséget, hogy módosítson rajta
                        commandModule.SetSkillStatus(playerKey, false);
                    }

                    //Ellenkező esetben az AI dönt a képességről
                    else 
                    {
                        StartCoroutine(AskBotToDecideSkills());

                        //Nem megyünk tovább, amíg nem végez a döntéssel
                        yield return WaitForEndOfAction();
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

    #region Skill Decisions

    //Képesség passzolása
    public void Pass()
    {
        StartCoroutine(commandModule.DisplayNotification(dataModule.GetPlayerWithKey(currentKey).GetUsername() + " passzolta a képességet!"));
        dataModule.GetPlayerWithKey(currentKey).SetStatus(PlayerTurnStatus.Finished);
        actionFinished = true;
    }
    //Képesség tartalékolása
    private IEnumerator Store(int cardPosition)
    {
        //Először megnézzük, hogy tud-e áldozni
        if(dataModule.GetPlayerWithKey(currentKey).GetWinAmount() > 0)
        {
            storeCount++;

            currentAction = CardSelectionAction.Store;

            //Ha ember döntött úgy, hogy store-olja a képességet
            if(dataModule.GetPlayerWithKey(currentKey).GetPlayerStatus())
            {
                commandModule.CardChoice(dataModule.GetWinnerList(currentKey), currentAction);

                //Várunk a visszajelzésére
                yield return WaitForEndOfAction();
            }

            currentAction = CardSelectionAction.None;
            actionFinished = true;
        }

        //Ha nem, akkor figyelmeztetjük és reseteljük a kártyát
        else
        {
            StartCoroutine(commandModule.DisplayNotification(dataModule.GetPlayerWithKey(currentKey).GetUsername() + ", ehhez nincs elég győztes lapod!"));
            commandModule.ResetCardSkill(currentKey, cardPosition);
        }
    }
    //Képesség használata
    private void Use(int cardPosition)
    {
        Debug.Log("Cards on the field: " + dataModule.GetPlayerWithKey(currentKey).GetCardsOnField().Count.ToString());
        this.skills.UseSkill(dataModule.GetPlayerWithKey(currentKey).GetCardsOnField()[cardPosition].GetCardID());
    }
    #endregion

    #region Skill Actions

    //A megadott paraméterekkel megjelenít egy kártya listát, amiből választhat a játékos
    public void ChooseCard(CardListFilter filter = CardListFilter.None, int limit = 0)
    {
        
        //Ha ember
        if(IsTheActivePlayerHuman())
        {
            StartCoroutine(DisplayCardList(filter, limit));
        }

        //Ha bot
        else
        {
            AskBotToSelectCard(filter, limit);
        }
    }

    //Kicserél egy mezőn lévő lapot egy másikra
    public void SwitchCard(int cardOnField, int cardToSwitch)
    {
        //Ha kézben kell keresni a választott lapot
        if(currentSelectionType == CardListType.Hand)
        {
            //Adatok kinyerése
            Card handData = dataModule.GetCardFromHand(currentKey, cardToSwitch);
            Card fieldData = dataModule.GetCardFromField(currentKey, cardOnField);

            //Model frissítése
            dataModule.SwitchFromHand(currentKey, cardOnField, cardToSwitch);

            //UI frissítése
            commandModule.SwitchHandFromField(currentKey, fieldData, cardOnField, handData, cardToSwitch, IsTheActivePlayerHuman());
        }
    }

    public void ReviveCard(int cardID)
    {
        //Ha kézben kell keresni a választott lapot
        if(currentSelectionType == CardListType.Losers)
        {
            //Adatok kinyerése
            Card cardData = dataModule.GetCardFromLosers(currentKey, cardID);

            //Model frissítése
            dataModule.ReviveLostCard(currentKey, cardID);

            //UI frissítése
            commandModule.DrawNewCard(cardData, currentKey, IsTheActivePlayerHuman());

            int winSize = dataModule.GetWinnerAmount(currentKey);
            int lostSize = dataModule.GetLostAmount(currentKey);
            Sprite win = dataModule.GetLastWinnerImage(currentKey);
            Sprite lost = dataModule.GetLastLostImage(currentKey);
            commandModule.ChangePileText(winSize, lostSize, currentKey, win, lost);
        }
    }

    public void GreatFight()
    {
        foreach (int key in dataModule.GetKeyList()) 
        {
            DrawTheCard(key, DrawTarget.Field);
        }
    }

    //Megváltoztatásra kerül az aktív harctípus
    public IEnumerator TriggerStatChange()
    {
        if(IsTheActivePlayerHuman())
        {
            ShowStatBox();
            yield return WaitForEndOfAction();
        }

        else 
        {
            //AI agy segítségét hívjuk a döntésben
            currentStat = Bot_Behaviour.ChangeFightType(dataModule.GetCardsFromField(currentKey),
                dataModule.GetOpponentsCard(currentKey), currentStat);

            //Jelenítsük meg a változást
            commandModule.RefreshStatDisplay();
        }
    }

    //Megváltoztatja a harc típusát egy megadott értékre
    public void SetActiveStat(ActiveStat newStat)
    {
        this.currentStat = newStat;
        commandModule.RefreshStatDisplay();
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
    private void DrawTheCard(int key, DrawTarget target = DrawTarget.Hand)
    {
        Card cardData = null; 

        if(target == DrawTarget.Hand && !blindMatch)
        {
            cardData = dataModule.GetPlayerWithKey(key).DrawCardFromDeck();
        }

        else if(blindMatch || target == DrawTarget.Field)
        {
            cardData = dataModule.GetPlayerWithKey(key).BlindDraw();
            Debug.Log(cardData);
        }

        //Ha a felhúzás sikeres
        if(cardData != null)
        {
            client.DisplayNewCard(cardData, key, dataModule.GetPlayerWithKey(key).GetPlayerStatus(), this.blindMatch, target);

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

    //Bizonyos kártya listát jelenít meg az aktív játékos számára, amiből választhat ezután
    private IEnumerator DisplayCardList(CardListFilter filter = CardListFilter.None, int limit = 0)
    {

        List<Card> cardList = new List<Card>(); 

        switch (currentSelectionType)
        {
            case CardListType.Hand: cardList = dataModule.GetPlayerWithKey(currentKey).GetCardsInHand(filter); break;
            case CardListType.Winners: cardList = dataModule.GetPlayerWithKey(currentKey).GetWinners(); break;
            case CardListType.Losers: cardList = dataModule.GetPlayerWithKey(currentKey).GetLosers(); break;
            case CardListType.Deck: cardList = dataModule.GetPlayerWithKey(currentKey).GetDeck(limit); break;
            default: break;
        }

        commandModule.CardChoice(cardList, currentAction);

        //Várunk a visszajelzésére
        yield return WaitForEndOfAction();

    }

    private void NewSkillCycle(int key)
    {
        storeCount = 0;
        commandModule.NewSkillCycle(key);
    }

    private void ShowStatBox()
    {
        StartCoroutine(commandModule.DisplayStatBox());
    }

    #endregion

    #region Incoming Messages

    //A játékos által kiválasztott harctípus beállítása
    public void HandleStatChange(ActiveStat newStat)
    {   
        //Kitörés a wait fázisból
        actionFinished = true;

        //Választott érték beállítása
        this.currentStat = newStat;

        //Jelenítsük meg a változást
        commandModule.RefreshStatDisplay();

    }

    //Játékos idézéséről a visszajelzés
    public void HandleSummon(int indexInHand)
    {
        actionFinished = true;
        dataModule.GetPlayerWithKey(currentKey).PlayCardFromHand(indexInHand);
    }

    //Játékos kártyájának skill helyzetében történt válotoztatás kezelése
    public void HandleSkillStatusChange(SkillState state, int cardPosition)
    {

        switch (state) 
        {
            case SkillState.Pass: Pass(); break;
            case SkillState.Use: currentActiveCard = cardPosition; Use(currentActiveCard); break;
            case SkillState.Store: StartCoroutine(Store(cardPosition)); break;
            default: Pass(); break;
        }

        //Ha az összes pályán lévő kártya skilljéről nyilatkozott a játékos
        if(commandModule.AskSkillStatus(currentKey))
        {
            //Ha van köztük stored érték, akkor még nem végeztünk
            if(storeCount == 0)
            {
                dataModule.GetPlayerWithKey(currentKey).SetStatus(PlayerTurnStatus.Finished); 
            }
        }
    }

    //Kezeljük, ha a játékos kilép a játékból
    public void HandleExit()
    {
        dataModule.ResetDecks();
    }

    //Kezeljük a játékos kiválasztott kártyáját
    public IEnumerator HandleCardSelection(int cardID)
    {

        //Ha az akció tartalékolás volt
        if(currentAction == CardSelectionAction.Store)
        {
            //A modelben frissítjük a helyzetet
            dataModule.SacrificeWinnerCard(cardID, currentKey);
            //UI oldalon is frissítjük a helyzetet, hogy aktuális állapotot tükrözzön
            int winSize = dataModule.GetWinnerAmount(currentKey);
            int lostSize = dataModule.GetLostAmount(currentKey);
            Sprite win = dataModule.GetLastWinnerImage(currentKey);
            Sprite lost = dataModule.GetLastLostImage(currentKey);
            commandModule.ChangePileText(winSize, lostSize, currentKey, win, lost);
            actionFinished = true;
        }

        //Ha az akció a csere volt
        else if(currentAction == CardSelectionAction.Switch)
        {
            SwitchCard(currentActiveCard, cardID);
            actionFinished = true;
        }

        //Ha az akció skill lopás volt
        else if(currentAction == CardSelectionAction.SkillUse)
        {

            //Másik Devil crok képesség esetén passzolunk
            if(cardID == 3)
            {
                Pass();
                actionFinished = true;
            }

            //Amúgy meg használjuk a másik lap képességét
            else
            {
                skills.UseSkill(cardID);
                yield return WaitForEndOfAction();
            }
            
        }

        else if(currentAction == CardSelectionAction.Revive)
        {
            ReviveCard(cardID);
            actionFinished = true;
        }

        
        currentAction = CardSelectionAction.None;
    }

    public void HandleSelectionCancel()
    {
        commandModule.ResetCardSkill(currentKey,currentActiveCard);
    }

    public void HandleDisplayRequest(CardListType listType, int playerKey)
    {
        List<Card> cardList = new List<Card>();

        switch (listType) 
        {
            case CardListType.Winners: cardList = dataModule.GetPlayerWithKey(playerKey).GetWinners(); break;
            case CardListType.Losers: cardList = dataModule.GetPlayerWithKey(playerKey).GetLosers(); break;
            default: break;
        }

        //Lélekrablás esetén saját vesztes lapokat nem választhatunk ki, illetve mások győztes lapjait sem
        if(playerKey == currentKey && currentAction == CardSelectionAction.SkillUse || listType == CardListType.Winners)
        {
            commandModule.CardChoice(cardList, CardSelectionAction.None);
        }

        else 
        {
            commandModule.CardChoice(cardList, currentAction);
        }
        
    }

    #endregion

    #region Bot Actions

    private IEnumerator AskBotToDecideSkills()
    {
        //A többi játékos pályán lévő kártyáinak adatait fetcheljük, mivel ezekről tudhat az AI

        List<Card> cardsOnField = new List<Card>();
        List<List<Card>> opponentCards = new List<List<Card>>();
        List<Card> cardsInHand = new List<Card>();
        int winAmount = 0;

        int cardCount = 0;
        int id = 0;
        //Addig megy a ciklus újra és újra, amíg minden lapjáról nem döntött az AI
        while(true)
        {

            //Ha a field szerint nincs több inaktív lap, akkor végeztünk
            if(commandModule.AskSkillStatus(currentKey))
            {
                break;
            }

            //Aktuális adatok lekérése
            cardsOnField = dataModule.GetPlayerWithKey(currentKey).GetCardsOnField();
            cardCount = cardsOnField.Count;
            opponentCards = dataModule.GetOpponentsCard(currentKey);
            cardsInHand = dataModule.GetPlayerWithKey(currentKey).GetCardsInHand();
            winAmount = dataModule.GetWinnerAmount(currentKey);

            //Ha a vizsgált kártya skill státusza Not Decided
            if(commandModule.AskCardSkillStatus(currentKey, id) == SkillState.NotDecided)
            {
                currentActiveCard = id;
                //AI agy segítségét hívjuk a döntésben, átadjuk neki a szükséges infót és választ kapunk cserébe
                SkillResponse response = Bot_Behaviour.ChooseSkill(id, cardsInHand, cardsOnField, opponentCards, winAmount);

                //Kis várakozás, gondolkodási idő imitálás a döntés meghozása előtt, hogy ne történjen minden túl hirtelen
                yield return new WaitForSeconds(drawTempo * UnityEngine.Random.Range(5,15));

                //Az AI döntése alapján itt végzi el a játék a megfelelő akciókat
                switch (response) 
                {
                    case SkillResponse.Pass: Use(id); commandModule.SetSkillState(currentKey, id, SkillState.Pass); break;
                    case SkillResponse.Store: Use(id); commandModule.SetSkillState(currentKey, id, SkillState.Store); break;
                    case SkillResponse.Use: Use(id); commandModule.SetSkillState(currentKey, id, SkillState.Use); break;
                    default: Pass(); break;
                }

                //Újrakezdjük a ciklust
                id = 0;
            }

            //Ellenkező esetben ha megtehetjük, léptetjük a ciklust
            else 
            {
                if(id + 1 < cardCount)
                {
                    id++;
                }    
            }
        }

        //Az AI végzett a körével a döntések után
        dataModule.GetPlayerWithKey(currentKey).SetStatus(PlayerTurnStatus.Finished); 
        actionFinished = true;
    }

    private void AskBotToSelectCard(CardListFilter filter, int limit)
    {
        //Ha cseréről van szó
        if(currentAction == CardSelectionAction.Switch)
        {
            int handID = -1;

            //Ha kézből cserélünk
            if(currentSelectionType == CardListType.Hand)
            {
                handID = Bot_Behaviour.HandSwitch(dataModule.GetCardsFromHand(currentKey),
                    dataModule.GetCardsFromField(currentKey),dataModule.GetOpponentsCard(currentKey), currentStat);

                SwitchCard(currentActiveCard, handID);
            }
        }

        //Ha skill lopásról van szó
        else if(currentAction == CardSelectionAction.SkillUse)
        {
            List<Card> cards = dataModule.GetOtherLosers(currentKey);
            int cardID = Bot_Behaviour.WhichSkillToUse(cards);
            Debug.Log("AI Devil used: " + cardID.ToString());
            this.skills.UseSkill(cardID);
        }

        //Ha skill lopásról van szó
        else if(currentAction == CardSelectionAction.Revive)
        {
            List<Card> cards = dataModule.GetLostList(currentKey);
            int cardID = Bot_Behaviour.WhomToRevive(cards);
            ReviveCard(cardID);
        }

        currentAction = CardSelectionAction.None;
    }

    #endregion


    #region Tools

    //Várakozás, amíg a játékos nem ad inputot
    private IEnumerator WaitForEndOfAction()
    {
        actionFinished = false;

        while(!actionFinished)
        {
            yield return null;
        }
    }

    public void SetPlayerReaction(bool newStatus)
    {
        this.actionFinished = newStatus;
    }

    public void SetSelectionAction(CardSelectionAction action)
    {
        currentAction = action;
    }

    public void SetSwitchType(CardListType type)
    {
        currentSelectionType = type;
    }

    public ActiveStat GetActiveStat()
    {
        return this.currentStat;
    }

    public void SetMessageStatus(bool newStatus)
    {
        this.displayedMessageStatus = newStatus;
    }

    public bool IsMessageOnScreen()
    {
        return this.displayedMessageStatus;
    }

    public bool IsTheActivePlayerHuman()
    {
        return dataModule.GetPlayerWithKey(currentKey).GetPlayerStatus();
    }

    //Visszaadja, hogy van-e vesztesünk
    public bool DoWeHaveLosers()
    {
        if(dataModule.GetLostList(currentKey).Count > 0)
        {
            return true;
        }

        else {
            return false;
        }
    }

    //Visszaadja, hogy vannak-e jelenleg vesztes lapok az ellenfelek térfelein
    public bool IsThereOtherLostCards()
    {
        if(dataModule.GetOtherLosers(currentKey).Count > 0)
        {
            return true;
        }

        else {
            return false;
        }
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

