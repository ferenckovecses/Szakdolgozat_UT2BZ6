using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatBonus
{
	private int duration;
	private int power = 0;
	private int intelligence = 0;
	private int reflex = 0;
	public bool active = true;

	public StatBonus(int power, int intelligence, int reflex, int duration = 1)
	{
		this.duration = duration;
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

	public void DecreaseDuration(int decreasingValue = 1)
	{
		this.duration -= decreasingValue;

		if (this.duration <= 0) 
		{
			active = false;;
		}
	}
}
