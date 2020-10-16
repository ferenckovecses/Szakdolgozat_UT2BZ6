using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace ClientControll
{
	public class CardListDisplay : MonoBehaviour
	{
		public GameObject displayField;
		public Button cardPrefab;
		public Button backButton;
		public TMP_Text instructionText;

		private Client client;

		public void SetupList(Client client_in, List<Card> cardList, SkillEffectAction action, int key, string msg)
		{
			this.client = client_in;
			this.instructionText.text = msg;
			
			foreach (Card card in cardList)
			{
				Button temp = Instantiate(cardPrefab, displayField.transform.position, Quaternion.identity, displayField.transform);
				temp.GetComponent<Image>().sprite = card.GetArt();

				//Listán belüli indexet ad vissza cseréhez, áldozáshoz.
				if (action == SkillEffectAction.Store
					|| action == SkillEffectAction.Switch
					|| action == SkillEffectAction.Revive
					|| action == SkillEffectAction.TossCard
					|| action == SkillEffectAction.SkillUse
					|| action == SkillEffectAction.SwitchOpponentCard
					|| action == SkillEffectAction.SacrificeFromHand
					|| action == SkillEffectAction.SacrificeDoppelganger)
				{
					temp.onClick.AddListener(delegate { client.ReportCardSelection(cardList.IndexOf(card), key); });
				}
			}

			//Visszalépés akciók

			if (action == SkillEffectAction.Store
				|| action == SkillEffectAction.Switch
				|| action == SkillEffectAction.SkillUse
				|| action == SkillEffectAction.Revive)
			{
				backButton.onClick.AddListener(delegate { client.ReportSelectionCancel(); });
			}

			else if (action == SkillEffectAction.None)
			{
				backButton.onClick.AddListener(delegate { client.CloseCardList(); });
			}

		}
	}
}