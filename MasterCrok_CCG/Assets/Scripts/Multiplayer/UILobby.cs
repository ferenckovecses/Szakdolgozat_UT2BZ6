using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class UILobby : MonoBehaviour
{
	[SerializeField] TMP_InputField joinMatchField;
	[SerializeField] Button hostButton;
	[SerializeField] Button joinButton;
	[SerializeField] Canvas lobbyCanvas;

	public static UILobby instance;

	private void Start() 
	{
		instance = this;
	}

	public void Host()
	{
		SetInteractable(false);

		Multiplayer_Player.localPlayer.HostGame();
	}

	public void HostSuccess(bool success)
	{
		if(!success)
		{
			SetInteractable(true);
		}

		else
		{
			lobbyCanvas.enabled = true;
		}
	}

	public void Join()
	{
		SetInteractable(false);
	}

	public void JoinSuccess(bool success)
	{
		if(!success)
		{
			SetInteractable(true);
		}
	}

	private void SetInteractable(bool status)
	{
		hostButton.interactable = status;
		joinButton.interactable = status;
		joinMatchField.interactable = status;
	}
}
