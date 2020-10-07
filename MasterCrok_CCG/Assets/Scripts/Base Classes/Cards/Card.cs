using UnityEngine;

public class Card
{
    //Default és statikus kártya adatok
    private CardData data;

    //Objektum specifikus, változó adatok
    private int power;
    private int intelligence;
    private int reflex;

    public Card(CardData in_data)
    {
        this.data = in_data;
        this.power = data.GetPower();
        this.intelligence = data.GetIntelligence();
        this.reflex = data.GetReflex();
    }

    #region Getters
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
		foreach (SkillProperty property in data.GetSkillProperty())
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
		foreach (SkillProperty property in data.GetSkillProperty())
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

	#endregion

	#region Setters and modifiers
	public void SetPower(int newPower)
    {
        this.power = newPower;
    }

    public void SetIntelligence(int newInt)
    {
        this.intelligence = newInt;
    }

    public void SetReflex(int newRef)
    {
        this.reflex = newRef;
    }

    public void IncreaseStats(int pow_bonus, int int_bonus, int ref_bonus)
    {
        this.power += pow_bonus;
        this.intelligence += int_bonus;
        this.reflex += ref_bonus;
    }

	public void ResetStats()
	{
		this.power = data.GetPower();
		this.intelligence = data.GetIntelligence();
		this.reflex = data.GetReflex();
	}
	#endregion



}
