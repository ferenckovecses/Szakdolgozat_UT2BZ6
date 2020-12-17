using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inverter : Node
{
	protected List<Node> childNodes = new List<Node>();

	public Inverter(List<Node> nodes)
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
					nodeState = NodeStates.Success;
					return nodeState;
				case NodeStates.Success:
					nodeState = NodeStates.Failure;
					return nodeState;
				case NodeStates.Running:
					nodeState = NodeStates.Running;
					return nodeState;
			}
		}

		nodeState =  NodeStates.Success;
		return nodeState;
	}
}
