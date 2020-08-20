using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public enum DeckStatus {NotEnoughCard, TooManyCards, Verified};

[System.Serializable]
public class Deck
{
	public List<Card> cards;
	DeckStatus deckStatus;

	public Deck()
	{
		this.cards = new List<Card>();
	}

	public void LoadDeck(string filepath)
	{
		//Betölti a játékos pakliját fájlból
	}

	public void AddCard(Card newCard)
	{
		cards.Add(newCard);
	}

	public void RemoveCard(Card cardToRemove)
	{
		cards.Remove(cardToRemove);
	}

	public void AddCardToIndex(Card newCard, int index)
	{
		cards.Insert(index, newCard);
	}

	public void SwapCards(int indexA, int indexB)
	{
		Card tmp = cards[indexA];
		cards[indexA] = cards[indexB];
		cards[indexB] = tmp;
	}

	//Változtatások esetén fut majd le az aktív paklin
	//Ellenőrzi, hogy a pakli a szabályoknak megfelelő-e.
	public bool VerifyDeckSize()
	{
		if(cards.Count < 10)
		{
			deckStatus = DeckStatus.NotEnoughCard;
			return false;
		}

		else if(cards.Count > 60)
		{
			deckStatus = DeckStatus.TooManyCards;
			return false;
		}

		else 
		{
			deckStatus = DeckStatus.Verified;
			return true;	
		}
	}

	//Fisher-Yates keverés implementálása
	public void ShuffleDeck()
	{
		System.Random rng = new System.Random();

		int n = cards.Count;  
    	while (n > 1) {  
	        n--;  
	        int k = rng.Next(n + 1); 
	        SwapCards(k,n); 
    	}  
	}

	//Visszaadja, hogy hány ilyen kártya található a pakliban
	public int DuplicateCounter(Card cardToCompare)
	{
		return (int)cards.Count(p => p.cardID == cardToCompare.cardID);
	}

	public DeckStatus GetDeckStatus()
	{
		return this.deckStatus;
	}

	public Card DrawCard()
	{
		if(cards.Any())
		{
			Card drawCard = cards[0];
			cards.Remove(drawCard);
			return drawCard;
		}

		else 
		{
			return null;	
		}
	}



}
