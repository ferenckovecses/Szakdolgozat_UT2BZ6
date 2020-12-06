using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldBonus
{
	private StatBonus bonus;
	private int duration;
	private bool status;

	public FieldBonus(StatBonus newBonus, int newDuration = 1)
	{
		this.bonus = newBonus;
		this.duration = (newDuration + 1);
		this.status = true;
	}

	public StatBonus GetBonus()
	{
		return this.bonus;
	}

	public bool GetStatus()
	{
		return this.status;
	}

	public void DecreaseDuration()
	{
		this.duration -= 1;

		if(this.duration <= 0)
		{
			this.status = false;
		}
	}

}
