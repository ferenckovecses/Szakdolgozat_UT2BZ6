using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : Node
{
	protected List<Node> childNodes = new List<Node>();

	public Sequence(List<Node> nodes)
	{
		childNodes = nodes;
	}

	public override NodeStates Evaluate()
	{
		bool anyChildRunning = false;
		foreach (Node child in childNodes) 
		{
			switch (child.Evaluate()) 
			{
				case NodeStates.Failure:
					nodeState = NodeStates.Failure;
					return nodeState;
				case NodeStates.Success:
					continue;
				case NodeStates.Running:
					anyChildRunning = true;
					continue;
				default: 
					nodeState = NodeStates.Success;
					return nodeState;

			}
		}

		nodeState = anyChildRunning ? NodeStates.Running : NodeStates.Success;
		return nodeState;
	}
}
