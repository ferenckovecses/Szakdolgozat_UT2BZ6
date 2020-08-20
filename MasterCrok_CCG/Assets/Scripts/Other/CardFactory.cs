using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CardFactory : MonoBehaviour
{
	public List<Card> cardList;

	public int GetCardCount()
	{
		return this.cardList.Count;
	}

	public Card GetCardByID(int id)
	{
		return this.cardList[id];
	}

	public Sprite GetImageByID(int id)
	{
		return this.cardList[id].GetArt();
	}

	public int[] GetCardAttributesByID(int id)
	{
		return this.cardList[id].GetAttributes();
	}

	//Egy első szériás kezdőpaklit ad az új játékosnak
	public Deck GetStarterDeck()
	{
		Deck newDeck = new Deck();

		for(var i = 0; i < 21; i++)
		{
			newDeck.AddCard(cardList[i]);
		}

		return newDeck;
	}

	//A megadott számlistából rekonstruálja a játékos elmentett pakliját
	public Deck GetDeckFromArray(List<int> cardIdentifiers)
	{
		Deck newDeck = new Deck();

		for(var i = 0; i < cardIdentifiers.Count; i++)
		{	int cardIndex = cardIdentifiers[i];
			newDeck.AddCard(cardList[cardIndex]);
		}

		return newDeck;
	}
}
