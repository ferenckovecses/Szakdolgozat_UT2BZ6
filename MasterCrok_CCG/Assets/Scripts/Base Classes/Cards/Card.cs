using UnityEngine;
using System.Collections.Generic;


public class Card
{
    //Default és statikus kártya adatok
    private CardData data;

    //Objektum specifikus, változó adatok
    private int power;
    private int intelligence;
    private int reflex;

    private List<StatBonus> cardStatBonus;
    private List<SkillProperty> skillProperties;
    private int SkillID;

    public Card(CardData in_data)
    {
    	this.cardStatBonus = new List<StatBonus>();
        this.data = in_data;
        this.power = data.GetPower();
        this.intelligence = data.GetIntelligence();
        this.reflex = data.GetReflex();
    	this.SkillID = data.GetCardID();
    	this.skillProperties = data.GetSkillProperties();
    }

    #region Getters
    public int GetPower()
    {
        return (this.power + GetPowerBonus());
    }

    public int GetIntelligence()
    {
        return (this.intelligence + GetIntelligenceBonus());
    }

    public int GetReflex()
    {
        return (this.reflex + GetReflexBonus());
    }

    public int GetPowerBonus()
    {
    	int sum = 0;
    	foreach (StatBonus bonus in cardStatBonus) 
    	{
    		sum += bonus.GetPowerBoost();
    	}

    	return sum;
    }

    public int GetIntelligenceBonus()
    {
    	int sum = 0;
    	foreach (StatBonus bonus in cardStatBonus) 
    	{
    		sum += bonus.GetIntelligenceBoost();
    	}

    	return sum;
    }

    public int GetReflexBonus()
    {
    	int sum = 0;
    	foreach (StatBonus bonus in cardStatBonus) 
    	{
    		sum += bonus.GetReflexBoost();
    	}

    	return sum;
    }

	public Sprite GetArt()
	{
		return data.GetArt();
	}

	public int GetCardID()
	{
		return SkillID;
	}

	public CardType GetCardType()
	{
		return data.GetCardType();
	}

	public string GetCardName()
	{
		return data.GetCardName();
	}

	public string GetSkillName()
	{
		return data.GetSkillName();
	}

	public string GetCardSkill()
	{
		return data.GetCardSkill();
	}

	//Visszaadja, hogy rendelkezik-e a kártya Gyors képességgel
	public bool HasAQuickSkill()
	{
		foreach (SkillProperty property in skillProperties)
		{
			if (property.activationTime == SkillActivationTime.Quick)
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
			if (property.activationTime == SkillActivationTime.Late)
			{
				return true;
			}
		}

		return false;
	}

	public MultipleSkillRule GetSkillRule()
	{
		return data.GetSkillRule();
	}

	public List<SkillProperty> GetSkillProperty()
	{
		return this.skillProperties;
	}

	#endregion

	#region Setters and modifiers

	public void ResetBonuses()
	{
		this.cardStatBonus.Clear();
	}

	public void AddBonus(StatBonus newBonus)
	{
		this.cardStatBonus.Add(newBonus);
	}

	public void SetSKillID(int newSkillID)
	{
		this.SkillID = newSkillID;
	}

	public void ModifySkills(List<SkillProperty> newSkills)
	{
		this.skillProperties.Clear();

		foreach (SkillProperty property in newSkills) 
		{
			SkillProperty temp = new SkillProperty(property.activationTime, property.skillRequirement, property.skillTarget, property.effectAction);
			this.skillProperties.Add(temp);
		}
	}

	public void ResetSkills()
	{
		this.skillProperties = data.GetSkillProperties();
		this.SkillID = data.GetCardID();
	}

	#endregion



}
