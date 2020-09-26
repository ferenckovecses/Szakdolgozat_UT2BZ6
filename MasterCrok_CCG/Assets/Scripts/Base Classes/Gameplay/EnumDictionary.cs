//A játék főbb fázisai, amiken a GameState vezérlő végigmegy
public enum MainGameStates
{
    SetupGame, CreateOrder, StarterDraw, SetRoles, SummonCard,
    RevealCards, QuickSkills, NormalSkills, CompareCards, CreateResult, 
    LateSkills, PutCardsAway, BlindMatch, AwardPlayers, EndGame
};
//A három fő harctípus, amelyek alapján a harc folyik épp
public enum CardStatType { NotDecided, Power, Intelligence, Reflex };

//Húzás jellemző: Milyen jellegű a kártyahúzás
public enum DrawType {Normal, Blind};
//Húzás jellemzők: A felhúzott lap hova kerül
public enum DrawTarget { Hand, Field };


//A lehetséges opciók a kártyák képességeiről
public enum SkillChoises { Use, Store, Pass };

//Skillekhez kapcsolatos fázis változók
public enum SkillEffectTarget { Self, Opponent, Everyone };
public enum CardListTarget { None, Deck, Hand, Winners, Losers, Field };
public enum CardListFilter { None, NoMasterCrok };
public enum SkillEffectAction { None, Store, Switch, Organise, SkillUse, Revive, BlindSwitch };

public enum MenuState { MainMenu, Settings, GameTypeSelect, PlayerNumberSelect, MultiplayerSettings, Login, Register };
public enum DexState { List, Detail };