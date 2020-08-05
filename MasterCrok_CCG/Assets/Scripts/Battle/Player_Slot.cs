using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum TurnStatus{WaitingForTurn, ChooseStat, ChooseCard, OutOfCards, CardPlayed, ChooseSkill, Finished};
public enum TurnResult{Winner, Loser, TurnNotEnded};
public enum TurnRole{Attacker, Defender};

public class Player_Slot
{
	//A játékos a pozícióban
	Player player;

	//A játékban használt lapok tárolói
	List<Card> cardsInHand;
	List<Card> winnerCards;
	List<Card> lostCards;

	//A kijátszott kártya helye
	public Card activeCard;

	//Értékmódosítók
	int powerBonus = 0;
	int intelligenceBonus = 0;
	int reflexBonus = 0;

	//Játék közbeni státuszok tárolói
	TurnStatus playerStatus;
	TurnResult playerResult;
	TurnRole playerRole;
	//Kiértékelés utáni/következő körre ható aktivált kártya hatás jelzése
	bool hasLateEffect;

	public Player_Slot(Player player)
	{
		this.player = player;
		this.cardsInHand = new List<Card>();
		this.winnerCards = new List<Card>();
		this.lostCards = new List<Card>();

		hasLateEffect = false;
	}


	public void ResetBonuses()
	{
		this.powerBonus = 0;
		this.intelligenceBonus = 0;
		this.reflexBonus = 0;
	}

	public void IncreasePower(int amount)
	{
		this.powerBonus += amount;
	}

	public void IncreaseIntelligence(int amount)
	{
		this.intelligenceBonus += amount;
	}

	public void IncreaseReflex(int amount)
	{
		this.reflexBonus += amount;
	}

	//A játékos paklijából a kézbe rak egy lapot.
	public void DrawCard()
	{
		Card temp = player.GetActiveDeck().DrawCard();
		if(temp != null)
		{
			cardsInHand.Add(temp);
		}

		else 
		{
			SetStatus(TurnStatus.OutOfCards);	
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
		PutAway(cardsInHand);
		PutAway(winnerCards);
		PutAway(lostCards);
	}

	//A kör végén hívódik meg alap esetben
	//A játékos eredménye alapján elrakja a kártyát a megfelelő listába
	public void PutActiveCardAway()
	{

		if(this.playerResult != TurnResult.TurnNotEnded)
		{
			if(this.playerResult == TurnResult.Winner)
			{
				this.winnerCards.Add(this.activeCard);
			}

			else 
			{
				this.lostCards.Add(this.activeCard);	
			}

			this.activeCard = null;
		}
	}

	public void SetRole(TurnRole newRole)
	{
		this.playerRole = newRole;
	}

	public TurnRole GetRole()
	{
		return this.playerRole;
	}

	public void SetStatus(TurnStatus newStatus)
	{
		this.playerStatus = newStatus;
	}

	public TurnStatus GetStatus()
	{
		return this.playerStatus;
	}

	public void SetResult(TurnResult newResult)
	{
		this.playerResult = newResult;
	}

	public TurnResult GetResult()
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
		activeCard = cardsInHand[cardID];
		cardsInHand.Remove(activeCard);
	}

	public Card GetPlayedCard()
	{
		return this.activeCard;
	}

	public void SetLateEffect(bool value)
	{
		this.hasLateEffect = value;
	}

	public bool GetLateEffect()
	{
		return this.hasLateEffect;
	}

}
