using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards")]
public class Card : ScriptableObject
{

	public enum CardType {Master_Crok, Távol_Kelet, Angyal, Démon, Kalandor, Katona, 
		Sportoló, Villám, Mókás, Vadnyugat, Rendfenntartás};

	[Header("Basic Informations")]
	public int cardID;
	public string cardName;
	public CardType type;

	[Header("Skill Details")]
	public string skillName;
	[TextArea(10,10)]
	public string skillDescription;

	[Header("Stats")]
	public int power;
	public int intelligence;
	public int reflex;

	[Header("Visuals")]
	public Sprite cardImage;

	public Sprite GetArt()
	{
		return this.cardImage;
	}

	public int GetCardID()
	{
		return this.cardID;
	}

	public CardType GetCardType()
	{
		return this.type;
	}


}
