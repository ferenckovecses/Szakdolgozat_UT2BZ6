using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using ClientControll;

namespace ClientSide
{
	public class DeckOrganise_Controller : MonoBehaviour
	{
		[Header("Referenciák")]
		public TMP_Text activeCardCounter;
		public TMP_Text secondaryCardCounter;
		public DeckOrganiseField activeCardField;
		public DeckOrganiseField secondaryCardField;
		public Button saveButton;
		public Button backButton;
		public Button rulesButton;

		[Header("Prefabok")]
		public GameObject cardDetailPrefab;
		public GameObject cardPrefab;

		private GameObject detailWindow;
		private Profile_Controller profile;
		private DeckStatus deckStatus = default(DeckStatus);
		private List<Card> activeCards;
		private List<Card> secondaryCards;
		private bool unsavedChanges;

		public static bool updateNeeded;
		public static bool detailsDisplayed;

		private void Awake()
		{
			activeCards = new List<Card>();
			secondaryCards = new List<Card>();
			unsavedChanges = false;
			detailsDisplayed = false;
		}

	    // Start is called before the first frame update
	    private void Start()
	    {
	        profile = GameObject.Find("Profile_Controller").GetComponent<Profile_Controller>();
	        CreateCards();
	        RefreshCounters();
	        VerifyDeck();
	        HandleSave();
	    }

	    // Update is called once per frame
	    private void Update()
	    {
	        if(updateNeeded)
	        {
	        	updateNeeded = false;
	        	unsavedChanges = true;
	        	UpdateLists();
	        	RefreshCounters();
	        	VerifyDeck();
	        	HandleSave();
	        }
	    }

	    private void UpdateLists()
	    {
	    	activeCards = activeCardField.RefreshStatus();
	    	secondaryCards = secondaryCardField.RefreshStatus();
	    }

	    private void RefreshCounters()
	    {
	    	activeCardCounter.text = $"{activeCards.Count} db.";
	    	secondaryCardCounter.text = $"{secondaryCards.Count} db.";
	    }

	    private void CreateCards()
	    {
	    	activeCards = profile.GetActivePlayer().GetActiveDeck().GetDeck();
	    	foreach (Card card in activeCards) 
	    	{
	    		GameObject temp = Instantiate(cardPrefab);
	    		temp.GetComponent<DeckOrganiseCard>().AddData(card);
	    		activeCardField.AddNewCard(temp);
	    	}

	    	secondaryCards = profile.GetActivePlayer().GetSecondaryDeck().GetDeck();
	    	foreach (Card card in secondaryCards) 
	    	{
	    		GameObject temp = Instantiate(cardPrefab);
	    		temp.GetComponent<DeckOrganiseCard>().AddData(card);
	    		secondaryCardField.AddNewCard(temp);
	    	}
	    }

	    public void BackButton()
	    {
	    	SceneManager.LoadScene("Main_Menu");
	    }

	    public void RulesButton()
	    {
	    	Notification_Controller.DisplayNotification("A pakli minimális mérte: 10 kártya.\n10 kártyánként 1 példány lehet csak minden lapból.\n(Példa: 20 lapnál 2 lehet maximum minden lapból)");
	    }

	    public void SaveButton()
	    {
	    	if(unsavedChanges)
	    	{
	    		unsavedChanges = false;
		    	Deck active = new Deck();
		    	foreach(Card card in activeCards)
		    	{
		    		active.AddCard(card);
		    	}
		    	profile.GetActivePlayer().AddActiveDeck(active);

		    	Deck secondary = new Deck();
		    	foreach (Card card in secondaryCards) 
		    	{
		    		secondary.AddCard(card);
		    	}
		    	profile.GetActivePlayer().AddSecondaryDeck(secondary);

		    	profile.SaveProfile();
	    	}

	    }

	    private void VerifyDeck()
	    {
	    	if(activeCards.Count < 10 || !DuplicateVerification())
	    	{
	    		deckStatus = DeckStatus.Illegal;
	    	}

	    	else
	    	{
	    		deckStatus = DeckStatus.Verified;	
	    	}
	    }

	    private bool DuplicateVerification()
	    {
	    	var maxValue =  (from x in activeCards
	    					group x by x.GetCardID() into g
	    					let count = g.Count()
	    					orderby count descending
	    					select new {Count = count}).Take(1);

	    	foreach (var x in maxValue) 
			{
				if(x.Count > (int)(activeCards.Count / 10))
				{
					return false;
				}
			}				
			

	    	return true;
	    }

	    private void HandleSave()
	    {
	    	if(deckStatus == DeckStatus.Illegal)
	    	{
	    		saveButton.gameObject.SetActive(false);
	    	}

	    	else
	    	{
	    		saveButton.gameObject.SetActive(true);
	    	}
	    }

	    public void DisplayDetails(Card data)
	    {
	    	detailsDisplayed = true;
	    	detailWindow = Instantiate(cardDetailPrefab);
	    	detailWindow.GetComponent<DetailedCardDisplay>().SetupDisplay(data, false);

	    }

	    public void HideDetails()
	    {
	    	detailsDisplayed = false;
	    	Destroy(detailWindow);
	    }
	}
}

