using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using ClientSide;

public class Multiplayer_Player : NetworkBehaviour
{
	public static Multiplayer_Player localPlayer;
	[SyncVar]public string matchID;
	[SyncVar]public int playerIndex;
	public string playerName = "Player";

	private NetworkMatchChecker networkMatchChecker;

	void Start()
	{
		networkMatchChecker = GetComponent<NetworkMatchChecker>();

		if(Profile_Controller.playerProfile != null)
		{
			this.playerName = Profile_Controller.playerProfile.GetUsername();
		}

		if(isLocalPlayer)
		{
			localPlayer = this;
		}

		else
		{
			UILobby.instance.SpawnPlayerPrefab(this);
		}
	}

	//****Host Game****

	public void HostGame()
	{
		string matchID = Matchmaker.GetRandomMatchID();
		CmdHostGame(matchID);
	}

	//Kliens -> Szerver
	[Command]
	private void CmdHostGame(string _matchID)
	{
		matchID = _matchID;
		if(Matchmaker.instance.HostGame(_matchID, gameObject, out playerIndex))
		{
			Debug.Log("Game hosted successfully");
			networkMatchChecker.matchId = _matchID.ToGuid();
			Debug.Log($"Match ID: {matchID}");
			TargetHostGame(true, matchID, playerIndex);
		}

		else 
		{
			Debug.Log("Game hosting failed");
			TargetHostGame(false, matchID, playerIndex);
		}
	}

	[TargetRpc]
	private void TargetHostGame(bool success, string _matchID, int _playerIndex)
	{
		this.matchID = _matchID;
		this.playerIndex = _playerIndex;
		UILobby.instance.HostSuccess(success, this.matchID);
	}

	//****Join Game****

	public void JoinGame(string _matchID)
	{
		CmdJoinGame(_matchID);
	}

	//Kliens -> Szerver
	[Command]
	private void CmdJoinGame(string _matchID)
	{
		matchID = _matchID;
		if(Matchmaker.instance.JoinGame(_matchID, gameObject, out playerIndex))
		{
			Debug.Log("Join was successfully");
			Debug.Log($"Player Index: {playerIndex}");
			networkMatchChecker.matchId = _matchID.ToGuid();
			TargetJoinGame(true, matchID, playerIndex);
		}

		else 
		{
			Debug.Log("Join failed");
			TargetJoinGame(false, matchID, playerIndex);
		}
	}

	//Szerver -> Adott kliens
	[TargetRpc]
	private void TargetJoinGame(bool success, string _matchID, int _playerIndex)
	{
		this.matchID = _matchID;
		this.playerIndex = _playerIndex;
		UILobby.instance.JoinSuccess(success, this.matchID);
	}


	//****Begin Game****

	public void BeginGame()
	{
		CmdBeginGame();
	}

	//Kliens -> Szerver
	[Command]
	private void CmdBeginGame()
	{
		Matchmaker.instance.BeginGame(matchID);
		Debug.Log("Game Beginning");
	}

	public void StartGame()
	{
		TargetBeginGame();
	}

	[TargetRpc]
	private void TargetBeginGame()
	{
		SceneManager.LoadScene("Multiplayer_Battle", LoadSceneMode.Additive);
	}
}
