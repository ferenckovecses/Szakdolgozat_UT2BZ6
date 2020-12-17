using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : Node
{
	protected List<Node> childNodes = new List<Node>();

	public Selector(List<Node> nodes)
	{
		childNodes = nodes;
	}

	public override NodeStates Evaluate()
	{
		foreach (Node child in childNodes) 
		{
			switch (child.Evaluate()) 
			{
				case NodeStates.Failure:
					continue;
				case NodeStates.Success:
					nodeState = NodeStates.Success;
					return nodeState;
				case NodeStates.Running:
					nodeState = NodeStates.Running;
					return nodeState;
				default: 
					continue;

			}
		}

		nodeState = NodeStates.Failure;
		return nodeState;
	}

}
