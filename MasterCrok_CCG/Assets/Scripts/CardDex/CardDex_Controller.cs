using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDex_Controller : MonoBehaviour
{
	public List<Card> cardList = new List<Card>();

	public GameObject cardDisplayPrefab;
	public GameObject canvasParent;

	List<GameObject> cardDisplayList = new List<GameObject>();

	void Start()
	{
		foreach (Card card in cardList) 
		{
			GameObject temp = Instantiate(cardDisplayPrefab, new Vector3(0,0,0), Quaternion.identity, canvasParent.transform);
			temp.GetComponent<Image>().sprite = card.GetArt();
		}
		canvasParent.transform.position = new Vector3(0,-1280,0);
	}



}
	