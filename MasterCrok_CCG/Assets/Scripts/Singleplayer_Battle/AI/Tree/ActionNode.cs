using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionNode : Node
{
	public delegate NodeStates ActionNodeDelegate();
	private ActionNodeDelegate action;

	public ActionNode(ActionNodeDelegate _action)
	{
		this.action = _action;
	}


	public override NodeStates Evaluate()
	{
		switch (this.action()) 
		{
			case NodeStates.Success:
				nodeState = NodeStates.Success;
				return nodeState;
			case NodeStates.Failure:
				nodeState = NodeStates.Failure;
				return nodeState;
			case NodeStates.Running:
				nodeState = NodeStates.Running;
				return nodeState;
			default:
				nodeState = NodeStates.Running;
				return nodeState;
		}
	}
}
