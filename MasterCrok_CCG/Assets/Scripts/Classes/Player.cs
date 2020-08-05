using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
	string username;
	int coinBalance = 100;
	Deck activeDeck;
	Deck secondaryDeck;

	public void ChangeName(string newName)
	{
		this.username = newName;
	}

	public void AddCoins(int amount)
	{
		this.coinBalance += amount;
	}

	public void SpendCoins(int price)
	{
		this.coinBalance -= price;
	}

	public int GetCoinBalance()
	{
		return this.coinBalance;
	}

	public void AddActiveDeck(Deck newDeck)
	{
		this.activeDeck = newDeck;
	}

	public void AddSecondaryDeck(Deck newDeck)
	{
		this.secondaryDeck = newDeck;
	}

	public Deck GetActiveDeck()
	{
		return this.activeDeck;
	}


}
