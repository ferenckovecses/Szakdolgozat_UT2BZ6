﻿using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace ClientControll
{
	public class DetailedCardDisplay : MonoBehaviour
	{
		public TMP_Text cardName;
		public TMP_Text skillName;
		public TMP_Text skillDetail;
		public TMP_Text power;
		public TMP_Text intelligence;
		public TMP_Text reflex;
		public GameObject Buttons;
		public Button Use;
		public Button Store;
		public Button Pass;
		public Button Exit;
		public Image art;

		private void Awake()
		{
			Buttons.SetActive(false);
		}

		public void SetupDisplay(Card data, bool skillDecision)
		{
			this.Buttons.SetActive(skillDecision);
			this.cardName.text = data.GetCardName();
			this.skillName.text = data.GetSkillName() + ":";
			this.skillDetail.text = data.GetCardSkill();
			this.power.text = "Erő: " + data.GetPower().ToString();
			this.intelligence.text = "Intelligencia: " + data.GetIntelligence().ToString();
			this.reflex.text = "Reflex: " + data.GetReflex().ToString();
			this.art.sprite = data.GetArt();
		}

		public void SetupButtons(Card data, Client client, int displayedCardID, int fieldKey)
		{
			Use.onClick.AddListener(delegate { client.ReportSkillDecision(SkillState.Use, fieldKey, displayedCardID); });
			Store.onClick.AddListener(delegate { client.ReportSkillDecision(SkillState.Store, fieldKey, displayedCardID); });
			Pass.onClick.AddListener(delegate { client.ReportSkillDecision(SkillState.Pass, fieldKey, displayedCardID); });
			Exit.onClick.AddListener(delegate { client.HideCardDetailsWindow(); });
		}
	}
}