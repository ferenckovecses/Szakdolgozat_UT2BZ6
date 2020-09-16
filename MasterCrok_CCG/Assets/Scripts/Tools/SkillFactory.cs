using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillFactory
{
	private SinglePlayer_Controller gameController;

	public SkillFactory(SinglePlayer_Controller con)
	{
		this.gameController = con;
	}

	public void UseSkill(int skillId)
	{
		switch (skillId) 
		{
			case 1: break;
			case 16: ShieldFight(); break;
			default: break;
		}
	}

	void ShieldFight()
	{
		this.gameController.SetActiveStat(ActiveStat.Power);
	}

	
}
