using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardPairs
{
	public int playerKey;
	public Card card;
	public int cardPosition;

	public PlayerCardPairs(int key, Card cardIn, int positionIn)
	{
		this.playerKey = key;
		this.card = cardIn;
		this.cardPosition = positionIn;
	}


}
