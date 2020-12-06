using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Multiplayer_Player : NetworkBehaviour
{
	public static Multiplayer_Player localPlayer;
	[SyncVar]public string matchID;

	private NetworkMatchChecker networkMatchChecker;

	void Start()
	{
		if(isLocalPlayer)
		{
			localPlayer = this;
		}

		networkMatchChecker = GetComponent<NetworkMatchChecker>();
	}

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
		if(Matchmaker.instance.HostGame(_matchID, gameObject))
		{
			Debug.Log("Game hosted successfully");
			networkMatchChecker.matchId = _matchID.ToGuid();
			TargetHostGame(true, matchID);
		}

		else 
		{
			Debug.Log("Game hosting failed");
			TargetHostGame(false, matchID);

		}
	}

	[TargetRpc]
	private void TargetHostGame(bool success, string matchID)
	{
		UILobby.instance.HostSuccess(success);
	}
}
