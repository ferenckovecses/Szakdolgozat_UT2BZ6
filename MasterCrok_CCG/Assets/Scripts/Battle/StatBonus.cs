using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatBonus
{
	int duration;
	int power;
	int intelligence;
	int reflex;

	public StatBonus(int duration,int power, int intelligence, int reflex)
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

	public bool DecreaseDuration()
	{
		this.duration-=1;
		if (this.duration == 0) 
		{
			return true;
		}

		else 
		{
			return false;
		}
	}
}
