using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameControll
{
	[System.Serializable]
	public class CardFactory : MonoBehaviour
	{
		public CardDatabase cardDataList;

		//Egy első szériás kezdőpaklit ad az új játékosnak
		public Deck GetStarterDeck()
		{
			Deck newDeck = new Deck();

			for (var i = 0; i < 21; i++)
			{
				newDeck.AddCard(cardDataList.GetCardByID(6));
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
				newDeck.AddCard(cardDataList.GetCardByID(cardIndex));
			}

			return newDeck;
		}


	}
}
