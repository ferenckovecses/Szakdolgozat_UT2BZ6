using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatBonus
{
	private int power = 0;
	private int intelligence = 0;
	private int reflex = 0;
	
	public StatBonus(int power, int intelligence, int reflex)
	{
		this.power = power;
		this.intelligence = intelligence;
		this.reflex = reflex;
	}

	public int[] GetBonus()
	{
		return new int[3] {this.power,this.intelligence,this.reflex};
	}

	public int GetPowerBoost()
	{
		return this.power;
	}

	public int GetIntelligenceBoost()
	{
		return this.intelligence;
	}

	public int GetReflexBoost()
	{
		return this.reflex;
	}
}
