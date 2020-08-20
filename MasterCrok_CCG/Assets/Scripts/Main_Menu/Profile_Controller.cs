using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Profile_Controller : MonoBehaviour
{
	public GameObject header;
	public List<Player> playerProfiles;
	public GameObject panel;

	void Start()
	{
		playerProfiles = new List<Player>();
		
		if(playerProfiles.Count < 1)
		{
			header.SetActive(false);
		}
	}

	void Update()
	{

	}

	public void NewProfile()
	{

	}

	public void GoBack()
	{
		Destroy(panel);
	}
}
