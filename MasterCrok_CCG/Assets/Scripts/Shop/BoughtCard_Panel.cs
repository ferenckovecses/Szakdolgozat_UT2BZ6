using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoughtCard_Panel : MonoBehaviour
{
	public Image cardImage;
	public GameObject cardPanel;

	public void SetupPanel(List<Card> cardData)
	{
		foreach (Card card in cardData) 
		{
			var temp = Instantiate(cardImage, cardPanel.transform.position, Quaternion.identity, cardPanel.transform);
			temp.sprite = card.GetArt();
		}
	}

	public void Return()
	{
		Destroy(gameObject);
	}
}
