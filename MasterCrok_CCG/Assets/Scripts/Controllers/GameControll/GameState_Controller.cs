using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ClientControll;
using UnityEngine.SceneManagement;
using ClientSide;

namespace GameControll
{
    public class GameState_Controller : MonoBehaviour
    {
        [Header("Prefabok és referenciák")]
        public CardFactory factory;
        public Client client;

        //Vezérlő modulok
        private Module_Controller modules;

        //Játékbeli fázisok és események
        private bool phaseChange;
        private bool firstRound;
        private bool actionFinished;
        private bool displayedMessageStatus;
        private bool blindMatch;
        private bool turnFinished;
        private bool skillFinished;
        private bool changedOrder;
        private bool instantWin;
        private bool negatedSkills;
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

        //RNG generátor
        private System.Random rng;

        #region MonoBehaviour

        private void Awake()
        {
            //Modulok beállítása
            this.modules = new Module_Controller(this, factory, client);

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
            this.instantWin = false;
            this.negatedSkills = false;

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
            modules.GetDataModule().AddPlayer();
            modules.GetDataModule().GenerateOpponents();
            modules.GetClientModule().GenerateUI(modules.GetDataModule().GetNumberOfOpponents(), modules.GetDataModule().GetKeyList(), modules.GetDataModule().GetNameList(), modules.GetDataModule().GetPlayerAtIndex(0).GetDeckSize());
            ChangePhase(MainGameStates.CreateOrder);
        }

        //A játékos listában felállít egy sorrendet, amit követve jönnek a következő körben
        private void SetOrder(int winnerKey = -1)
        {
            modules.GetDataModule().CreateRandomOrder();

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
                    modules.GetDataModule().PutWinnerInFirstPlace(winnerKey);
                }

                ChangePhase(MainGameStates.SetRoles);
            }
        }

        //A kezdőkártyák felhúzása a kézbe
        private void DrawPhase()
        {
            //Paklik megkeverése
            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                modules.GetDataModule().ShuffleDeck(key);
            }

            //Kezdő 4 lap felhúzása
            StartCoroutine(DrawStarterCards());
        }

        //Beállítjuk a Támadó-Védő szerepeket
        //Mindig a player lista első eleme a Támadó, a többiek védők
        private void SetRoles()
        {
            bool firstPlayer = true;
            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                if (firstPlayer)
                {
                    firstPlayer = false;
                    modules.GetDataModule().GetPlayerWithKey(key).SetRole(PlayerTurnRole.Attacker);
                }

                else
                {
                    modules.GetDataModule().GetPlayerWithKey(key).SetRole(PlayerTurnRole.Defender);
                }

                modules.GetDataModule().GetPlayerWithKey(key).SetStatus(PlayerTurnStatus.ChooseCard);

                //A kézben lévő lapokra érvényesítjük a bónuszokat, így az a következő fázisban már látható hatással bír
                modules.GetDataModule().GetPlayerWithKey(key).ApplyFieldBonusToAll();

                modules.GetClientModule().StartOfRound();
            }
            ChangePhase(MainGameStates.SetStat);
        }

        //A támadó játékost megkérdezi, hogy milyen statot választ
        private IEnumerator SetStat()
        {
            currentKey = modules.GetDataModule().GetKeyList()[0];

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
            if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetPlayerStatus())
            {
                ShowStatBox();

                //Várunk, amíg befejezi a döntést
                yield return WaitForTurnsEnd();
            }

            //Ellenkező esetben az AI dönt
            else
            {
                //AI agy segítségét hívjuk a döntésben
                currentStat = Bot_Behaviour.ChooseFightType(modules.GetDataModule().GetPlayerWithKey(currentKey).GetCardsInHand());

                //Jelenítsük meg a változást
                modules.GetClientModule().RefreshStatDisplay();
            }

            ChangePhase(MainGameStates.SummonCard);

        }


        //Idézés fázis
        private IEnumerator Summon()
        {
            //Minden játékoson végigmegy
            foreach (int playerKey in modules.GetDataModule().GetKeyList())
            {
                currentKey = playerKey;

                //Ha a játékosnak már nincs idézhető lapja: játék vége
                if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetHandCount() == 0)
                {
                    StartCoroutine(modules.GetClientModule().DisplayNotification(modules.GetDataModule().GetPlayerWithKey(currentKey).GetUsername() + " kifogyott a lapokból!\nJáték vége!"));
                    yield return WaitForEndOfText();
                    ChangePhase(MainGameStates.CreateResult);
                }

                //Csak akkor kell kézből idézni, ha nincs vakharc
                if (!this.blindMatch)
                {
                    //Értesítést adunk a kör kezdetéről
                    StartCoroutine(modules.GetClientModule().DisplayNotification(modules.GetDataModule().GetPlayerWithKey(currentKey).GetUsername() + " következik!"));
                    yield return WaitForEndOfText();

                    modules.GetClientModule().WaitForSummon();

                    //A támadó az előző fázisban már húzott lapot, de a védők húzhatnak a kör elején
                    if(modules.GetDataModule().GetPlayerWithKey(currentKey).GetRole() == PlayerTurnRole.Defender)
                    {   
                        //Húzunk fel a játékosnak 1 lapot a kör elején
                        StartCoroutine(DrawCardsUp(currentKey));
                        yield return WaitForEndOfAction();
                        StartCoroutine(CardAmountCheck());
                        yield return WaitForEndOfAction();
                    }

                    //Ha a játékos státusza szerint a kártyahúzásra vár
                    if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetStatus() == PlayerTurnStatus.ChooseCard)
                    {

                        //Engedélyezzük neki a kártyák dragelését
                        modules.GetClientModule().SetDragStatus(currentKey, true);

                        //Ha ember, akkor várunk az idézésre
                        if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetPlayerStatus())
                        {
                            //Jelzünk a játékosnak
                            StartCoroutine(modules.GetClientModule().DisplayNotification("Rakj le egy kártyát, " + modules.GetDataModule().GetPlayerWithKey(currentKey).GetUsername()));
                            yield return WaitForEndOfText();

                            //Várunk a visszajelzésére
                            yield return WaitForTurnsEnd();
                        }

                        //Ellenkező esetben az AI-al rakatunk le kártyát
                        else
                        {
                            modules.GetAImodule().SummonCard(currentKey);

                            //Várunk az akció befejezésére
                            yield return WaitForTurnsEnd();
                        }
                    }
                }

                //Átállítjuk a játékos státuszát: A képesség kiválasztására vár
                modules.GetDataModule().GetPlayerWithKey(playerKey).SetStatus(PlayerTurnStatus.ChooseSkill);
            }

            ChangePhase(MainGameStates.RevealCards);
        }

        //Pályára rakott kártyák felfedése sorban
        private IEnumerator Reveal()
        {
            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                modules.GetClientModule().RevealCards(key);
                yield return new WaitForSeconds(GameSettings_Controller.drawTempo * 5);
            }

            ChangePhase(MainGameStates.QuickSkills);
        }

        private IEnumerator QuickSkills()
        {
            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                currentKey = key;
                int position = 0;
                foreach (Card card in modules.GetDataModule().GetCardsFromPlayer(currentKey, CardListTarget.Field)) 
                {
                    //Ha a megadott játékosnál van a mezőn gyors skill és még eldöntetlen/felhasználható
                    if(card.HasAQuickSkill() && modules.GetClientModule().AskCardSkillStatus(key, position) == SkillState.NotDecided)
                    {
                        currentActiveCard = position;
                        Use(position);
                        yield return WaitForEndOfSkill();
                    }
                    
                    position++;

                }

                if(negatedSkills)
                {
                    negatedSkills = false;
                    break;
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
                foreach (int playerKey in modules.GetDataModule().GetKeyList())
                {
                    currentKey = playerKey;

                    NewSkillCycle(currentKey);

                    modules.GetClientModule().WaitForSkill();

                    //Ha a játékos státusza szerint a skill eldöntésére vár
                    if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetStatus() == PlayerTurnStatus.ChooseSkill)
                    {

                        //Ha ember, akkor várunk a döntésre
                        if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetPlayerStatus())
                        {
                            //Jelezzük a játékosnak, hogy itt az idő dönteni a képességről
                            StartCoroutine(modules.GetClientModule().DisplayNotification("Dönts a kártyáid képességeiről!"));
                            yield return WaitForEndOfText();

                            //Megjelenítjük a kör vége gombot
                            modules.GetClientModule().SetEndTurnButton(true);

                            //Megadjuk a lehetőséget a játékosnak, hogy döntsön a képességekről most
                            modules.GetClientModule().SetSkillStatus(currentKey, true);

                            //Várunk a visszajelzésére
                            yield return WaitForTurnsEnd();

                            //Ha döntött, akkor elvesszük tőle a lehetőséget, hogy módosítson rajta
                            modules.GetClientModule().SetSkillStatus(currentKey, false);

                            //Az esetleges nem rendelkezett lapokat passzoljuk
                            modules.GetClientModule().HandleRemainingCards(currentKey);
                        }

                        //Ellenkező esetben az AI dönt a képességről
                        else
                        {
                            StartCoroutine(modules.GetAImodule().DecideSkill(currentKey));

                            //Nem megyünk tovább, amíg nem végez a döntéssel
                            yield return WaitForTurnsEnd();
                        }

                        SetCurrentAction(SkillEffectAction.None);
                    }

                    modules.GetClientModule().SkillStateEnded();

                    if(instantWin)
                    {
                        instantWin = false;
                        yield break;
                    }

                    //Ha az aktuális játékos végzett, növeljük a countert
                    if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetStatus() == PlayerTurnStatus.Finished)
                    {
                        playerMadeDecision++;
                    }

                }

                //Ha mindenki végzett, akkor loop vége
                if (playerMadeDecision == modules.GetDataModule().GetNumberOfOpponents() + 1)
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

            foreach (int key in modules.GetDataModule().GetKeyList())
            {

                //Hozzáadjuk minden mező értékét
                int playerFieldValue = modules.GetDataModule().GetPlayerWithKey(key).GetActiveCardsValue(currentStat);

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
                StartCoroutine(modules.GetClientModule().DisplayNotification("Döntetlen!"));
                yield return WaitForEndOfText();

                this.blindMatch = true;
            }

            //Ellenkező esetben eldönthető, hogy ki a győztes
            else
            {
                //Győzelmi üzenet megjelenítése
                StartCoroutine(modules.GetClientModule().DisplayNotification("A kör győztese: " + modules.GetDataModule().GetPlayerName(maxId)));
                yield return WaitForEndOfText();

                //Ha vakharcok voltak, akkor ezzel végetértek
                if (this.blindMatch)
                {
                    this.blindMatch = false;
                }
            }

            lastWinnerKey = maxId;

            //Győzelmi státuszok beállítása
            foreach (int key in modules.GetDataModule().GetKeyList())
            {

                //Ha az első helyezettével megegyezik a kulcs és nincs vakharc státusz
                if (key == lastWinnerKey && !blindMatch)
                {
                    modules.GetDataModule().GetPlayerWithKey(key).SetResult(PlayerTurnResult.Win);
                }

                //Ha vakharc lesz, akkor döntetlen volt az eredmény
                else if(blindMatch)
                {
                    modules.GetDataModule().GetPlayerWithKey(key).SetResult(PlayerTurnResult.Draw);
                }

                //Ellenkező esetben vesztes
                else
                {
                    modules.GetDataModule().GetPlayerWithKey(key).SetResult(PlayerTurnResult.Lose);
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
                foreach (Card card in modules.GetDataModule().GetCardsFromField(currentKey)) 
                {
                    //Ha a megadott játékosnál van a mezőn gyors skill és még eldöntetlen/felhasználható
                    if(card.HasALateSkill() && modules.GetClientModule().AskCardSkillStatus(key, position) == SkillState.Use)
                    {
                        currentActiveCard = position;

                        //Használjuk a késői képességet
                        Use(position);
                        yield return WaitForEndOfSkill();

                        StartCoroutine(modules.GetClientModule().DisplayNotification(modules.GetDataModule().GetPlayerName(key) +" késői képességet használt!"));
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
            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                //Ha az első helyezettével megegyezik a kulcs, akkor győztes
                if (modules.GetDataModule().GetPlayerWithKey(key).GetResult() == PlayerTurnResult.Win)
                {
                    //Lapok elrakása a győztesek közé a UI-ban
                    StartCoroutine(modules.GetClientModule().PutCardsAway(key, true));
                }

                //Ha nem győztes, akkor vesztes.
                //*Surprised Pikachu fej*
                else
                {
                    //Lapok elrakása a vesztesek közé a UI-ban
                    StartCoroutine(modules.GetClientModule().PutCardsAway(key, false));
                    
                }
                yield return WaitForEndOfAction();

                //Player modell frissítése, aktív kártya field elemeinek elrakása a győztes vagy vesztes tárolóba
                modules.GetDataModule().GetPlayerWithKey(key).PutActiveCardAway();

                //Továbblépés előtt nullázzuk a kézben lévő lapok bónuszait, hogy tiszta lappal kezdődjön a következő kör
                //Hehe, érted, tiszta LAPPAL, mert ez egy kártyajáték :(( 
                //Please end my suffer, csak a fránya diplomámat akarom
                modules.GetDataModule().GetPlayerWithKey(key).ResetBonuses();

                //Csökkentjük a bónuszok számlálóját és megválunk a lejárt bónuszoktól
                modules.GetDataModule().GetPlayerWithKey(key).UpdateBonuses();

                //Jelezzük a kliens felé a kör végét.
                modules.GetClientModule().EndOfRound();
                
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
                    changedOrder = false;
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
            StartCoroutine(modules.GetClientModule().DisplayNotification("Vakharc következik!"));
            yield return WaitForEndOfText();

            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                //A pakli felső lapját lerakjuk
                StartCoroutine(DrawCardsUp(key, 1, DrawTarget.Field, DrawType.Blind, SkillState.NotDecided));
            }

            //Ha nincs sorrend változtatás
            if(!changedOrder)
            {
                //Az előző körben lévő utolsó védő lesz a következő támadó
                SetOrder(modules.GetDataModule().GetKeyList()[modules.GetDataModule().GetKeyList().Count - 1]);
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

            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                if (modules.GetDataModule().GetPlayerWithKey(key).GetWinAmount() > maxWin)
                {
                    winnerCardCount.Add(modules.GetDataModule().GetPlayerWithKey(key).GetWinAmount());
                    maxWin = modules.GetDataModule().GetPlayerWithKey(key).GetWinAmount();
                    maxKey = key;
                }
            }

            yield return new WaitForSeconds(0.1f);

            //Ha van holtverseny
            if (winnerCardCount.Count(p => p == maxWin) > 1)
            {
                Notification_Controller.DisplayNotification("Játék Vége!\nAz eredmény döntetlen!", ExitGame);
            }

            //Ha egyértelmű a győzelem
            else
            {
                int reward = 0;

                switch (GameObject.Find("GameData_Controller").GetComponent<GameSettings_Controller>().GetOpponents()) 
                {
                    case 1: reward = 50; break;
                    case 2: reward = 200; break;
                    case 3: reward = 500; break;
                    default: break;
                }

                GameObject.Find("Profile_Controller").GetComponent<Profile_Controller>().GetActivePlayer().AddCoins(reward);
                Profile_Controller.settingsState = ProfileSettings.Silent;
                GameObject.Find("Profile_Controller").GetComponent<Profile_Controller>().SaveProfile();


                Notification_Controller.DisplayNotification($"Játék Vége!\nA győztes: {modules.GetDataModule().GetPlayerWithKey(maxKey).GetUsername()}", ExitGame);
            }
        }
        #endregion

        #region Skill Decisions

        //Képesség passzolása
        public void Pass()
        {
            StartCoroutine(modules.GetClientModule().DisplayNotification(modules.GetDataModule().GetPlayerWithKey(currentKey).GetUsername() + " passzolta a képességet!"));
            StartCoroutine(WaitForText());
            StartCoroutine(SkillFinished());
            //modules.GetDataModule().GetPlayerWithKey(currentKey).SetStatus(PlayerTurnStatus.Finished);
        }
        //Képesség tartalékolása
        public IEnumerator Store(int cardPosition)
        {
            //Először megnézzük, hogy tud-e áldozni
            if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetWinAmount() > 0)
            {
                storeCount++;

                currentAction = SkillEffectAction.Store;

                //Ha ember döntött úgy, hogy store-olja a képességet
                if (modules.GetDataModule().GetPlayerWithKey(currentKey).GetPlayerStatus())
                {
                    string msg = "Válaszd ki, hogy melyik győztest áldozod fel!";
                    modules.GetClientModule().CardChoice(modules.GetDataModule().GetCardsFromPlayer(currentKey, CardListTarget.Winners), currentAction, currentKey, msg);

                    //Várunk a visszajelzésére
                    yield return WaitForEndOfAction();
                }

                currentAction = SkillEffectAction.None;
            }

            //Ha nem, akkor figyelmeztetjük és reseteljük a kártyát
            else
            {
                StartCoroutine(modules.GetClientModule().DisplayNotification(modules.GetDataModule().GetPlayerWithKey(currentKey).GetUsername() + ", ehhez nincs elég győztes lapod!"));
                yield return WaitForEndOfText();
                modules.GetClientModule().ResetCardSkill(currentKey, cardPosition);
            }
        }
        //Képesség használata
        public void Use(int cardPosition)
        {
            modules.GetSkillModule().UseSkill(modules.GetDataModule().GetPlayerWithKey(currentKey).GetCardsOnField()[cardPosition].GetCardID());
        }
        #endregion

        #region Commands

        public void ExitGame()
        {
            modules.GetDataModule().ResetDecks();
            SceneManager.LoadScene("Main_Menu");

        }

        //Felhúz a játékosok kezébe 4 lapot kezdésnél
        private IEnumerator DrawStarterCards()
        {
            for (var i = 0; i < 4; i++)
            {
                foreach (int key in modules.GetDataModule().GetKeyList())
                {

                    DrawTheCard(key);
                    yield return new WaitForSeconds(GameSettings_Controller.drawTempo);
                }
            }

            ChangePhase(MainGameStates.SetRoles);
        }


        //Felhúz megadott számú lapot a megadott játékosnak
        public IEnumerator DrawCardsUp(int key, int amount = 1,  DrawTarget target = DrawTarget.Hand, DrawType drawType = DrawType.Normal, SkillState newState = SkillState.NotDecided)
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
                cardData = modules.GetDataModule().GetPlayerWithKey(key).DrawCardFromDeck();
            }

            else if (blindMatch || target == DrawTarget.Field)
            {
                cardData = modules.GetDataModule().GetPlayerWithKey(key).BlindDraw();
            }

            //Ha a felhúzás sikeres
            if (cardData != null)
            {
                modules.GetClientModule().DrawNewCard(cardData, key, modules.GetDataModule().GetPlayerWithKey(key).GetPlayerStatus(), drawType, target, newState);

                //Ha a játékosról van szó továbbítjuk megjelenítésre a deck új méretét
                if (modules.GetDataModule().GetPlayerWithKey(key).GetPlayerStatus())
                {
                    int remainingDeckSize = modules.GetDataModule().GetPlayerWithKey(key).GetDeckSize();
                    modules.GetClientModule().RefreshDeckSize(key, remainingDeckSize);
                }
            }

            //Ha vakharc esetén valaki már nem tud pakliból húzni: játék vége
            else if (cardData == null && blindMatch)
            {
                StartCoroutine(modules.GetClientModule().DisplayNotification(modules.GetDataModule().GetPlayerWithKey(key).GetUsername() + " paklija elfogyott!\nJáték vége!"));
                StartCoroutine(WaitForText());

                ChangePhase(MainGameStates.CreateResult);
            }



            //Amúgy addig megy a játék, amíg a kézből is elfogynak a lapok
        }

        private IEnumerator CardAmountCheck()
        {
            //Ha több mint 7 lap van a kezünkben, le kell dobni egy szabadon választottat.
            if(modules.GetDataModule().GetCardsFromPlayer(currentKey, CardListTarget.Hand).Count > 7)
            {
                currentAction = SkillEffectAction.TossCard;
                currentSelectionType = CardListTarget.Hand;

                modules.GetSkillModule().MakePlayerChooseCard();
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
            modules.GetClientModule().NewSkillCycle(key);
        }

        public void ShowStatBox()
        {
            StartCoroutine(modules.GetClientModule().DisplayStatBox());
        }

        public void MakeWinner(int selectedKey)
        {
            //Győzelmi státuszok beállítása
            foreach (int key in modules.GetDataModule().GetKeyList())
            {

                //Ha az első helyezettével megegyezik a kulcs és nincs vakharc státusz
                if (key == selectedKey)
                {
                    modules.GetDataModule().GetPlayerWithKey(key).SetResult(PlayerTurnResult.Win);
                }

                //Ellenkező esetben vesztes
                else
                {
                    modules.GetDataModule().GetPlayerWithKey(key).SetResult(PlayerTurnResult.Lose);
                }
            }

            if(blindMatch)
            {
                blindMatch = false;
            }
            lastWinnerKey = selectedKey;
            this.instantWin = true;
            TurnFinished();
            ChangePhase(MainGameStates.PutCardsAway);
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
            return modules.GetDataModule().GetPlayerWithKey(currentKey).GetPlayerStatus();
        }

        public bool IsThisPlayerHuman(int playerKey)
        {
            return modules.GetDataModule().GetPlayerWithKey(playerKey).GetPlayerStatus();
        }

        public void SetNewAttacker(int key)
        {
            this.changedOrder = true;
            this.newAttacker = key;
        }

        //Visszaadja, hogy van-e vesztesünk
        public bool DoWeHaveLosers()
        {
            if (modules.GetDataModule().GetCardsFromPlayer(currentKey, CardListTarget.Losers).Count > 0)
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

        public void SetNegationStatus(bool newState)
        {
            this.negatedSkills = newState;
        }

        //Visszaadja, hogy vannak-e jelenleg vesztes lapok az ellenfelek térfelein
        public bool IsThereOtherLostCards()
        {
            if (modules.GetDataModule().GetOtherLosers(currentKey).Count > 0)
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
            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                if (modules.GetDataModule().GetPlayerWithKey(key).GetWinAmount() >= GameSettings_Controller.winsNeeded)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion



    }
}


