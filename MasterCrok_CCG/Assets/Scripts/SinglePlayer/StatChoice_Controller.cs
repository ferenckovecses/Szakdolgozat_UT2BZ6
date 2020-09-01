using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatChoice_Controller : MonoBehaviour
{
	public Button power;
	public Button intelligence;
	public Button reflex;
	BattleUI_Controller UI;


	void Awake()
	{
		UI = GameObject.Find("UI_Controller").GetComponent<BattleUI_Controller>();
	}

	public void SetupPanel()
	{
		power.onClick.AddListener(delegate{UI.ChooseStat(ActiveStat.Power);});
		intelligence.onClick.AddListener(delegate{UI.ChooseStat(ActiveStat.Intelligence);});
		reflex.onClick.AddListener(delegate{UI.ChooseStat(ActiveStat.Reflex);});
	}
}
