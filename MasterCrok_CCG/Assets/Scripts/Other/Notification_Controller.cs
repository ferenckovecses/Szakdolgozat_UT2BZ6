﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Notification_Controller : MonoBehaviour
{
	public GameObject panel;
	public TMP_Text message;

	public void ChangeText(string msg)
	{
		message.text = msg;
	}

	public void Return()
	{
		Destroy(panel);
	}
}
