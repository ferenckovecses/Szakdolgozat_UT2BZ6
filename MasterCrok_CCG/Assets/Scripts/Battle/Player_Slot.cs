﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
Feladata: A játék, illetve a játékos adatmodellje. Játék indulásakor a játékos játékprofilja betöltődik a player változóba,
majd a játékvezérlőtől kap utasításokat (felhúzás, lerakás, státuszok változatatása)
*/

public enum TurnStatus{WaitingForTurn, ChooseStat, ChooseCard, CardPlayed, ChooseSkill, Finished};
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
	List<StatBonus> statBonus;

	//Játék közbeni státuszok tárolói
	TurnStatus playerStatus;
	TurnResult playerResult;
	TurnRole playerRole;
	
	//Kiértékelés utáni/következő körre ható aktivált kártya hatás jelzése
	bool hasLateEffect;

	bool isThePlayer;

	public Player_Slot(Player player, bool playerStatus = false)
	{
		this.player = player;
		this.cardsInHand = new List<Card>();
		this.winnerCards = new List<Card>();
		this.lostCards = new List<Card>();
		this.statBonus = new List<StatBonus>();
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
		PutAway(cardsInHand);
		PutAway(winnerCards);
		PutAway(lostCards);
		player.GetActiveDeck().SortDeck();
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

	public string GetNickname()
	{
		return this.player.GetUsername();
	}

	public bool GetPlayerStatus()
	{
		return isThePlayer;
	}

}
