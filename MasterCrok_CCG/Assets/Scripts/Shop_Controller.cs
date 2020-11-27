using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace ClientSide
{
	public class Shop_Controller : MonoBehaviour
{
	public TMP_Text itemTitle;
	public TMP_Text itemPrice;
	public Image itemImage;
	public TMP_Text balance;
	public CardDatabase cardData;
	public GameObject boughtCardWindowPrefab;
	public List<Chips> chipsData;


	private int index;
	private Profile_Controller profile;
	private List<Card> cardList;
	private GameObject boughtCardWindow;

	private void Awake()
	{
		cardList = new List<Card>();
		index = 0;
		profile = GameObject.Find("Profile_Controller").GetComponent<Profile_Controller>();
		balance.text = profile.GetActivePlayer().GetCoinBalance().ToString();
		RefreshMenu();
	}

	public void Turn(int amount)
	{
		if( (index + amount) >= 0 
			&& (index + amount) <= (chipsData.Count - 1))
		{
			index += amount;
			
		}

		else if ((index + amount) >= (chipsData.Count - 1)) 
		{
			index = 0;
		}

		else if ((index + amount) <= 0) 
		{
			index = (chipsData.Count - 1);
		}

		RefreshMenu();
	}

	private void RefreshMenu()
	{
		itemTitle.text = chipsData[index].GetName();
		itemPrice.text = $"Ár: {chipsData[index].GetPrice().ToString()}";
		itemImage.sprite = chipsData[index].GetImage();
	}

	public void BuyItem()
	{
		if(profile.GetActivePlayer().GetCoinBalance() >= chipsData[index].GetPrice())
		{
			cardList.Clear();

			profile.GetActivePlayer().SpendCoins(chipsData[index].GetPrice());

			int cardNumber = chipsData[index].GetValue() + GetBonus();
			for(var i = 0; i < cardNumber; i++)
			{
				Card temp = cardData.GetRandomCard();
				cardList.Add(temp);
				profile.GetActivePlayer().GetSecondaryDeck().AddCard(temp);
			}

			Profile_Controller.settingsState = ProfileSettings.Silent;
			profile.SaveProfile();
			balance.text = profile.GetActivePlayer().GetCoinBalance().ToString();
			RefreshMenu();

			boughtCardWindow = Instantiate(boughtCardWindowPrefab);
			boughtCardWindow.GetComponent<BoughtCard_Panel>().SetupPanel(cardList);

		}

		else
		{
			Notification_Controller.DisplayNotification("Nem rendelkezel elég érmével!");
		}
	}

	public void Return()
	{
		SceneManager.LoadScene("Main_Menu");
	}

	public void GetHelp()
	{
		Notification_Controller.DisplayNotification("A sajtos Chips 1, a pizzás 3 garantált kártyát ad.\nVan rá esély, hogy ezek mellett egy bónusz lapot is találsz a zacskóban!");
	}

	private int GetBonus()
	{
		int id = UnityEngine.Random.Range(0,999);
		if(id == 69)
		{
			return 1;
		}
		return 0;
	}


}
}

