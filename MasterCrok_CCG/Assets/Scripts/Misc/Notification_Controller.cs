﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Notification_Controller : MonoBehaviour
{

	public GameObject notificationPrefab;
	public static GameObject staticPrefab
	{
		get
		{
			return instance.notificationPrefab;
		}
	}

	public static Notification_Controller instance = null;
	private NotificationPanel_Controller panel;

	void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}

        else
        {
            Destroy(gameObject);
        }
	}

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        
    }

    public static void DisplayNotification(string msg, Action func = null)
    {
    	GameObject obj = Instantiate(Notification_Controller.staticPrefab);
    	obj.GetComponent<NotificationPanel_Controller>().ChangeText(msg);

    	if(func != null)
    	{
    		obj.GetComponent<NotificationPanel_Controller>().ChangeFunction(func);
    	}
    }

}
