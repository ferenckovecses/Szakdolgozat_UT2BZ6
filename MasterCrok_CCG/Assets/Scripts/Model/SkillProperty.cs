using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillProperty
{
	public SkillActivationTime activationTime;
	public SkillTriggerRequirement skillRequirement;
	public SkillEffectTarget skillTarget;
	public SkillEffectAction effectAction;
	

	public SkillProperty(
		SkillActivationTime time = SkillActivationTime.Normal,
		SkillTriggerRequirement req = SkillTriggerRequirement.None,
		SkillEffectTarget target = SkillEffectTarget.Self,
		SkillEffectAction action = SkillEffectAction.None)
	{
		this.activationTime = time;
		this.skillRequirement = req;
		this.skillTarget = target;
		this.effectAction = action;
	}
	

}
