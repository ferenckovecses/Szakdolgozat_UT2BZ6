using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleMenu_Controller : MonoBehaviour
{
	private bool status;
	public Button openButton;
	//X: 750 <<
	//X: 450 <<

	private void Awake()
	{
		status = false;
	}

	public void HandleClicks()
	{
		if(status)
		{
			status = false;
			transform.position = new Vector3(transform.position.x + 310f,transform.position.y, transform.position.z);
			openButton.GetComponentInChildren<TMP_Text>().text = "<<";
		}

		else
		{
			status = true;
			transform.position = new Vector3(transform.position.x - 310f,transform.position.y, transform.position.z);	
			openButton.GetComponentInChildren<TMP_Text>().text = ">>";
		}
	}
}
