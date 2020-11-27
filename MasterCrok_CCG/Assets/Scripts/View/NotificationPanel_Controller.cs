using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationPanel_Controller : MonoBehaviour
{
	public GameObject panel;
	public TMP_Text message;
	public Button btn;

	public void ChangeText(string msg)
	{
		message.text = msg;
	}

	public void ChangeFunction(Action func)
	{
		btn.onClick.AddListener(delegate { func(); });
	}


	public void Return()
	{
		Destroy(this.gameObject);
	}
}
