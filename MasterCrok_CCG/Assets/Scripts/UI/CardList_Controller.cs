using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Interface;

public class CardList_Controller : MonoBehaviour
{
	public GameObject displayField;
	public Button cardPrefab;
	public Button backButton;

	private IClient client;

	public void SetupList(IClient client_in, List<Card> cardList, CardSelectionAction action)
	{
		this.client = client_in;

		foreach (Card card in cardList) 
		{
			Button temp = Instantiate(cardPrefab, displayField.transform.position, Quaternion.identity, displayField.transform);
			temp.GetComponent<Image>().sprite = card.GetArt();

			//Listán belüli indexet ad vissza cseréhez, áldozáshoz.
			if(action == CardSelectionAction.Store 
				|| action == CardSelectionAction.Switch
				|| action == CardSelectionAction.Revive)
			{
				temp.onClick.AddListener(delegate { client.ReportCardSelection(cardList.IndexOf(card)); });
			}

			//A kártya egyedi azonosítóját adja vissza képesség használathoz
			else if(action == CardSelectionAction.SkillUse)
			{
				temp.onClick.AddListener(delegate { client.ReportCardSelection(card.GetCardID()); });
			}
		}

		//Visszalépés akciók

		if(action == CardSelectionAction.Store 
			|| action == CardSelectionAction.Switch 
			|| action == CardSelectionAction.SkillUse
			|| action == CardSelectionAction.Revive)
		{
			backButton.onClick.AddListener(delegate {client.ReportSelectionCancel();});
		}

		else if(action == CardSelectionAction.None)
		{
			backButton.onClick.AddListener(delegate { client.CloseCardList(); });
		}

	}
}
