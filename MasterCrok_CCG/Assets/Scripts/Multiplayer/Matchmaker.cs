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

	private void Start()
	{
		instance = this;
	}

	public bool HostGame(string matchID, GameObject host)
	{
		//Ha az id még nem foglalt
		if(!matchIDList.Contains(matchID))
		{
			matchIDList.Add(matchID);
			matches.Add(new Match(matchID,host));
			Debug.Log("Match generated");
			return true;
		}

		else 
		{
			Debug.Log("Match ID already exists!");
			return false;	
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
