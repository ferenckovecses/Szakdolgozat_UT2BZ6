﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards")]
public class CardData : ScriptableObject
{

	[Header("Alap Információk")]

	[SerializeField]
	private int cardID = 0;

	[SerializeField]
	private string cardName = null;

	[SerializeField]
	private CardType type = default(CardType);

	[Header("Képesség")]

	[SerializeField]
	private string skillName = null;

	[TextArea(10,10)]
	[SerializeField]
	private string skillDescription = null;

	[SerializeField]
	private List<SkillProperty> skillProperties = new List<SkillProperty>();

	[Header("Pontok")]
	[SerializeField]
	private int power = 0;

	[SerializeField]
	private int intelligence = 0;

	[SerializeField]
	private int reflex = 0;

	[Header("Vizuális elemek")]
	[SerializeField]
	private Sprite cardImage = null;

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

	//Visszaadja, hogy rendelkezik-e a kártya Gyors képességgel
	public bool HasAQuickSkill()
	{
		foreach (SkillProperty property in skillProperties) 
		{
			if(property.activationTime == SkillActivationTime.Quick)
			{
				return true;
			}
		}

		return false;
	}

	public bool HasALateSkill()
	{
		foreach (SkillProperty property in skillProperties) 
		{
			if(property.activationTime == SkillActivationTime.Late)
			{
				return true;
			}
		}

		return false;
	}

	//Visszaad egy friss, független listát megegyező tartalmú elemekkel
	public List<SkillProperty> GetSkillProperties()
	{
		return this.skillProperties;
	}

}
