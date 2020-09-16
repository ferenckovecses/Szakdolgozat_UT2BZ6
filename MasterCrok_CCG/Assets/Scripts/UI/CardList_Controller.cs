using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Interface;

public class CardList_Controller : MonoBehaviour
{
	public GameObject displayField;
	public Button cardPrefab;

	private IClient client;

	public void SetupList(IClient client_in, List<Card> cardList)
	{
		this.client = client_in;
		foreach (Card card in cardList) 
		{
			Button temp = Instantiate(cardPrefab, displayField.transform.position, Quaternion.identity, displayField.transform);
			temp.GetComponent<Image>().sprite = card.GetArt();
			temp.onClick.AddListener(delegate { client.ReportCardSelection(cardList.IndexOf(card)); });
		}
	}
}
