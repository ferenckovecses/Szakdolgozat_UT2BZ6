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
        private CardStatType currentStat;
        private SkillEffectAction currentAction;
        private CardListTarget currentSelectionType;

        public static bool readyForAction = false;

        //Gyakori interakciók
        private Interactions interactions;

        //Fázisok
        private OrderChangeState orderChangeState;
        private PreparationState preparationState;
        private StarterDrawState starterDrawState;
        private SettingRoleState settingRoleState;
        private SelectFightTypeState selectFightTypeState;
        private SummonState summonState;
        private RevealCardsState revealCardsState;
        private QuickSkillState quickSkillState;
        private MainSkillState mainSkillState;
        private CompareState compareState;
        private LateSkillState lateSkillState;
        private CardPutAwayState cardPutAwayState;
        private BlindMatchState blindMatchState;
        private ResultState resultState;

        //RNG generátor
        private System.Random rng;

        #region MonoBehaviour

        private void Awake()
        {
            //Modulok beállítása
            this.modules = Module_Controller.CreateModuleController(this, factory, client);

            //Interakciók beállítása
            this.interactions = new Interactions(this, modules);

            //Játékfázisok beállítása
            this.orderChangeState = new OrderChangeState(modules, this, interactions);
            this.preparationState = new PreparationState(modules, this, interactions);
            this.starterDrawState = new StarterDrawState(modules, this, interactions);
            this.settingRoleState = new SettingRoleState(modules, this, interactions);
            this.selectFightTypeState = new SelectFightTypeState(modules, this, interactions);
            this.summonState = new SummonState(modules, this, interactions);
            this.revealCardsState = new RevealCardsState(modules, this, interactions);
            this.quickSkillState = new QuickSkillState(modules, this, interactions);
            this.mainSkillState = new MainSkillState(modules, this, interactions);
            this.compareState = new CompareState(modules, this, interactions);
            this.lateSkillState = new LateSkillState(modules, this, interactions);
            this.cardPutAwayState = new CardPutAwayState(modules, this, interactions);
            this.blindMatchState = new BlindMatchState(modules, this, interactions);
            this.resultState = new ResultState(modules, this, interactions);

            //Játékfázisok és státuszok alapértékezése
            this.currentPhase = MainGameStates.SetupGame;
            this.currentStat = CardStatType.NotDecided;
            this.currentAction = SkillEffectAction.None;
            this.currentSelectionType = CardListTarget.None;
            this.currentKey = -1;
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
                    case MainGameStates.SetupGame: preparationState.Init(); break;
                    case MainGameStates.CreateOrder: orderChangeState.Init(); break;
                    case MainGameStates.StarterDraw: starterDrawState.Init(); break;
                    case MainGameStates.SetRoles: settingRoleState.Init(); break;
                    case MainGameStates.SetStat: selectFightTypeState.Init(); break;
                    case MainGameStates.SummonCard: summonState.Init(); break;
                    case MainGameStates.RevealCards: revealCardsState.Init(); break;
                    case MainGameStates.QuickSkills: quickSkillState.Init(); break;
                    case MainGameStates.NormalSkills: mainSkillState.Init(); break;
                    case MainGameStates.CompareCards: compareState.Init(); break;
                    case MainGameStates.LateSkills: lateSkillState.Init(); break;
                    case MainGameStates.PutCardsAway: cardPutAwayState.Init(); break;
                    case MainGameStates.BlindMatch: blindMatchState.Init(); break;
                    case MainGameStates.CreateResult: resultState.Init(); break;
                    default: break;
                }
            }
        }

        #endregion

        #region Skill Decisions

        //Képesség passzolása
        public void Pass()
        {
            StartCoroutine(modules.GetClientModule().DisplayNotification(modules.GetDataModule().GetPlayerWithKey(currentKey).GetUsername() + " passzolta a képességet!"));
            StartCoroutine(WaitForText());
            SkillFinished();
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
            Module_Controller.instance = null;
            SceneManager.LoadScene("Main_Menu");

        }

        //Felhúz a játékosok kezébe 4 lapot kezdésnél
        public IEnumerator DrawStarterCards()
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

        public IEnumerator CardAmountCheck()
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

        public void NewSkillCycle(int key)
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

        public void SkillFinished()
        {
            SetCurrentAction(SkillEffectAction.None);
            this.skillFinished = true;
        }

        public bool GetSkillWaitStatus()
        {
            return this.skillFinished;
        }

        public bool GetTurnWaitStatus()
        {
            return this.turnFinished;
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

        public List<int> GetLateSkillKeys()
        {
            return this.lateSkillKeys;
        }

        public void AddKeyToLateSkills(int key)
        {
            //Ha még nincs a kulcs közte, akkor hozzáadjuk
            if(!this.lateSkillKeys.Contains(key))
            {
                this.lateSkillKeys.Add(key);
            }  
        }

        public void ClearLateSkills()
        {
            this.lateSkillKeys.Clear();
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

        public int GetNewAttacker()
        {
            return this.newAttacker;
        }

        public void HandleOrderChange()
        {
            if(changedOrder)
            {
                lastWinnerKey = newAttacker;
            }
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

        public bool GetBlindMatchState()
        {
            return this.blindMatch;
        }

        public void SetNegationStatus(bool newState)
        {
            this.negatedSkills = newState;
        }

        public bool GetNegationStatus()
        {
            return this.negatedSkills;
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

        public void ChangePhase(MainGameStates nextPhase)
        {
            GameState_Controller.readyForAction = false;
            currentPhase = nextPhase;
            phaseChange = true;
        }

        public MainGameStates GetGameState()
        {
            return currentPhase;
        }

        public bool CheckForWinners()
        {
            foreach (int key in modules.GetDataModule().GetKeyList())
            {
                List<Card> list = modules.GetDataModule().GetPlayerWithKey(key).GetWinners();

                int res = (from s in list
                        group s by new {s = s.GetCardType()} into g
                        select new {value = g.Distinct().Count()}).Count();

                if (res >= GameSettings_Controller.winsNeeded)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetCurrentKey(int newKey)
        {
            this.currentKey = newKey;
            GameState_Controller.readyForAction = true;
        }

        public bool GetFirstRoundStatus()
        {
            return this.firstRound;
        }

        public void SetFirstRoundStatus(bool newStatus)
        {
            this.firstRound = newStatus;
        }

        public int GetCurrentWinnerKey()
        {
            return this.lastWinnerKey;
        }

        public void SetCurrentWinnerKey(int newKey)
        {
            this.lastWinnerKey = newKey;
        }

        public bool GetInstantWinStatus()
        {
            return this.instantWin;
        }

        public void SetInstantWinStatus(bool newStatus)
        {
            this.instantWin = newStatus;
        }

        public bool GetOrderChangeStatus()
        {
            return this.changedOrder;
        }

        public void SetOrderChangeStatus(bool newStatus)
        {
            this.changedOrder = newStatus;
        }

        #endregion



    }
}