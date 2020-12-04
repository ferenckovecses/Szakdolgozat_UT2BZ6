using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Deck
{
	private List<Card> cards;

	public Deck()
	{
		this.cards = new List<Card>();
	}

	public void AddCard(Card newCard)
	{
		cards.Add(newCard);
	}

	public void RemoveCard(Card cardToRemove)
	{
		cards.Remove(cardToRemove);
	}

	public void RemoveCardAt(int index)
	{
		cards.RemoveAt(index);
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

	//Fisher-Yates keverés implementálása
	public void ShuffleDeck(System.Random rng)
	{
		int n = cards.Count;  
    	while (n > 1) {  
	        n--;  
	        int k = rng.Next(n + 1); 
	        SwapCards(k,n); 
    	}  
	}

	public int GetDeckSize()
	{
		return this.cards.Count;
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

	public void SortDeck()
	{
		this.cards = cards.OrderBy(o=>o.GetCardID()).ToList();
	}

	public List<Card> GetDeck()
	{
		return this.cards;
	}

	public List<int> GetCardList()
	{
		List<int> cardID = new List<int>();
		foreach (Card card in cards) 
		{
			cardID.Add(card.GetCardID());
		}

		return cardID;
	}

	public Card GetCardFromDeck(int index)
	{
		return this.cards[index];
	}



}
