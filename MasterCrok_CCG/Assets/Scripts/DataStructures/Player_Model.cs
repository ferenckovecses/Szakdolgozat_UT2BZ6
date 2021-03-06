﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player_Model
{
	//A játékos a pozícióban
	private Player player;

	//A játékban használt lapok tárolói
	private List<Card> cardsInHand;
	private List<Card> winnerCards;
	private List<Card> lostCards;
	private List<Card> cardsOnField;

	//Értékmódosító bónuszok
	private List<FieldBonus> fieldBonusList;

	//Játék közbeni státuszok tárolói
	private PlayerTurnStatus playerStatus;
	private PlayerTurnResult playerResult;
	private PlayerTurnRole playerRole;

	//A model egy játékoshoz tartozik-e?
	private bool isPlayer;

	public Player_Model(Player player, bool playerStatus = false)
	{
		this.player = player;
		this.cardsInHand = new List<Card>();
		this.winnerCards = new List<Card>();
		this.lostCards = new List<Card>();
		this.fieldBonusList = new List<FieldBonus>();
		this.cardsOnField = new List<Card>();
		this.isPlayer = playerStatus;
	}

	#region Getters

	public PlayerTurnRole GetRole()
	{
		return this.playerRole;
	}

	public PlayerTurnStatus GetStatus()
	{
		return this.playerStatus;
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

	public List<Card> GetCardsOnField()
	{
		return this.cardsOnField;
	}

	public List<Card> GetCardsInHand()
	{
		return this.cardsInHand;
	}

	public List<Card> GetDeck(int limit)
	{
		if(limit == 0 || limit > GetDeckSize())
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

	public string GetUsername()
	{
		return this.player.GetUsername();
	}

	public bool GetPlayerStatus()
	{
		return isPlayer;
	}

	public int GetPlayerID()
	{
		return this.player.GetUniqueID();
	}

	public int GetDeckSize()
	{
		return this.player.GetActiveDeck().GetDeckSize();
	}

	public int GetHandCount()
	{
		return this.cardsInHand.Count;
	}

	public int GetActiveCardsValue(CardStatType type)
	{
		int sum = 0;
		int index = 0;
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

			index++;
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

	#endregion

	#region Setters

	public void SetRole(PlayerTurnRole newRole)
	{
		this.playerRole = newRole;
	}

	public void SetStatus(PlayerTurnStatus newStatus)
	{
		this.playerStatus = newStatus;
	}

	public void SetResult(PlayerTurnResult newResult)
	{
		this.playerResult = newResult;
	}

	#endregion

	#region Interactions

	//A játékos paklijából a kézbe rak egy lapot.
	public Card DrawCardFromDeck()
	{
		Card temp = player.GetActiveDeck().DrawCard();
		if(temp != null)
		{
			ApplyFieldBonusToCard(temp);
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
			ApplyFieldBonusToCard(temp);
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
			foreach (Card card in cardList) 
			{
				card.ResetBonuses();
				this.player.GetActiveDeck().AddCard(card);
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
				card.ResetBonuses();
			}
		}

		else 
		{
			foreach (Card card in cardsOnField) 
			{
				this.lostCards.Add(card);
				card.ResetBonuses();
			}	
		}

		this.cardsOnField.Clear();
	}

	//A játékostól begyűjtött id-val rendelkező lapot lerakja
	public void PlayCardFromHand(int cardID)
	{
		cardsOnField.Add(cardsInHand[cardID]);
		cardsInHand.RemoveAt(cardID);
	}

	public void ShuffleDeck(System.Random rng)
	{
		this.player.GetActiveDeck().ShuffleDeck(rng);
	}

	public void SacrificeAWinner(int id)
	{
		Card temp = this.winnerCards[id];
		temp.ResetBonuses();
		this.winnerCards.RemoveAt(id);
		this.lostCards.Add(temp);
	}

	public void ReviveLostCard(int id)
	{
		Card temp = this.lostCards[id];
		ApplyFieldBonusToCard(temp);
		this.lostCards.RemoveAt(id);
		this.cardsInHand.Add(temp);
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
		ApplyFieldBonusToCard(deckTemp);

		cardsOnField.RemoveAt(fieldPosition);
		cardsOnField.Insert(fieldPosition, deckTemp);

		player.GetActiveDeck().RemoveCardAt(deckPosition);
		player.GetActiveDeck().AddCardToIndex(fieldTemp, deckPosition);

		GetCardFromDeck(deckPosition).ResetBonuses();

	}

	public void TossCard(int positionID)
	{
		Card temp = cardsInHand[positionID];
		temp.ResetBonuses();
		lostCards.Add(temp);
		cardsInHand.RemoveAt(positionID);
	}

	//Új stat bónuszt adunk a játékosnak
	public void AddFieldBonus(FieldBonus newBonus)
	{
		fieldBonusList.Add(newBonus);
	}

	//Kör elején hozzáadja a bónuszokat a kézben lévő lapokhoz
	public void ApplyFieldBonusToAll()
	{
		foreach (Card card in cardsInHand) 
		{
			ApplyFieldBonusToCard(card);
		}
	}

	//Megadott laphoz az összes bónuszt hozzáadja
	public void ApplyFieldBonusToCard(Card card)
	{
		//Ha van bónusz a listában
		if(fieldBonusList.Any())
		{
			foreach(FieldBonus fieldBonus in fieldBonusList)
			{
				card.AddBonus(fieldBonus.GetBonus());
			}	
		}
	}

	//Kör végén eltávolítja a bónuszokat a lapokról
	public void ResetBonuses()
	{
		foreach (Card card in cardsInHand) 
		{
			foreach(FieldBonus fieldBonus in fieldBonusList)
			{
				card.ResetBonuses();
			}
		}
	}

	//Frissíti a bónuszok státuszát: Csökkenti a tartalmi idejüket egy körrel
	//Amik lejártak, azokat kidobja
	public void UpdateBonuses()
	{
		foreach (FieldBonus bonus in fieldBonusList) 
		{
			bonus.DecreaseDuration();
		}

		fieldBonusList.RemoveAll(bonus => bonus.GetStatus() == false);
	}

	#endregion

}
