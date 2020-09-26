using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClientControll
{
	public class CardStatSelectorDisplay : MonoBehaviour
	{
		public Button power;
		public Button intelligence;
		public Button reflex;
		Client client;

		public void SetupPanel(Client client_m)
		{
			this.client = client_m;
			power.onClick.AddListener(delegate { client.ChooseStatButton(CardStatType.Power); });
			intelligence.onClick.AddListener(delegate { client.ChooseStatButton(CardStatType.Intelligence); });
			reflex.onClick.AddListener(delegate { client.ChooseStatButton(CardStatType.Reflex); });
		}
	}
}
