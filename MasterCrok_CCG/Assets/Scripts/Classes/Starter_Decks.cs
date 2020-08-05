using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Pre-made paklikhoz, mint pl a játék kezdetén kapott kezdőpakli

[CreateAssetMenu(fileName = "New Deck", menuName = "Decks")]
public class Starter_Decks : ScriptableObject
{
	public Deck activeDeck;
	public Deck secondaryDeck;
}
