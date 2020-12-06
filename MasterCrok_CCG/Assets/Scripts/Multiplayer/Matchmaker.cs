using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Security.Cryptography;
using System.Text;

[System.Serializable]
public class Match
{
	public string matchID;
	public SyncListGameObject players = new SyncListGameObject();

	public Match(string matchID, GameObject host)
	{
		this.matchID = matchID;
		players.Add(host);
	}

	public Match() { }
}

[System.Serializable]
public class SyncListGameObject : SyncList<GameObject> { }

[System.Serializable]
public class SyncListMatch : SyncList<Match> { }

public class Matchmaker : NetworkBehaviour
{
	public static Matchmaker instance;
	public SyncListMatch matches = new SyncListMatch();
	public SyncList<string> matchIDList = new SyncList<string>();
	[SerializeField] GameObject turnManagerPrefab;

	private void Start()
	{
		instance = this;
	}

	public bool HostGame(string matchID, GameObject host, out int playerIndex)
	{
		playerIndex = -1;
		//Ha az id még nem foglalt
		if(!matchIDList.Contains(matchID))
		{
			matchIDList.Add(matchID);
			matches.Add(new Match(matchID,host));
			Debug.Log("Match generated");
			playerIndex = 1;
			return true;
		}

		else 
		{
			Debug.Log("Match ID already exists!");
			return false;	
		}
	}
	public bool JoinGame(string _matchID, GameObject player, out int playerIndex)
	{
		playerIndex = -1;
		//Ha a megadott ID létezik
		if(matchIDList.Contains(_matchID))
		{
			for (int i = 0; i < matches.Count; i++)
			{
				if(matches[i].matchID == _matchID)
				{
					matches[i].players.Add(player);
					playerIndex = matches[i].players.Count;
					break;
				}
			}
			Debug.Log("Joined to match");
			return true;
		}

		else 
		{
			Debug.Log("Match ID does not exist!");
			return false;	
		}
	}

	public void BeginGame(string _matchID)
	{
		GameObject newTurnManager = Instantiate(turnManagerPrefab);
		NetworkServer.Spawn(newTurnManager);
		TurnManager turnManager = newTurnManager.GetComponent<TurnManager>();
		newTurnManager.GetComponent<NetworkMatchChecker>().matchId = _matchID.ToGuid();

		for (int i = 0; i < matches.Count; i++)
		{
			if(matches[i].matchID == _matchID)
			{
				foreach (var player in matches[i].players)
				{
					Multiplayer_Player _player = player.GetComponent<Multiplayer_Player>();
					turnManager.AddPlayer(_player);
					_player.StartGame();
				}
				break;
			}
		}
	}

	public static string GetRandomMatchID()
	{
		string id = string.Empty;

		for(int i = 0; i < 5; i++)
		{
			int random = UnityEngine.Random.Range(0, 36);
			if(random < 26)
			{
				id += (char)(random + 65);
			}

			else 
			{
				id += (random - 26).ToString();	
			}
		}

		Debug.Log($"Random match ID: {id}");

		return id;
	}
}

	public static class MatchExtensions
	{
		public static Guid ToGuid(this string id)
		{
			MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
			byte[] inputBytes = Encoding.Default.GetBytes(id);
			byte[] hashBytes = provider.ComputeHash(inputBytes);

			return new Guid(hashBytes);

		}
	}
