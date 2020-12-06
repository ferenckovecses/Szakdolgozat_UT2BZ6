using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AutoHostClient : MonoBehaviour
{
	[SerializeField] private NetworkManager networkManager;

	private void Start()
	{
		//Not a headless build
		if(!Application.isBatchMode)
		{
			Debug.Log("***Client build***");
			networkManager.StartClient();
		}

		else 
		{
			Debug.Log("***Server build***");
		}
	}

	public void JoinLocal()
	{
		networkManager.networkAddress = "localhost";
		networkManager.StartClient();
	}
}
