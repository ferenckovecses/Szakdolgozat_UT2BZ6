using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ClientControll;

namespace GameControll
{
    public class GameState_Controller : MonoBehaviour
    {
        [Header("Prefabok és referenciák")]
        public CardFactory factory;

        //Vezérlő modulok
        private Data_Controller dataModule;
        private Input_Controller inputModule;
        private Client_Controller clientModule;
        private Skill_Controller skillModule;
        private AI_Controller AI_module;
        public Client client;

        //Játékbeli fázisok és események
        private bool phaseChange;
        private bool firstRound;
        private bool actionFinished;
        private bool displayedMessageStatus;
        private bool blindMatch;
        private bool turnFinished;
        private bool skillFinished;
        private bool changedOrder;
        private int storeCount;
        public int currentKey;
        private int currentActiveCard;
        private int choosenKey;
        private int lastWinnerKey;
        private int newAttacker;
        private List<int> lateSkillKeys;

        public MainGameStates currentPhase;
        public CardStatType currentStat;
        public SkillEffectAction currentAction;
        public CardListTarget currentSelectionType;

        //Játék győzelmi paraméterei, TODO: Módosítható
        public static int winsNeeded = 5;


        //RNG generátor
        private System.Random rng;

        #region MonoBehaviour

        private void Awake()
        {
            //Modulok beállítása
            this.dataModule = new Data_Controller(this, factory);
            this.inputModule = new Input_Controller(this);
            this.clientModule = new Client_Controller(this, client);
            this.skillModule = new Skill_Controller(this);
            this.AI_module = new AI_Controller(this);

            //Játékfázisok és státuszok alapértékezése
            this.currentPhase = MainGameStates.SetupGame;
            this.currentStat = CardStatType.NotDecided;
            this.currentAction = SkillEffectAction.None;
            this.currentSelectionType = CardListTarget.None;
            this.currentActiveCard = -1;
            this.lastWinnerKey = -1;
            this.phaseChange = true;
            this.firstRound = true;
            this.displayedMessageStatus = false;
            this.blindMatch = false;
            this.actionFinished = true;
            this.turnFinished = true;
            this.skillFinished = true;
            this.changedOrder = false;
            this.lateSkillKeys = new List<int>();

            rng = new System.Random();
        }

        // Update is called once per frame
        private void Update()
        {
            //Játék fő fázisainak változásai
            if (phaseChange)
            {
                phaseChange = false;
                switch (currentPhase)
                {
                    case MainGameStates.SetupGame: Preparation(); break;
                    case MainGameStates.CreateOrder: SetOrder(); break;
                    case MainGameStates.StarterDraw: DrawPhase(); break;
                    case MainGameStates.SetRoles: SetRoles(); break;
                    case MainGameStates.SetStat: StartCoroutine(SetStat()); break;
                    case MainGameStates.SummonCard: StartCoroutine(Summon()); break;
                    case MainGameStates.RevealCards: StartCoroutine(Reveal()); break;
                    case MainGameStates.QuickSkills: StartCoroutine(QuickSkills()); break;
                    case MainGameStates.NormalSkills: StartCoroutine(Skill()); break;
                    case MainGameStates.CompareCards: StartCoroutine(Compare()); break;
                    case MainGameStates.LateSkills: StartCoroutine(LateSkills()); break;
                    case MainGameStates.PutCardsAway: StartCoroutine(PutCardsAway()); break;
                    case MainGameStates.BlindMatch: StartCoroutine(BlindMatch()); break;
                    case MainGameStates.CreateResult: StartCoroutine(Result()); break;
                    default: break;
                }
            }
        }

        #endregion

        #region Main Game Phases

        //Előkészületi fázis
        private void Preparation()
        {
            dataModule.AddPlayer();
            dataModule.GenerateOpponents();
            clientModule.GenerateUI(dataModule.GetNumberOfOpponents(), dataModule.GetKeyList(), dataModule.GetNameList(), dataModule.GetPlayerAtIndex(0).GetDeckSize());
            ChangePhase(MainGameStates.CreateOrder);
        }

        //A játékos listában felállít egy sorrendet, amit követve jönnek a következő körben
        private void SetOrder(int winnerKey = -1)
        {
            dataModule.CreateRandomOrder();

            //A legelső körnél nincs szükség különleges sorrendre
            if (firstRound)
            {
                firstRound = false;
                ChangePhase(MainGameStates.StarterDraw);
            }

            //A többi esetben az előző meccs nyertese lesz az új támadó, azaz az első a listában
            else
            {
                //Valós érték vizsgálat
                if (winnerKey != -1)
                {
                    dataModule.PutWinnerInFirstPlace(winnerKey);
                }

                ChangePhase(MainGameStates.SetRoles);
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
                if (firstPlayer)
                {
                    firstPlayer = false;
                    dataModule.GetPlayerWithKey(key).SetRole(PlayerTurnRole.Attacker);
                }

                else
                {
                    dataModule.GetPlayerWithKey(key).SetRole(PlayerTurnRole.Defender);
                }

                dataModule.GetPlayerWithKey(key).SetStatus(PlayerTurnStatus.ChooseCard);

                //A kézben lévő lapokra érvényesítjük a bónuszokat, így az a következő fázisban már látható hatással bír
                dataModule.GetPlayerWithKey(key).ApplyFieldBonusToAll();
            }
            ChangePhase(MainGameStates.SetStat);
        }

        //A támadó játékost megkérdezi, hogy milyen statot választ
        private IEnumerator SetStat()
        {
            currentKey = dataModule.GetKeyList()[0];

            //Ha nem vakharc van, akkor húzhat lapot
            if(!blindMatch)
            {
                //Húzunk fel a játékosnak 1 lapot a kör elején
                StartCoroutine(DrawCardsUp(currentKey));
                yield return WaitForEndOfAction();
                StartCoroutine(CardAmountCheck());
                yield return WaitForEndOfAction();
            }

            //Ha ember, akkor dobjuk fel neki az értékválasztó képernyőt
            if (dataModule.GetPlayerWithKey(currentKey).GetPlayerStatus())
            {
                ShowStatBox();

                //Várunk, amíg befejezi a döntést
                yield return WaitForTurnsEnd();
            }

            //Ellenkező esetben az AI dönt
            else
            {
                //AI agy segítségét hívjuk a döntésben
                currentStat = Bot_Behaviour.ChooseFightType(dataModule.GetPlayerWithKey(currentKey).GetCardsInHand());

                //Jelenítsük meg a változást
                clientModule.RefreshStatDisplay();
            }

            ChangePhase(MainGameStates.SummonCard);

        }


        //Idézés fázis
        private IEnumerator Summon()
        {
            //Minden játékoson végigmegy
            foreach (int playerKey in dataModule.GetKeyList())
            {
                currentKey = playerKey;

                //Ha a játékosnak már nincs idézhető lapja: játék vége
                if (dataModule.GetPlayerWithKey(currentKey).GetHandCount() == 0)
                {
                    StartCoroutine(clientModule.DisplayNotification(dataModule.GetPlayerWithKey(currentKey).GetUsername() + " kifogyott a lapokból!\nJáték vége!"));
                    yield return WaitForEndOfText();
                    ChangePhase(MainGameStates.CreateResult);
                }

                //Csak akkor kell kézből idézni, ha nincs vakharc
                if (!this.blindMatch)
                {
                    //Értesítést adunk a kör kezdetéről
                    StartCoroutine(clientModule.DisplayNotification(dataModule.GetPlayerWithKey(currentKey).GetUsername() + " következik!"));
                    yield return WaitForEndOfText();

                    //A támadó az előző fázisban már húzott lapot, de a védők húzhatnak a kör elején
                    if(dataModule.GetPlayerWithKey(currentKey).GetRole() == PlayerTurnRole.Defender)
                    {   
                        //Húzunk fel a játékosnak 1 lapot a kör elején
                        StartCoroutine(DrawCardsUp(currentKey));
                        yield return WaitForEndOfAction();
                        StartCoroutine(CardAmountCheck());
                        yield return WaitForEndOfAction();
                    }

                    //Ha a játékos státusza szerint a kártyahúzásra vár
                    if (dataModule.GetPlayerWithKey(currentKey).GetStatus() == PlayerTurnStatus.ChooseCard)
                    {

                        //Engedélyezzük neki a kártyák dragelését
                        clientModule.SetDragStatus(currentKey, true);

                        //Ha ember, akkor várunk az idézésre
                        if (dataModule.GetPlayerWithKey(currentKey).GetPlayerStatus())
                        {
                            //Jelzünk a játékosnak
                            StartCoroutine(clientModule.DisplayNotification("Rakj le egy kártyát, " + dataModule.GetPlayerWithKey(currentKey).GetUsername()));
                            yield return WaitForEndOfText();

                            //Várunk a visszajelzésére
                            yield return WaitForTurnsEnd();
                        }

                        //Ellenkező esetben az AI-al rakatunk le kártyát
                        else
                        {

                            //AI agy segítségét hívjuk a döntésben
                            int index = Bot_Behaviour.ChooseRightCard(dataModule.GetPlayerWithKey(playerKey).GetCardsInHand(), currentStat);

                            dataModule.GetPlayerWithKey(playerKey).PlayCardFromHand(index);


                            //Lerakatjuk a kártyát
                            StartCoroutine(clientModule.SummonCard(playerKey, index));

                            //Várunk az akció befejezésére
                            yield return WaitForTurnsEnd();
                        }
                    }
                }

                //Átállítjuk a játékos státuszát: A képesség kiválasztására vár
                dataModule.GetPlayerWithKey(playerKey).SetStatus(PlayerTurnStatus.ChooseSkill);
            }

            ChangePhase(MainGameStates.RevealCards);
        }

        //Pályára rakott kártyák felfedése sorban
        private IEnumerator Reveal()
        {
            foreach (int key in dataModule.GetKeyList())
            {
                clientModule.RevealCards(key);
                yield return new WaitForSeconds(GameSettings_Controller.drawTempo * 5);
            }

            ChangePhase(MainGameStates.QuickSkills);
        }

        private IEnumerator QuickSkills()
        {
            foreach (int key in dataModule.GetKeyList())
            {
                currentKey = key;
                int position = 0;
                foreach (Card card in dataModule.GetCardsFromField(currentKey)) 
                {
                    //Ha a megadott játékosnál van a mezőn gyors skill és még eldöntetlen/felhasználható
                    if(card.HasAQuickSkill() && clientModule.AskCardSkillStatus(key, position) == SkillState.NotDecided)
                    {
                        Use(position);
                        yield return WaitForEndOfSkill();

                        StartCoroutine(clientModule.DisplayNotification(dataModule.GetPlayerName(key) +" Gyors képességet használt!"));
                        yield return WaitForEndOfText();
                    }

                    position++;
                }
            }

            ChangePhase(MainGameStates.NormalSkills);
        }

        //Skill fázis: Körbemegyünk és mindenkit megkérdezünk, hogy mit akar a képességével kezdeni
        private IEnumerator Skill()
        {
            //Ciklusváltozó a while-hoz: Akkor lesz true, ha mindenki döntött a képességéről
            bool everyoneDecided = false;

            //Ha mindenki döntött, akkor break
            int playerMadeDecision;

            //A lateSkill kulcsok listáját kitakarítjuk
            lateSkillKeys.Clear();

            SetCurrentAction(SkillEffectAction.None);

            //Addig megyünk, amíg mindenki el nem használja vagy passzolja a képességét
            while (!everyoneDecided)
            {
                playerMadeDecision = 0;
                foreach (int playerKey in dataModule.GetKeyList())
                {
                    currentKey = playerKey;

                    NewSkillCycle(playerKey);
                    //Ha a játékos státusza szerint a skill eldöntésére vár
                    if (dataModule.GetPlayerWithKey(currentKey).GetStatus() == PlayerTurnStatus.ChooseSkill)
                    {

                        //Ha ember, akkor várunk a döntésre
                        if (dataModule.GetPlayerWithKey(currentKey).GetPlayerStatus())
                        {
                            //Jelezzük a játékosnak, hogy itt az idő dönteni a képességről
                            StartCoroutine(clientModule.DisplayNotification("Dönts a kártyáid képességeiről!"));
                            yield return WaitForEndOfText();

                            //Megjelenítjük a kör vége gombot
                            clientModule.SetEndTurnButton(true);

                            //Megadjuk a lehetőséget a játékosnak, hogy döntsön a képességekről most
                            clientModule.SetSkillStatus(currentKey, true);

                            //Várunk a visszajelzésére
                            yield return WaitForTurnsEnd();

                            //Ha döntött, akkor elvesszük tőle a lehetőséget, hogy módosítson rajta
                            clientModule.SetSkillStatus(currentKey, false);

                            //Az esetleges nem rendelkezett lapokat passzoljuk
                            clientModule.HandleRemainingCards(currentKey);
                        }

                        //Ellenkező esetben az AI dönt a képességről
                        else
                        {
                            StartCoroutine(AI_module.DecideSkill(currentKey));

                            //Nem megyünk tovább, amíg nem végez a döntéssel
                            yield return WaitForTurnsEnd();

                            Debug.Log("Waiting ended");
                        }

                        SetCurrentAction(SkillEffectAction.None);
                    }

                    //Ha az aktuális játékos végzett, növeljük a countert
                    if (dataModule.GetPlayerWithKey(currentKey).GetStatus() == PlayerTurnStatus.Finished)
                    {
                        playerMadeDecision++;
                    }

                }
                //Ha mindenki végzett, akkor loop vége
                if (playerMadeDecision == dataModule.GetNumberOfOpponents() + 1)
                {
                    everyoneDecided = true;
                }
            }

            ChangePhase(MainGameStates.CompareCards);

        }

        //Összehasonlítás fázis: Összehasonlítjuk az aktív kártyák értékét
        private IEnumerator Compare()
        {
            //Kiértékeléshez szükséges változók
            List<int> values = new List<int>();
            int max = 0;
            int maxId = -1;

            foreach (int key in dataModule.GetKeyList())
            {

                //Hozzáadjuk minden mező értékét
                int playerFieldValue = dataModule.GetPlayerWithKey(key).GetActiveCardsValue(currentStat);

                values.Add(playerFieldValue);


                //Eltároljuk a legnagyobb értéket menet közben
                if (playerFieldValue > max)
                {
                    max = playerFieldValue;
                    maxId = key;
                }
            }

            //Ha több ember is rendelkezik a max value-val: Vakharc
            if (values.Count(p => p == max) > 1)
            {
                StartCoroutine(clientModule.DisplayNotification("Döntetlen!"));
                yield return WaitForEndOfText();

                this.blindMatch = true;
            }

            //Ellenkező esetben eldönthető, hogy ki a győztes
            else
            {
                //Győzelmi üzenet megjelenítése
                StartCoroutine(clientModule.DisplayNotification("A kör győztese: " + dataModule.GetPlayerName(maxId)));
                yield return WaitForEndOfText();

                //Ha vakharcok voltak, akkor ezzel végetértek
                if (this.blindMatch)
                {
                    this.blindMatch = false;
                }
            }

            lastWinnerKey = maxId;

            //Győzelmi státuszok beállítása
            foreach (int key in dataModule.GetKeyList())
            {

                //Ha az első helyezettével megegyezik a kulcs és nincs vakharc státusz
                if (key == lastWinnerKey && !blindMatch)
                {
                    dataModule.GetPlayerWithKey(key).SetResult(PlayerTurnResult.Win);
                }

                //Ellenkező esetben vesztes
                else
                {
                    dataModule.GetPlayerWithKey(key).SetResult(PlayerTurnResult.Lose);
                }
            }

            ChangePhase(MainGameStates.LateSkills);

        }

        private IEnumerator LateSkills()
        {
            foreach (int key in this.lateSkillKeys)
            {
                currentKey = key;
                /*
                int position = 0;
                foreach (Card card in dataModule.GetCardsFromField(currentKey)) 
                {
                    //Ha a megadott játékosnál van a mezőn gyors skill és még eldöntetlen/felhasználható
                    if(card.HasALateSkill() && clientModule.AskCardSkillStatus(key, position) == SkillState.Use)
                    {
                        currentActiveCard = position;

                        //Használjuk a késői képességet
                        Use(position);
                        yield return WaitForEndOfSkill();

                        StartCoroutine(clientModule.DisplayNotification(dataModule.GetPlayerName(key) +" késői képességet használt!"));
                        yield return WaitForEndOfText();
                    }

                    position++;
                }
                */
                yield return new WaitForSeconds(0.1f);
            }

            ChangePhase(MainGameStates.PutCardsAway);
        }

        private IEnumerator PutCardsAway()
        {
            foreach (int key in dataModule.GetKeyList())
            {
                //Ha az első helyezettével megegyezik a kulcs, akkor győztes
                if (dataModule.GetPlayerWithKey(key).GetResult() == PlayerTurnResult.Win)
                {
                    //Lapok elrakása a győztesek közé a UI-ban
                    StartCoroutine(clientModule.PutCardsAway(key, true));
                }

                //Ha nem győztes, akkor vesztes.
                //*Surprised Pikachu fej*
                else
                {
                    //Lapok elrakása a vesztesek közé a UI-ban
                    StartCoroutine(clientModule.PutCardsAway(key, false));
                    
                }
                yield return WaitForEndOfAction();

                //Player modell frissítése, aktív kártya field elemeinek elrakása a győztes vagy vesztes tárolóba
                dataModule.GetPlayerWithKey(key).PutActiveCardAway();

                //Továbblépés előtt nullázzuk a kézben lévő lapok bónuszait, hogy tiszta lappal kezdődjön a következő kör
                //Hehe, érted, tiszta LAPPAL, mert ez egy kártyajáték :(( 
                //Please end my suffer, csak a fránya diplomámat akarom
                dataModule.GetPlayerWithKey(key).ResetBonuses();

                //Csökkentjük a bónuszok számlálóját és megválunk a lejárt bónuszoktól
                dataModule.GetPlayerWithKey(key).UpdateBonuses();
                
            }


            //Ha a következő kör vakharc kell hogy legyen.
            if(this.blindMatch)
            {
                ChangePhase(MainGameStates.BlindMatch);
            }

            //Megnézzük, hogy van-e a győzelmi feltételeknek megfelelő játékos
            else if (CheckForWinners())
            {
                ChangePhase(MainGameStates.CreateResult);
            }

            //Ha nem, akkor megy minden tovább a következő körrel
            else
            {
                //Ha történt sorrendváltoztatás
                if(this.changedOrder)
                {
                    SetOrder(newAttacker);
                }

                //Normál esetben pedig
                else 
                {
                    //A győztes kulccsal rendelkező játékos lesz a következő támadó
                    SetOrder(lastWinnerKey);
                }

            }

        }

        //Vakharc: Döntetlen esetén mindegyik lap vesztes lesz, majd lefordítva a pakli felső lapját teszik ki a pájára    
        private IEnumerator BlindMatch()
        {
            StartCoroutine(clientModule.DisplayNotification("Vakharc következik!"));
            yield return WaitForEndOfText();

            foreach (int key in dataModule.GetKeyList())
            {
                //A pakli felső lapját lerakjuk
                StartCoroutine(DrawCardsUp(key, 1, DrawTarget.Field, DrawType.Blind, SkillState.NotDecided));
            }

            //Ha nincs sorrend változtatás
            if(!changedOrder)
            {
                //Az előző körben lévő utolsó védő lesz a következő támadó
                SetOrder(dataModule.GetKeyList()[dataModule.GetKeyList().Count - 1]);
            }

            //Ha egy képesség miatt változott, hogy ki kezd és választ típust a következő körben
            else 
            {
                changedOrder = false;
                SetOrder(newAttacker);
            }

        }

        

        //Eredmény: Győztes hirdetés.
        private IEnumerator Result()
        {

            //Ha a lapokból kifogyott valamelyik játékos: Legtöbb nyeréssel rendelkező játékos(ok) viszik a győzelmet
            List<int> winnerCardCount = new List<int>();
            int maxWin = 0;
            int maxKey = -1;

            foreach (int key in dataModule.GetKeyList())
            {
                if (dataModule.GetPlayerWithKey(key).GetWinAmount() > maxWin)
                {
                    winnerCardCount.Add(dataModule.GetPlayerWithKey(key).GetWinAmount());
                    maxWin = dataModule.GetPlayerWithKey(key).GetWinAmount();
                    maxKey = key;
                }
            }

            //Ha van holtverseny
            if (winnerCardCount.Count(p => p == maxWin) > 1)
            {
                StartCoroutine(clientModule.DisplayNotification("Játék Vége!\nAz eredmény döntetlen!"));
            }

            //Ha egyértelmű a győzelem
            else
            {
                StartCoroutine(clientModule.DisplayNotification("Játék Vége!\nA győztes: " + dataModule.GetPlayerWithKey(maxKey).GetUsername()));
            }
            yield return WaitForEndOfText();
        }
        #endregion

        #region Skill Decisions

        //Képesség passzolása
        public void Pass()
        {
            StartCoroutine(clientModule.DisplayNotification(dataModule.GetPlayerWithKey(currentKey).GetUsername() + " passzolta a képességet!"));
            StartCoroutine(WaitForText());
            StartCoroutine(SkillFinished());
            //dataModule.GetPlayerWithKey(currentKey).SetStatus(PlayerTurnStatus.Finished);
        }
        //Képesség tartalékolása
        public IEnumerator Store(int cardPosition)
        {
            //Először megnézzük, hogy tud-e áldozni
            if (dataModule.GetPlayerWithKey(currentKey).GetWinAmount() > 0)
            {
                storeCount++;

                currentAction = SkillEffectAction.Store;

                //Ha ember döntött úgy, hogy store-olja a képességet
                if (dataModule.GetPlayerWithKey(currentKey).GetPlayerStatus())
                {
                    clientModule.CardChoice(dataModule.GetWinnerList(currentKey), currentAction, currentKey);

                    //Várunk a visszajelzésére
                    yield return WaitForEndOfAction();
                }

                currentAction = SkillEffectAction.None;
            }

            //Ha nem, akkor figyelmeztetjük és reseteljük a kártyát
            else
            {
                StartCoroutine(clientModule.DisplayNotification(dataModule.GetPlayerWithKey(currentKey).GetUsername() + ", ehhez nincs elég győztes lapod!"));
                yield return WaitForEndOfText();
                clientModule.ResetCardSkill(currentKey, cardPosition);
            }
        }
        //Képesség használata
        public void Use(int cardPosition)
        {
            this.skillModule.UseSkill(dataModule.GetPlayerWithKey(currentKey).GetCardsOnField()[cardPosition].GetCardID());
        }
        #endregion

        #region Commands

        //Felhúz a játékosok kezébe 4 lapot kezdésnél
        private IEnumerator DrawStarterCards()
        {
            for (var i = 0; i < 4; i++)
            {
                foreach (int key in dataModule.GetKeyList())
                {

                    DrawTheCard(key);
                    yield return new WaitForSeconds(GameSettings_Controller.drawTempo);
                }
            }

            ChangePhase(MainGameStates.SetRoles);
        }


        //Felhúz megadott számú lapot a megadott játékosnak
        private IEnumerator DrawCardsUp(int key, int amount = 1,  DrawTarget target = DrawTarget.Hand, DrawType drawType = DrawType.Normal, SkillState newState = SkillState.NotDecided)
        {
            for (var i = 0; i < amount; i++)
            {
                DrawTheCard(key, target, drawType, newState);
                yield return new WaitForSeconds(GameSettings_Controller.drawTempo);
            }

            ActionFinished();
        }

        //Kártya húzás: Modellből adatok lekérése és továbbítása a megfelelő játékos oldalnak.
        public void DrawTheCard(int key, DrawTarget target = DrawTarget.Hand, DrawType drawType = DrawType.Normal, SkillState newState = SkillState.NotDecided)
        {
            Card cardData = null;

            if (target == DrawTarget.Hand && !blindMatch)
            {
                cardData = dataModule.GetPlayerWithKey(key).DrawCardFromDeck();
            }

            else if (blindMatch || target == DrawTarget.Field)
            {
                cardData = dataModule.GetPlayerWithKey(key).BlindDraw();
            }

            //Ha a felhúzás sikeres
            if (cardData != null)
            {
                clientModule.DrawNewCard(cardData, key, dataModule.GetPlayerWithKey(key).GetPlayerStatus(), drawType, target, newState);

                //Ha a játékosról van szó továbbítjuk megjelenítésre a deck új méretét
                if (dataModule.GetPlayerWithKey(key).GetPlayerStatus())
                {
                    int remainingDeckSize = dataModule.GetPlayerWithKey(key).GetDeckSize();
                    clientModule.RefreshDeckSize(key, remainingDeckSize);
                }
            }

            //Ha vakharc esetén valaki már nem tud pakliból húzni: játék vége
            else if (cardData == null && blindMatch)
            {
                StartCoroutine(clientModule.DisplayNotification(dataModule.GetPlayerWithKey(key).GetUsername() + " paklija elfogyott!\nJáték vége!"));
                StartCoroutine(WaitForText());

                ChangePhase(MainGameStates.CreateResult);
            }



            //Amúgy addig megy a játék, amíg a kézből is elfogynak a lapok
        }

        private IEnumerator CardAmountCheck()
        {
            //Ha több mint 7 lap van a kezünkben, le kell dobni egy szabadon választottat.
            if(dataModule.GetCardsFromHand(currentKey).Count > 7)
            {
                currentAction = SkillEffectAction.TossCard;
                currentSelectionType = CardListTarget.Hand;
                skillModule.ChooseCard(currentKey);
                yield return new WaitForSeconds((GameSettings_Controller.drawTempo)/2);
                ActionFinished();  
            }

            else 
            {
                yield return new WaitForSeconds((GameSettings_Controller.drawTempo)/2);
                ActionFinished();    
            }
        }

        private void NewSkillCycle(int key)
        {
            storeCount = 0;
            clientModule.NewSkillCycle(key);
        }

        public void ShowStatBox()
        {
            StartCoroutine(clientModule.DisplayStatBox());
        }

        #endregion


        #region Tools

        //Várakozás, amíg az adott akció nem ér véget
        public IEnumerator WaitForEndOfAction()
        {
            actionFinished = false;

            while (!actionFinished)
            {
                yield return null;
            }
        }

        //Várakozás, amíg a skill nem végez
        public IEnumerator WaitForEndOfSkill()
        {
            skillFinished = false;

            while (!skillFinished)
            {
                yield return null;
            }
        }

        //Várakozás, amíg a játékos nem fejezi be a körét
        public IEnumerator WaitForTurnsEnd()
        {
            turnFinished = false;
            while (!turnFinished)
            {
                yield return null;
            }
        }

        //Várat, amíg az üzenet el nem tűnik
        public IEnumerator WaitForEndOfText()
        {
            while (displayedMessageStatus)
            {
                yield return null;
            }
        }

        private IEnumerator WaitForText()
        {
            yield return WaitForEndOfText();
        }

        public AI_Controller GetAImodule()
        {
            return this.AI_module;
        }

        public Skill_Controller GetSkillModule()
        {
            return this.skillModule;
        }

        public Data_Controller GetDataModule()
        {
            return this.dataModule;
        }

        public Client_Controller GetClientModule()
        {
            return this.clientModule;
        }

        public Input_Controller GetInputModule()
        {
            return this.inputModule;
        }

        public CardListTarget GetCurrentListType()
        {
            return currentSelectionType;
        }

        public void ActionFinished()
        {
            this.actionFinished = true;
        }

        public void TurnFinished()
        {
            this.turnFinished = true;
        }

        public IEnumerator SkillFinished()
        {
            SetCurrentAction(SkillEffectAction.None);
            yield return new WaitForSeconds(1f);
            this.skillFinished = true;
        }

        public int GetChoosenKey()
        {
            return this.choosenKey;
        }

        public void SetChoosenKey(int newKey)
        {
            this.choosenKey = newKey;
        }

        public bool GetActionStatus()
        {
            return this.actionFinished;
        }

        public void SetActiveCardID(int newID)
        {
            this.currentActiveCard = newID;
        }

        public int GetActiveCardID()
        {
            return this.currentActiveCard;
        }

        public SkillEffectAction GetCurrentAction()
        {
            return this.currentAction;
        }

        public void SetCurrentAction(SkillEffectAction newAction)
        {
            this.currentAction = newAction;
        }

        public int GetCurrentKey()
        {
            return this.currentKey;
        }

        public int GetStoreCount()
        {
            return this.storeCount;
        }

        public void SetSelectionAction(SkillEffectAction action)
        {
            currentAction = action;
        }

        public void SetSwitchType(CardListTarget type)
        {
            currentSelectionType = type;
        }

        public CardStatType GetActiveStat()
        {
            return this.currentStat;
        }

        public void SetActiveStat(CardStatType newStat)
        {
            this.currentStat = newStat;
        }

        public void SetMessageStatus(bool newStatus)
        {
            this.displayedMessageStatus = newStatus;
        }

        public void AddKeyToLateSkills(int key)
        {
            //Ha még nincs a kulcs közte, akkor hozzáadjuk
            if(!this.lateSkillKeys.Contains(key))
            {
                this.lateSkillKeys.Add(key);
            }  
        }

        public bool IsMessageOnScreen()
        {
            return this.displayedMessageStatus;
        }

        public bool IsTheActivePlayerHuman()
        {
            return dataModule.GetPlayerWithKey(currentKey).GetPlayerStatus();
        }

        public bool IsThisPlayerHuman(int playerKey)
        {
            return dataModule.GetPlayerWithKey(playerKey).GetPlayerStatus();
        }

        public void SetNewAttacker(int key)
        {
            this.changedOrder = true;
            this.newAttacker = key;
        }

        //Visszaadja, hogy van-e vesztesünk
        public bool DoWeHaveLosers()
        {
            if (dataModule.GetLostList(currentKey).Count > 0)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        public void SetBlindMatchState(bool newState)
        {
            this.blindMatch = newState;
        }

        //Visszaadja, hogy vannak-e jelenleg vesztes lapok az ellenfelek térfelein
        public bool IsThereOtherLostCards()
        {
            if (dataModule.GetOtherLosers(currentKey).Count > 0)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        private void ChangePhase(MainGameStates nextPhase)
        {
            currentPhase = nextPhase;
            phaseChange = true;
        }

        public MainGameStates GetGameState()
        {
            return currentPhase;
        }

        private bool CheckForWinners()
        {
            foreach (int key in dataModule.GetKeyList())
            {
                if (dataModule.GetPlayerWithKey(key).GetWinAmount() >= winsNeeded)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion


    }
}


