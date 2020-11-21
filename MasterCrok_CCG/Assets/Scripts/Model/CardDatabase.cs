using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Card Database", menuName = "Card Database")]
public class CardDatabase : ScriptableObject
{
	[SerializeField]
	private List<CardData> cardDataList = new List<CardData>();

	public int GetCardCount()
	{
		return this.cardDataList.Count;
	}

	public Card GetCardByID(int id)
	{
		return new Card(cardDataList[id]);
	}

	public Sprite GetImageByID(int id)
	{
		return this.cardDataList[id].GetArt();
	}

	public List<CardData> GetAllCard()
	{
		return this.cardDataList;
	}

	public List<Sprite> GetAllArt()
	{
		List<Sprite> imageList = new List<Sprite>();
		foreach (CardData card in cardDataList)
		{
			imageList.Add(card.GetArt());
		}
		return imageList;
	}


}
