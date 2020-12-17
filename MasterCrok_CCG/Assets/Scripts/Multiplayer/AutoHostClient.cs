using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class AutoHostClient : MonoBehaviour
{
	private NetworkManager networkManager;

	private void Start()
	{
		networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
	}

	public void HostLocal()
	{
		networkManager.StartHost();
	}

	public void JoinLocal()
	{
		networkManager.networkAddress = "localhost";
		networkManager.StartClient();
	}

	public void ReturnMenu()
	{
		SceneManager.LoadScene("Main_Menu");
	}
}
