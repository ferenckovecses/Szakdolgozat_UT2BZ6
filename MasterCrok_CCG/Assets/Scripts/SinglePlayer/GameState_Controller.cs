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
        private int storeCount;
        private int currentKey;
        private int currentActiveCard;

        private MainGameStates currentPhase;
        private CardStatType currentStat;
        private SkillEffectAction currentAction;
        private CardListTarget currentSelectionType;

        //Játék győzelmi paraméterei, TODO: Módosítható
        public static int winsNeeded = 5;


        //RNG generátor
        private System.Random rng;

        #region MonoBehaviour

        private void Awake()
        {
            //Modulok beállítása
            dataModule = new Data_Controller(this, factory);
            inputModule = new Input_Controller(this);
            clientModule = new Client_Controller(this, client);
            skillModule = new Skill_Controller(this);
            AI_module = new AI_Controller(this);

            //Játékfázisok és státuszok alapértékezése
            currentPhase = MainGameStates.SetupGame;
            currentStat = CardStatType.NotDecided;
            currentAction = SkillEffectAction.None;
            currentSelectionType = CardListTarget.None;
            currentActiveCard = -1;
            phaseChange = true;
            firstRound = true;
            displayedMessageStatus = false;
            blindMatch = false;

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
                    case MainGameStates.SummonCard: StartCoroutine(Summon()); break;
                    case MainGameStates.RevealCards: StartCoroutine(Reveal()); break;
                    case MainGameStates.NormalSkills: StartCoroutine(Skill()); break;
                    case MainGameStates.CompareCards: StartCoroutine(Compare()); break;
                    case MainGameStates.BlindMatch: StartCoroutine(BlindMatch()); break;
                    case MainGameStates.CreateResult: Result(); break;
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
            clientModule.GenerateUI(dataModule.GetNumberOfOpponents(), dataModule.GetKeyList(), dataModule.GetPlayerAtIndex(0).GetDeckSize());
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

                //Ezekre csak akkor van szükség, ha nincs vakharc
                if (!this.blindMatch)
                {
                    //Értesítést adunk a kör kezdetéről
                    StartCoroutine(clientModule.DisplayNotification(dataModule.GetPlayerWithKey(playerKey).GetUsername() + " következik!"));

                    //Húzunk fel a játékosnak 1 lapot a kör elején
                    StartCoroutine(DrawCardsUp(playerKey));
                }

                //Ha a játékos támadó, akkor döntenie kell a harctípusról
                if (dataModule.GetPlayerWithKey(playerKey).GetRole() == PlayerTurnRole.Attacker)
                {
                    //Ha a játékosnak már nincs idézhető lapja: játék vége
                    if (dataModule.GetPlayerWithKey(playerKey).GetHandCount() == 0)
                    {
                        StartCoroutine(clientModule.DisplayNotification(dataModule.GetPlayerWithKey(playerKey).GetUsername() + " kifogyott a lapokból!\nJáték vége!"));
                        currentPhase = MainGameStates.CreateResult;
                        phaseChange = true;
                    }

                    //Ha ember, akkor dobjuk fel neki az értékválasztó képernyőt
                    if (dataModule.GetPlayerWithKey(playerKey).GetPlayerStatus())
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
                        clientModule.RefreshStatDisplay();
                    }
                }

                //Csak akkor kell kézből idézni, ha nincs vakharc
                if (!this.blindMatch)
                {
                    //Ha a játékos státusza szerint a kártyahúzásra vár
                    if (dataModule.GetPlayerWithKey(playerKey).GetStatus() == PlayerTurnStatus.ChooseCard)
                    {

                        //Engedélyezzük neki a kártyák dragelését
                        clientModule.SetDragStatus(playerKey, true);

                        //Ha ember, akkor várunk az idézésre
                        if (dataModule.GetPlayerWithKey(playerKey).GetPlayerStatus())
                        {
                            //Jelzünk a játékosnak
                            StartCoroutine(clientModule.DisplayNotification("Rakj le egy kártyát, " + dataModule.GetPlayerWithKey(playerKey).GetUsername()));

                            //Várunk a visszajelzésére
                            yield return WaitForEndOfAction();
                        }

                        //Ellenkező esetben az AI-al rakatunk le kártyát
                        else
                        {
                            //AI agy segítségét hívjuk a döntésben
                            int index = Bot_Behaviour.ChooseRightCard(dataModule.GetPlayerWithKey(playerKey).GetCardsInHand(), currentStat);

                            dataModule.GetPlayerWithKey(playerKey).PlayCardFromHand(index);

                            yield return new WaitForSeconds(GameSettings_Controller.drawTempo * UnityEngine.Random.Range(15, 30));
                            clientModule.SummonCard(playerKey, index);
                        }

                        //Visszavesszük tőle az opciót a kártya lerakásra
                        clientModule.SetDragStatus(playerKey, false);
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
            ChangePhase(MainGameStates.NormalSkills);
        }

        //Skill fázis: Körbemegyünk és mindenkit megkérdezünk, hogy mit akar a képességével kezdeni
        private IEnumerator Skill()
        {
            //Ciklusváltozó a while-hoz: Akkor lesz true, ha mindenki döntött a képességéről
            bool everyoneDecided = false;

            //Ha mindenki döntött, akkor break
            int playerMadeDecision;

            //Addig megyünk, amíg mindenki el nem használja vagy passzolja a képességét
            while (!everyoneDecided)
            {
                playerMadeDecision = 0;
                foreach (int playerKey in dataModule.GetKeyList())
                {
                    currentKey = playerKey;

                    NewSkillCycle(playerKey);
                    //Ha a játékos státusza szerint a skill eldöntésére vár
                    if (dataModule.GetPlayerWithKey(playerKey).GetStatus() == PlayerTurnStatus.ChooseSkill)
                    {

                        //Ha ember, akkor várunk a döntésre
                        if (dataModule.GetPlayerWithKey(playerKey).GetPlayerStatus())
                        {
                            //Jelezzük a játékosnak, hogy itt az idő dönteni a képességről
                            StartCoroutine(clientModule.DisplayNotification("Dönts a kártyáid képességeiről!"));

                            //Megadjuk a lehetőséget a játékosnak, hogy döntsön a képességekről most
                            clientModule.SetSkillStatus(playerKey, true);

                            //Várunk a visszajelzésére
                            yield return WaitForEndOfAction();

                            //Ha döntött, akkor elvesszük tőle a lehetőséget, hogy módosítson rajta
                            clientModule.SetSkillStatus(playerKey, false);
                        }

                        //Ellenkező esetben az AI dönt a képességről
                        else
                        {
                            StartCoroutine(AI_module.DecideSkill());

                            //Nem megyünk tovább, amíg nem végez a döntéssel
                            yield return WaitForEndOfAction();
                        }
                    }

                    //Ha az aktuális játékos végzett, növeljük a countert
                    if (dataModule.GetPlayerWithKey(playerKey).GetStatus() == PlayerTurnStatus.Finished)
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

            //Adunk időt az üzeneteknek, illetve a játékosoknak hogy megnézzék a pályát
            yield return new WaitForSeconds(GameSettings_Controller.drawTempo * 10);

            //Kiértékeléshez szükséges változók
            List<int> values = new List<int>();
            int max = 0;
            int maxId = -1;

            foreach (int key in dataModule.GetKeyList())
            {

                //Hozzáadjuk minden mező értékét
                values.Add(dataModule.GetPlayerWithKey(key).GetActiveCardsValue(currentStat));


                //Eltároljuk a legnagyobb értéket menet közben
                if (dataModule.GetPlayerWithKey(key).GetActiveCardsValue(currentStat) > max)
                {
                    max = dataModule.GetPlayerWithKey(key).GetActiveCardsValue(currentStat);
                    maxId = key;
                }
            }

            //Ha több ember is rendelkezik a max value-val: Vakharc
            if (values.Count(p => p == max) > 1)
            {
                StartCoroutine(clientModule.DisplayNotification("Döntetlen!\nVakharc következik!"));
                this.blindMatch = true;
                currentPhase = MainGameStates.BlindMatch;
                phaseChange = true;
            }

            //Ellenkező esetben eldönthető, hogy ki a győztes
            else
            {
                //Győzelmi üzenet megjelenítése
                StartCoroutine(clientModule.DisplayNotification("A kör győztese: " + dataModule.GetPlayerWithKey(maxId).GetUsername()));

                //Ha vakharcok voltak, akkor ezzel végetértek
                if (this.blindMatch)
                {
                    blindMatch = false;
                }

                foreach (int key in dataModule.GetKeyList())
                {
                    //Ha az első helyezettével megegyezik a kulcs
                    if (key == maxId)
                    {

                        dataModule.GetPlayerWithKey(key).SetResult(PlayerTurnResult.Win);

                        //Lapok elrakása a győztesek közé a UI-ban
                        StartCoroutine(clientModule.PutCardsAway(key, true));
                    }

                    //Ellenkező esetben vesztes
                    else
                    {
                        dataModule.GetPlayerWithKey(key).SetResult(PlayerTurnResult.Lose);

                        //Lapok elrakása a vesztesek közé a UI-ban
                        StartCoroutine(clientModule.PutCardsAway(key, false));
                    }

                    //Player modell frissítése, aktív mező elemeinek elrakása a győztes vagy vesztes tárolóba
                    dataModule.GetPlayerWithKey(key).PutActiveCardAway();
                }

                yield return new WaitForSeconds(GameSettings_Controller.drawTempo * 5f);

                //Megnézzük, hogy van-e a győzelmi feltételeknek megfelelő játékos
                if (CheckForWinners())
                {
                    currentPhase = MainGameStates.CreateResult;
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
                StartCoroutine(clientModule.PutCardsAway(key, false));
                dataModule.GetPlayerWithKey(key).PutActiveCardAway();

                yield return new WaitForSeconds(1f);

                //A pakli felső lapját lerakjuk
                StartCoroutine(DrawCardsUp(key));

            }

            //Megnézzük, hogy van-e a győzelmi feltételeknek megfelelő játékos
            if (CheckForWinners())
            {
                currentPhase = MainGameStates.CreateResult;
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
        }
        #endregion

        #region Skill Decisions

        //Képesség passzolása
        public void Pass()
        {
            StartCoroutine(clientModule.DisplayNotification(dataModule.GetPlayerWithKey(currentKey).GetUsername() + " passzolta a képességet!"));
            dataModule.GetPlayerWithKey(currentKey).SetStatus(PlayerTurnStatus.Finished);
            actionFinished = true;
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
                    clientModule.CardChoice(dataModule.GetWinnerList(currentKey), currentAction);

                    //Várunk a visszajelzésére
                    yield return WaitForEndOfAction();
                }

                currentAction = SkillEffectAction.None;
                actionFinished = true;
            }

            //Ha nem, akkor figyelmeztetjük és reseteljük a kártyát
            else
            {
                StartCoroutine(clientModule.DisplayNotification(dataModule.GetPlayerWithKey(currentKey).GetUsername() + ", ehhez nincs elég győztes lapod!"));
                clientModule.ResetCardSkill(currentKey, cardPosition);
            }
        }
        //Képesség használata
        public void Use(int cardPosition)
        {
            this.skillModule.UseSkill(dataModule.GetPlayerWithKey(currentKey).GetCardsOnField()[cardPosition].GetCardID());
        }
        #endregion

        #region Skill Action

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
        private IEnumerator DrawCardsUp(int key, int amount = 1)
        {
            for (var i = 0; i < amount; i++)
            {
                DrawTheCard(key);
                yield return new WaitForSeconds(GameSettings_Controller.drawTempo);
            }
        }

        //Kártya húzás: Modellből adatok lekérése és továbbítása a megfelelő játékos oldalnak.
        public void DrawTheCard(int key, DrawTarget target = DrawTarget.Hand)
        {
            Card cardData = null;

            if (target == DrawTarget.Hand && !blindMatch)
            {
                cardData = dataModule.GetPlayerWithKey(key).DrawCardFromDeck();
            }

            else if (blindMatch || target == DrawTarget.Field)
            {
                cardData = dataModule.GetPlayerWithKey(key).BlindDraw();
                Debug.Log(cardData);
            }

            //Ha a felhúzás sikeres
            if (cardData != null)
            {
                client.DisplayNewCard(cardData, key, dataModule.GetPlayerWithKey(key).GetPlayerStatus(), this.blindMatch, target);

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
                currentPhase = MainGameStates.CreateResult;
                phaseChange = true;
            }

            //Amúgy addig megy a játék, amíg a kézből is elfogynak a lapok
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

        //Várakozás, amíg a játékos nem ad inputot
        public IEnumerator WaitForEndOfAction()
        {
            actionFinished = false;

            while (!actionFinished)
            {
                yield return null;
            }
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
            if (dataModule.GetLostList(currentKey).Count > 0)
            {
                return true;
            }

            else
            {
                return false;
            }
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

        private bool CheckForWinners()
        {
            foreach (int key in dataModule.GetKeyList())
            {
                if (dataModule.GetPlayerWithKey(key).GetWinAmount() == winsNeeded)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion


    }
}


