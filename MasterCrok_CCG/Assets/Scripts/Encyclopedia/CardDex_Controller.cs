using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum DexState {List, Detail};

public class CardDex_Controller : MonoBehaviour
{
    //Adattárolók
	private List<Sprite> cardList;
	private List<GameObject> cardDisplayList = new List<GameObject>();
	private DexState currentState;
    private GameObject displayedUI;
    private Button backButton;

	//Prefabek és referenciák
    public Button BackButton;
	public Button cardDisplayPrefab;
	public GameObject cardDetailPrefab;
	public GameObject contentParent;
	public GameObject UI_canvas;
    public CardFactory factory;
	

	void Start()
	{
        cardList = factory.GetAllArt();
		currentState = DexState.List;
		//Kártyák megjelenítése egymás után
		foreach (Sprite card in cardList) 
		{
			Button temp = Instantiate(cardDisplayPrefab,contentParent.transform.position, Quaternion.identity, contentParent.transform);
			temp.GetComponent<Image>().sprite = card;
			temp.onClick.AddListener(delegate{CardClicked(cardList.IndexOf(card));});
		}

		//Nézet igazítása a lista elejére.
		contentParent.transform.position = new Vector3(0,-1280,0);
	}

	//Kártyákra kattintás esetén behozza a részletes nézetet.
	public void CardClicked(int id)
    {
        Card data = factory.GetCardByID(id);
    	if(currentState == DexState.List)
    	{
    		currentState = DexState.Detail;
    		displayedUI = Instantiate(cardDetailPrefab, UI_canvas.transform.position, Quaternion.identity, UI_canvas.transform);
    		displayedUI.GetComponent<CardDisplay_Controller>().SetupDisplay(data, false);

            backButton = Instantiate(BackButton, UI_canvas.transform.position, Quaternion.identity, UI_canvas.transform);
            backButton.onClick.AddListener(delegate{BackToList();});
    	}
        
    }

    //Visszalépés a lista nézetbe a részletes nézetből
    public void BackToList()
    {
    	if(currentState == DexState.Detail)
    	{
            Destroy(backButton.gameObject);
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
	