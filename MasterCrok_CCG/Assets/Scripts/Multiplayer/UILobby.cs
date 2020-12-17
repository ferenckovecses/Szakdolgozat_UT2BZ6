using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;

public class UILobby : MonoBehaviour
{
	[Header("Host Join")]
	[SerializeField] TMP_InputField joinMatchField;
	[SerializeField] Button hostButton;
	[SerializeField] Button joinButton;
	[SerializeField] Canvas lobbyCanvas;

	[Header("Lobby")]
	[SerializeField] Transform PlayerGrid;
	[SerializeField] GameObject LobbyPlayerPrefab;
	[SerializeField] TMP_Text matchID_Text;
	[SerializeField] GameObject beginGameBtn;

	public static UILobby instance;
	GameObject localPlayerUI;

	private void Start() 
	{
		instance = this;
	}

	public void Host()
	{
		SetInteractable(false);

		Multiplayer_Player.localPlayer.HostGame();
	}

	public void HostSuccess(bool success, string lobbyID)
	{
		if(!success)
		{
			Notification_Controller.DisplayNotification("Nem sikerült a meccs létrehozása!\nPróbálkozz újra!");
			SetInteractable(true);
		}

		else
		{
			lobbyCanvas.enabled = true;
			if(localPlayerUI != null)
			{
				Destroy(localPlayerUI);
			}
			localPlayerUI = SpawnPlayerPrefab(Multiplayer_Player.localPlayer);
			matchID_Text.text = $"Harc: {lobbyID}";
			beginGameBtn.SetActive(true);
		}
	}

	public void Join()
	{
		SetInteractable(false);

		Multiplayer_Player.localPlayer.JoinGame(joinMatchField.text.ToUpper());
	}

	public void JoinSuccess(bool success, string lobbyID)
	{
		if(!success)
		{
			Notification_Controller.DisplayNotification("Sikertelen csatlakozás!\nEllenőrizd újra a kódot!");
			SetInteractable(true);
		}

		else
		{
			lobbyCanvas.enabled = true;
			if(localPlayerUI != null)
			{
				Destroy(localPlayerUI);
			}
			localPlayerUI = SpawnPlayerPrefab(Multiplayer_Player.localPlayer);
			matchID_Text.text = $"Harc: {lobbyID}";
		}
	}

	public void DisconnectGame()
	{
		if(localPlayerUI != null)
		{
			Destroy(localPlayerUI);
		}
		Multiplayer_Player.localPlayer.DisconnectGame();
		lobbyCanvas.enabled = false;
		SetInteractable(true);
		beginGameBtn.SetActive(false);
	}

	

	private void SetInteractable(bool status)
	{
		hostButton.interactable = status;
		joinButton.interactable = status;
		joinMatchField.interactable = status;
	}

	public GameObject SpawnPlayerPrefab(Multiplayer_Player player)
	{
		GameObject newPlayer = Instantiate(LobbyPlayerPrefab, PlayerGrid);
		newPlayer.GetComponent<UILobby_Player>().SetPlayer(player);
		newPlayer.transform.SetSiblingIndex(player.playerIndex-1);
		return newPlayer;
	}

	public void BeginGame()
	{
		Multiplayer_Player.localPlayer.BeginGame();
	}
}
