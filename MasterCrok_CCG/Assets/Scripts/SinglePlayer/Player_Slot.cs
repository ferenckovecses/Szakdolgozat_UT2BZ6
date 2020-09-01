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

public class Player_Slot
{
	//A játékos a pozícióban
	Player player;

	//A játékban használt lapok tárolói
	List<Card> cardsInHand;
	List<Card> winnerCards;
	List<Card> lostCards;

	//A kijátszott kártya helye
	List<Card> activeCards;

	//Értékmódosítók
	List<StatBonus> statBonus;

	//Játék közbeni státuszok tárolói
	PlayerTurnStatus playerStatus;
	PlayerTurnResult playerResult;
	PlayerTurnRole playerRole;
	
	//Kiértékelés utáni/következő körre ható aktivált kártya hatás jelzése
	bool hasLateEffect;

	//A model egy játékoshoz tartozik-e
	bool isThePlayer;

	public Player_Slot(Player player, bool playerStatus = false)
	{
		this.player = player;
		this.cardsInHand = new List<Card>();
		this.winnerCards = new List<Card>();
		this.lostCards = new List<Card>();
		this.statBonus = new List<StatBonus>();
		this.activeCards = new List<Card>();
		isThePlayer = playerStatus;
		hasLateEffect = false;
	}

	public void AddBonus(StatBonus newBonus)
	{
		statBonus.Add(newBonus);
	}

	//A játékos paklijából a kézbe rak egy lapot.
	public Card DrawCard()
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
		PutAway(activeCards);
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
			foreach (Card card in activeCards) 
			{
				this.winnerCards.Add(card);
			}
		}

		else 
		{
			foreach (Card card in activeCards) 
			{
				this.lostCards.Add(card);
			}	
		}

		this.activeCards.Clear();
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
		activeCards.Add(cardsInHand[cardID]);
		cardsInHand.RemoveAt(cardID);
	}

	public List<Card> GetPlayedCard()
	{
		return this.activeCards;
	}

	public void SetLateEffect(bool value)
	{
		this.hasLateEffect = value;
	}

	public bool GetLateEffect()
	{
		return this.hasLateEffect;
	}

	public void ShuffleDeck()
	{
		this.player.GetActiveDeck().ShuffleDeck();
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

	public List<Card> GetCardsInHand()
	{
		return this.cardsInHand;
	}

	public int GetActiveCardsValue(ActiveStat type)
	{
		int sum = 0;

		foreach (Card card in activeCards) 
		{
			if(type == ActiveStat.Power)
			{
				sum += card.GetPower();
			}

			else if(type == ActiveStat.Intelligence)
			{
				sum += card.GetIntelligence();
			}

			else if(type == ActiveStat.Reflex)
			{
				sum += card.GetReflex();
			}
		}

		return sum;
	}

}
