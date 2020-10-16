//A játék főbb fázisai, amiken a GameState vezérlő végigmegy
public enum MainGameStates
{
    SetupGame, Notifications, CreateOrder, StarterDraw, SetRoles, SetStat, SummonCard,
    RevealCards, QuickSkills, NormalSkills, CompareCards, CreateResult, 
    LateSkills, PutCardsAway, BlindMatch, AwardPlayers, EndGame
};

public enum ClientStates{ WaitingForTurn, WaitingForSummon, SummonFinished, WaitingForSkills, SkillStateFinished ,RoundEnded };

//A három fő harctípus, amelyek alapján a harc folyik épp
public enum CardStatType { NotDecided, Power, Intelligence, Reflex };

//Húzás jellemző: Milyen jellegű a kártyahúzás
public enum DrawType {Normal, Blind};
//Húzás jellemzők: A felhúzott lap hova kerül
public enum DrawTarget { Hand, Field };


//A lehetséges opciók a kártyák képességeiről
public enum SkillChoises { Use, Store, Pass };

//Kártya UI objektumok állapotai
public enum SkillState{NotDecided,Store,Use,Pass,Predetermined};

//Skillekhez kapcsolatos fázis változók
public enum SkillActivationTime {Quick, Normal, Late, Hand};
public enum SkillTriggerRequirement{
	None, MoreIntelligent, Lose, Attack, Defense, Sacrifice,
	SacrificeDoppelganger, HasDevilCrock, Draw, LessWinner,
	NumberOfOpponents};
public enum SkillEffectTarget { Self, Opponent, Everyone, Game };
public enum SkillEffectAction { 
	None, Store, Switch, Organise, SkillUse, Revive, BlindSwitch, 
	StatChange, NegateEffects, GreatFight, SacrificeFromHand, StatBonus, 
	TriggerBlindFight, MakeAttacker, Win, DrawCard, TossCard, Execute, CheckWinnerAmount,
	SwitchOpponentCard, PickCardForSwitch, SacrificeDoppelganger};
public enum MultipleSkillRule {None, And, Or};


//Kártyalistázási paraméterek
public enum CardListTarget { None, Deck, Hand, Winners, Losers, Field };
public enum CardListFilter { None, NoMasterCrok, EnemyDoppelganger };


//Menü fázisok
public enum MenuState { MainMenu, Settings, GameTypeSelect, PlayerNumberSelect, MultiplayerSettings, Login, Register };
public enum DexState { List, Detail };

//Játékos fázisok és helyzetek
public enum PlayerTurnStatus{ChooseCard, ChooseSkill, Finished};
public enum PlayerTurnResult{Win, Lose, Draw};
public enum PlayerTurnRole{Attacker, Defender};

