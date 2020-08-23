using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum DexState {List, Detail};

public class CardDex_Controller : MonoBehaviour
{
    //A kártyák és UI elemek tárolói
    public CardFactory factory;
	List<Card> cardList;
	List<GameObject> cardDisplayList = new List<GameObject>();

	//Az enciklopédia státuszát tároló változó
	DexState currentState;

	//Prefabek és referenciák
	public Button cardDisplayPrefab;
	public GameObject cardDetailPrefab;
	public GameObject contentParent;
	public GameObject UI_canvas;

	GameObject displayedUI;

	void Start()
	{
        cardList = factory.GetAllCard();
		currentState = DexState.List;

		//Kártyák megjelenítése egymás után
		foreach (Card card in cardList) 
		{
			Button temp = Instantiate(cardDisplayPrefab, new Vector3(0,0,0), Quaternion.identity, contentParent.transform);
			temp.GetComponent<Image>().sprite = card.GetArt();
			temp.onClick.AddListener(delegate{CardClicked((card.cardID)-1);});
		}

		//Nézet igazítása a lista elejére.
		contentParent.transform.position = new Vector3(0,-1280,0);
	}

	//Kártyákra kattintás esetén behozza a részletes nézetet.
	public void CardClicked(int id)
    {
    	if(currentState == DexState.List)
    	{
    		currentState = DexState.Detail;

    		displayedUI = Instantiate(cardDetailPrefab, new Vector3(640,360,0), Quaternion.identity, UI_canvas.transform);
    		
    		GameObject.Find("Skill").GetComponent<TMP_Text>().text = cardList[id].skillName + ": \n" + cardList[id].skillDescription;
    		GameObject.Find("Name").GetComponent<TMP_Text>().text = cardList[id].cardName;
    		GameObject.Find("Art").GetComponent<Image>().sprite = cardList[id].GetArt();
    		GameObject.Find("Power").GetComponent<TMP_Text>().text = "Erő: " + cardList[id].GetPower().ToString();
    		GameObject.Find("Intelligence").GetComponent<TMP_Text>().text = "Intelligencia: " + cardList[id].GetIntelligence().ToString();
    		GameObject.Find("Reflex").GetComponent<TMP_Text>().text = "Reflex: " + cardList[id].GetReflex().ToString();
    		GameObject.Find("Back").GetComponent<Button>().onClick.AddListener(delegate{BackToList();});
    	}
        
    }

    //Visszalépés a lista nézetbe a részletes nézetből
    public void BackToList()
    {
    	if(currentState == DexState.Detail)
    	{
    		Destroy(displayedUI);
    		currentState = DexState.List;
    	}
    }

    //Visszalépés a főmenübe
    public void BackToMenu()
    {
        SceneManager.LoadScene("Main_Menu");
    }


}
	