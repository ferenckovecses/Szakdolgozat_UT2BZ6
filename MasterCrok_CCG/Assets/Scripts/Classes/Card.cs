using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "New Card", menuName = "Cards")]
public class Card : ScriptableObject
{

	public enum CardType {Master_Crok, Távol_Kelet, Angyal, Démon, Kalandor, Katona, 
		Sportoló, Villám, Mókás, Vadnyugat, Rendfenntartás};

	[Header("Alap Információk")]
	public int cardID;
	public string cardName;
	public CardType type;

	[Header("Képesség")]
	public string skillName;
	[TextArea(10,10)]
	public string skillDescription;
	public bool IsAQuickSkill = false;

	[Header("Pontok")]
	public int power;
	public int intelligence;
	public int reflex;

	[Header("Vizuális elemek")]
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

	public int GetPower()
	{
		return this.power;
	}

	public int GetIntelligence()
	{
		return this.intelligence;
	}

	public int GetReflex()
	{
		return this.reflex;
	}


}
