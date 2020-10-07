using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameControll
{
	[System.Serializable]
	public class CardFactory : MonoBehaviour
	{
		public List<CardData> cardDataList;

		public int GetCardCount()
		{
			return this.cardDataList.Count;
		}

		public Card GetCardByID(int id)
		{
			return new Card(cardDataList[id]);
		}

		public Sprite GetImageByID(int id)
		{
			return this.cardDataList[id].GetArt();
		}

		public int[] GetCardAttributesByID(int id)
		{
			return this.cardDataList[id].GetAttributes();
		}

		//Egy első szériás kezdőpaklit ad az új játékosnak
		public Deck GetStarterDeck()
		{
			Deck newDeck = new Deck();

			for (var i = 0; i < 21; i++)
			{
				
				if(i < 10)
					newDeck.AddCard(new Card(cardDataList[8]));
				else
					newDeck.AddCard(new Card(cardDataList[i]));
			}

			return newDeck;
		}

		//A megadott számlistából rekonstruálja a játékos elmentett pakliját
		public Deck GetDeckFromList(List<int> cardIdentifiers)
		{
			Deck newDeck = new Deck();
			for (var i = 0; i < cardIdentifiers.Count; i++)
			{
				int cardIndex = cardIdentifiers[i] - 1;
				newDeck.AddCard(new Card(cardDataList[cardIndex]));
			}

			return newDeck;
		}

		public List<CardData> GetAllCard()
		{
			return this.cardDataList;
		}

		public List<Sprite> GetAllArt()
		{
			List<Sprite> imageList = new List<Sprite>();
			foreach (CardData card in cardDataList)
			{
				imageList.Add(card.GetArt());
			}
			return imageList;
		}


	}
}
