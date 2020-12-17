using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Node
{
	public delegate NodeStates NodeReturn();
	protected NodeStates nodeState;

	public Node()
	{

	}

	public abstract NodeStates Evaluate();


	public NodeStates GetNodeState()
	{
		return this.nodeState;
	}
}
