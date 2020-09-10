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

	[SerializeField]
	int cardID = 0;

	[SerializeField]
	string cardName = null;

	[SerializeField]
	CardType type = default(CardType);

	[Header("Képesség")]

	[SerializeField]
	string skillName = null;

	[TextArea(10,10)]
	[SerializeField]
	string skillDescription = null;

	[SerializeField]
	bool IsAQuickSkill = false;

	[SerializeField]
	bool isAHandSkill = false;

	[Header("Pontok")]
	[SerializeField]
	int power = 0;

	[SerializeField]
	int intelligence = 0;

	[SerializeField]
	int reflex = 0;

	[Header("Vizuális elemek")]
	[SerializeField]
	Sprite cardImage = null;

	public Sprite GetArt()
	{
		return this.cardImage;
	}

	public int GetCardID()
	{
		return this.cardID;
	}

	public int[] GetAttributes()
	{
		return new int[3] {power,intelligence,reflex};
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

	public string GetCardName()
	{
		return this.cardName;		
	}

	public string GetSkillName()
	{
		return this.skillName;
	}

	public string GetCardSkill()
	{
		return this.skillDescription;
	}

	public bool HasHandSkill()
	{
		return this.isAHandSkill;
	}

	public bool HasQuickSkill()
	{
		return this.IsAQuickSkill;
	}


}
