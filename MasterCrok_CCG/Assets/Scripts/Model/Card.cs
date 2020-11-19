using UnityEngine;
using System.Collections.Generic;


public class Card
{
    //Statikus kártya adatok
    private CardData data;

    //Dinamikus kártya adatok
    private List<StatBonus> cardStatBonus;
    private SkillState skillState;

    public Card(CardData in_data)
    {
    	this.cardStatBonus = new List<StatBonus>();
        this.data = in_data;
        this.skillState = SkillState.NotDecided;
    }

    #region Getters
    public int GetPower()
    {
        return (data.GetPower() + GetPowerBonus());
    }

    public int GetIntelligence()
    {
        return (data.GetIntelligence() + GetIntelligenceBonus());
    }

    public int GetReflex()
    {
        return (data.GetReflex() + GetReflexBonus());
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
		return data.GetCardID();
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
		foreach (SkillProperty property in GetSkillProperty())
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
		foreach (SkillProperty property in GetSkillProperty())
		{
			if (property.activationTime == SkillActivationTime.Late)
			{
				return true;
			}
		}

		return false;
	}

	public List<SkillProperty> GetSkillProperty()
	{
		return data.GetSkillProperties();
	}

	public SkillState GetSkillState()
	{
		return this.skillState;
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

	public void SetSkillState(SkillState newState)
	{
		this.skillState = newState;
	}

	public void ResetSkillState()
	{
		this.skillState = SkillState.NotDecided;
	}

	#endregion



}
