using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace ClientControll
{
	public class CardListDisplay : MonoBehaviour
	{
		public GameObject displayField;
		public Button cardPrefab;
		public Button backButton;

		private Client client;

		public void SetupList(Client client_in, List<Card> cardList, SkillEffectAction action, int key)
		{
			this.client = client_in;

			foreach (Card card in cardList)
			{
				Button temp = Instantiate(cardPrefab, displayField.transform.position, Quaternion.identity, displayField.transform);
				temp.GetComponent<Image>().sprite = card.GetArt();

				//Listán belüli indexet ad vissza cseréhez, áldozáshoz.
				if (action == SkillEffectAction.Store
					|| action == SkillEffectAction.Switch
					|| action == SkillEffectAction.Revive
					|| action == SkillEffectAction.TossCard)
				{
					temp.onClick.AddListener(delegate { client.ReportCardSelection(cardList.IndexOf(card), key); });
				}

				//A kártya egyedi azonosítóját adja vissza képesség használathoz
				else if (action == SkillEffectAction.SkillUse)
				{
					temp.onClick.AddListener(delegate { client.ReportCardSelection(card.GetCardID(), key); });
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