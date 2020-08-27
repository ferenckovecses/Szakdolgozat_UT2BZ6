using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Profile_Data
{
	public int profileID {get;set;}
	public string username {get;set;}
	public int coinBalance {get;set;}
	public List<int> activeDeck {get;set;}
	public List<int> secondaryDeck {get;set;}

	public Profile_Data(int Id, string name, int coins, List<int> mainDeck, List<int> secondDeck)
	{
		this.profileID = Id;
		this.username = name;
		this.coinBalance = coins;
		this.activeDeck = mainDeck;
		this.secondaryDeck = secondDeck;
	}
}
