using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Interface;

public class StatChoice_Controller : MonoBehaviour
{
	public Button power;
	public Button intelligence;
	public Button reflex;
	IClient client;

	public void SetupPanel(IClient client_m)
	{
		this.client = client_m;
		power.onClick.AddListener(delegate{ client.ChooseStatButton(ActiveStat.Power);});
		intelligence.onClick.AddListener(delegate{ client.ChooseStatButton(ActiveStat.Intelligence);});
		reflex.onClick.AddListener(delegate{ client.ChooseStatButton(ActiveStat.Reflex);});
	}
}
