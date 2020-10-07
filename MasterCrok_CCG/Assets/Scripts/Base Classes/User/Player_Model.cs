using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
Feladata: A játék, illetve a játékos adatmodellje. Játék indulásakor a játékos játékprofilja betöltődik a player változóba,
majd a játékvezérlőtől kap utasításokat (felhúzás, lerakás, státuszok változatatása)
*/

public enum PlayerTurnStatus{ChooseCard, ChooseSkill, Finished};
public enum PlayerTurnResult{Win, Lose, Draw};
public enum PlayerTurnRole{Attacker, Defender};

public class Player_Model
{
	//A játékos a pozícióban
	private Player player;

	//A játékban használt lapok tárolói
	private List<Card> cardsInHand;
	private List<Card> winnerCards;
	private List<Card> lostCards;

	//A kijátszott kártya helye
	private List<Card> cardsOnField;

	//Értékmódosítók
	private List<StatBonus> statBonus;

	//Játék közbeni státuszok tárolói
	private PlayerTurnStatus playerStatus;
	private PlayerTurnResult playerResult;
	private PlayerTurnRole playerRole;
	
	//Kiértékelés utáni/következő körre ható aktivált kártya hatás jelzése
	private bool hasLateEffect;

	//A model egy játékoshoz tartozik-e
	private bool isThePlayer;

	public Player_Model(Player player, bool playerStatus = false)
	{
		this.player = player;
		this.cardsInHand = new List<Card>();
		this.winnerCards = new List<Card>();
		this.lostCards = new List<Card>();
		this.statBonus = new List<StatBonus>();
		this.cardsOnField = new List<Card>();
		isThePlayer = playerStatus;
		hasLateEffect = false;
	}

	//Új stat bónuszt adunk a játékosnak
	public void AddBonus(StatBonus newBonus)
	{
		statBonus.Add(newBonus);
	}

	public List<StatBonus> GetStatBonuses()
	{
		return this.statBonus;
	}

	public void UpdateStatBonuses()
	{
		//A bónuszok időtartamát csökkentjük
		foreach (StatBonus bonus in GetStatBonuses()) 
		{
			bonus.DecreaseDuration();
		}

		//A lejárt bónuszokat eltávolítjuk
		statBonus.RemoveAll(bonus => bonus.active == false);
	}

	//A játékos paklijából a kézbe rak egy lapot.
	public Card DrawCardFromDeck()
	{
		Card temp = player.GetActiveDeck().DrawCard();
		if(temp != null)
		{
			cardsInHand.Add(temp);
			return temp;
		}

		else 
		{
			return null;
		}
	}

	//Vak húzás: A játékos a pakliból húzott lapot egyből a mezőre rakja.
	public Card BlindDraw()
	{
		Card temp = player.GetActiveDeck().DrawCard();

		if(temp != null)
		{
			cardsOnField.Add(temp);
			return temp;
		}

		else 
		{
			return null;	
		}
	}

	//Draw ellentéte, visszarakja a lapot a pakliba a megadott célból.
	public void PutAway(List<Card> cardList)
	{
		//Ha nem üres
		if(cardList.Any())
		{
			foreach (Card cards in cardList) 
			{
				this.player.GetActiveDeck().AddCard(cards);
			}
			cardList.Clear();
		}
		
	}

	//Elrak mindent a pályáról vissza a pakliba
	public void PutEverythingBack()
	{
		PutAway(cardsOnField);
		PutAway(cardsInHand);
		PutAway(winnerCards);
		PutAway(lostCards);
		player.GetActiveDeck().SortDeck();
	}

	//A kör végén hívódik meg alap esetben
	//A játékos eredménye alapján elrakja a kártyát a megfelelő listába
	public void PutActiveCardAway()
	{
		if(this.playerResult == PlayerTurnResult.Win)
		{
			foreach (Card card in cardsOnField) 
			{
				this.winnerCards.Add(card);
			}
		}

		else 
		{
			foreach (Card card in cardsOnField) 
			{
				this.lostCards.Add(card);
			}	
		}

		this.cardsOnField.Clear();
	}

	public void SetRole(PlayerTurnRole newRole)
	{
		this.playerRole = newRole;
	}

	public PlayerTurnRole GetRole()
	{
		return this.playerRole;
	}

	public void SetStatus(PlayerTurnStatus newStatus)
	{
		this.playerStatus = newStatus;
	}

	public PlayerTurnStatus GetStatus()
	{
		return this.playerStatus;
	}

	public void SetResult(PlayerTurnResult newResult)
	{
		this.playerResult = newResult;
	}

	public PlayerTurnResult GetResult()
	{
		return this.playerResult;
	}

	public List<Card> GetWinners()
	{
		return this.winnerCards;
	}

	public List<Card> GetLosers()
	{
		return this.lostCards;
	}

	//A játékostól begyűjtött id-val rendelkező lapot lerakja
	public void PlayCardFromHand(int cardID)
	{
		cardsOnField.Add(cardsInHand[cardID]);
		cardsInHand.RemoveAt(cardID);
	}

	public List<Card> GetCardsOnField()
	{
		return this.cardsOnField;
	}

	public void SetLateEffect(bool value)
	{
		this.hasLateEffect = value;
	}

	public bool GetLateEffect()
	{
		return this.hasLateEffect;
	}

	public void ShuffleDeck(System.Random rng)
	{
		this.player.GetActiveDeck().ShuffleDeck(rng);
	}

	public int GetPlayerID()
	{
		return this.player.GetUniqueID();
	}

	public int GetDeckSize()
	{
		return this.player.GetActiveDeck().GetDeckSize();
	}

	public string GetUsername()
	{
		return this.player.GetUsername();
	}

	public bool GetPlayerStatus()
	{
		return isThePlayer;
	}

	public List<Card> GetCardsInHand(CardListFilter filter = CardListFilter.None)
	{

		//Ha nem jeleníthetünk meg Master Crockot
		if(filter == CardListFilter.NoMasterCrok)
		{
			List<Card> temp = new List<Card>();
			foreach (Card card in cardsInHand) 
			{
				if(card.GetCardType() != CardType.Master_Crok)
				{
					temp.Add(card);
				}
			}
			return temp;
		}

		//Ha nincs különösebb kitétel arra, hogy milyen lapokat adjunk vissza
		else if(filter == CardListFilter.None)
		{
			return this.cardsInHand;
		}

		else 
		{
			return null;
		}
	}

	public int GetHandCount()
	{
		return this.cardsInHand.Count;
	}

	public int GetActiveCardsValue(CardStatType type)
	{
		int sum = 0;

		foreach (Card card in cardsOnField) 
		{
			if(type == CardStatType.Power)
			{
				sum += card.GetPower();
			}

			else if(type == CardStatType.Intelligence)
			{
				sum += card.GetIntelligence();
			}

			else if(type == CardStatType.Reflex)
			{
				sum += card.GetReflex();
			}

			//Laponként hozzáadjuk a boostot
			sum += GetBonusValue(type);
		}

		//Az értékek felhasználása után frissítjük a stat bónusz listát
		UpdateStatBonuses();

		return sum;
	}

	//Visszaadja összesítve, hogy az adott stat értékre mennyi boosttal rendelkezik a játékos
	private int GetBonusValue(CardStatType statType)
	{
		int sum = 0;

		foreach (StatBonus bonus in GetStatBonuses()) 
		{
			switch (statType) 
			{
				case CardStatType.Power:sum += bonus.GetPowerBoost(); break;
				case CardStatType.Intelligence:sum += bonus.GetIntelligenceBoost(); break;
				case CardStatType.Reflex:sum += bonus.GetReflexBoost(); break;
				default: break;
			}
		}

		return sum;
	}

	public int GetWinAmount()
	{
		return this.GetWinners().Count;
	}

	public int GetLostAmount()
	{
		return this.GetLosers().Count;
	}

	public void SacrificeAWinner(int id)
	{
		Card temp = this.winnerCards[id];
		this.winnerCards.RemoveAt(id);
		this.lostCards.Add(temp);
	}

	public void ReviveLostCard(int id)
	{
		Card temp = this.lostCards[id];
		this.lostCards.RemoveAt(id);
		this.cardsInHand.Add(temp);
	}

	public Sprite GetLastWinnerImage()
	{
		if(GetWinAmount() > 0)
		{
			return this.winnerCards[GetWinAmount() - 1].GetArt();
		}

		else 
		{
			return null;	
		}
	}

	public Sprite GetLastLostImage()
	{
		if(GetLostAmount() > 0)
		{
			return this.lostCards[GetLostAmount() - 1].GetArt();
		}

		else 
		{
			return null;	
		}
	}

	public List<Card> GetDeck(int limit)
	{
		//Ha nincs limit az egész paklit visszaadjuk
		if(limit == 0)
		{
			return player.GetActiveDeck().GetDeck();
		}

		//Limit esetén csak az első X = limit lapot adjuk meg
		else 
		{
			List<Card> temp = new List<Card>();
			for(var i = 0; i < limit; i++)
			{
				temp.Add(player.GetActiveDeck().GetDeck()[i]);
			}
			return temp;	
		}
	}

	public Card GetCardFromHand(int positionID)
	{
		return cardsInHand[positionID];
	}

	public Card GetCardFromField(int positionID)
	{
		return cardsOnField[positionID];
	}

	public Card GetCardFromLosers(int positionID)
	{
		return lostCards[positionID];
	}

	public Card GetCardFromDeck(int positionID)
	{
		return this.player.GetActiveDeck().GetCardFromDeck(positionID);
	}

	public void SwitchFromHand(int fieldPosition, int handPosition)
	{
		Card fieldTemp = cardsOnField[fieldPosition];
		Card handTemp = cardsInHand[handPosition];

		cardsOnField.RemoveAt(fieldPosition);
		cardsOnField.Insert(fieldPosition, handTemp);

		cardsInHand.RemoveAt(handPosition);
		cardsInHand.Insert(handPosition, fieldTemp);

	}

	public void SwitchFromDeck(int fieldPosition, int deckPosition)
	{
		Card fieldTemp = cardsOnField[fieldPosition];
		Card deckTemp = GetCardFromDeck(deckPosition);

		cardsOnField.RemoveAt(fieldPosition);
		cardsOnField.Insert(fieldPosition, deckTemp);

		player.GetActiveDeck().RemoveCardAt(deckPosition);
		player.GetActiveDeck().AddCardToIndex(fieldTemp, deckPosition);

	}

	public void TossCard(int positionID)
	{
		lostCards.Add(cardsInHand[positionID]);
		cardsInHand.RemoveAt(positionID);
	}

}
