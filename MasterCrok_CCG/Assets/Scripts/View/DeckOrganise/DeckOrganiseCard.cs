using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientSide
{
	public class DeckOrganiseCard : BaseCard
	{
		private GameObject originalParent;
		private bool isAboveDeckField;
		private GameObject deckField;
		private GameObject mainCanvas;
		private Card data;

	    // Start is called before the first frame update
	    void Awake()
	    {
	        mainCanvas = GameObject.Find("DeckOrganise_Panel");
	    }

	    // Update is called once per frame
	    void Update()
	    {
	        if(isDragged)
	        {
	        	gameObject.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
	        	
	        }
	    }

	    public override void OnCollisionEnter2D(Collision2D collision)
	    {
	    	if(collision.gameObject.tag == "PlayerField")
	    	{
		        isAboveDeckField = true;
	        	deckField = collision.gameObject;
	    	}
	    }

	    public override void OnCollisionExit2D(Collision2D collision)
	    {
	    	if(collision.gameObject.tag == "PlayerField")
	    	{
		        isAboveDeckField = false;
		        deckField = null;
	    	}
	    }

	    public override void StartDrag()
	    {
	    	originalParent = gameObject.transform.parent.gameObject;
	    	isDragged = true;
	    	startingPosition = gameObject.transform.position;
	    	gameObject.transform.SetParent(mainCanvas.transform);

	    	if(DeckOrganise_Controller.detailsDisplayed)
	    	{
	    		HideDetails();
	    	}
	    }

	    public override void EndDrag()
	    {
	    	isDragged = false;

	    	//Ha érvénytelen helyen dobtuk le.
	    	if(!isAboveDeckField)
	    	{
	    		gameObject.transform.position = startingPosition;
	    	}

	    	else
	    	{
	    		originalParent = deckField;
	    	}

	    	gameObject.transform.SetParent(originalParent.transform);
	    	DeckOrganise_Controller.updateNeeded = true;
	    	
	    }

	    public override void PressCard()
	    {
	    	DisplayDetails();
	    }

        public override void ReleaseCard()
	    {
	    	HideDetails();
	    }

	    public void AddData(Card card)
	    {
	    	data = card;
	    	gameObject.GetComponent<UnityEngine.UI.Image>().sprite = data.GetArt();
	    }

	    public Card GetData()
	    {
	    	return data;
	    }

	    private void DisplayDetails()
	    {
	    	GameObject.Find("DeckOrganise_Panel").GetComponent<DeckOrganise_Controller>().DisplayDetails(data);
	    }

	    private void HideDetails()
	    {
	    	GameObject.Find("DeckOrganise_Panel").GetComponent<DeckOrganise_Controller>().HideDetails();
	    }
	}
}

