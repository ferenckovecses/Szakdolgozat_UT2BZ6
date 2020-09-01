using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
Feladata: A kártya mozgásait és kártyákra tipikus felhasználói interakciókat, UI effekteket kezelő script.

[Card] ----> PlayerUI Elements ----> UI Controller ----> Battle Controller
*/

public class CardBehaviour : MonoBehaviour
{
	[Header("Státusz változók")]
	bool isDragged;
	bool isAboveActiveField;
	bool activated;
	bool detailedView;

	[Header("Pozíciónáló elemek")]
	Vector2 startingPosition;
	GameObject activeField;
	GameObject startParent;
	GameObject mainCanvas;
	GameObject playerField;
    int siblingIndex;

    [Header("Adatok")]
    Card cardData;
    bool visible;

	private void Awake()
    {
        mainCanvas = GameObject.Find("GameField_Canvas");
        playerField = GameObject.Find("Player");
        this.siblingIndex = gameObject.transform.GetSiblingIndex();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragged)
        {
            if(detailedView)
            {
                detailedView = false;
                HideDetails();
            }

        	//Pozíció változtatása
            gameObject.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            //Minden további elem felett álljon a kártya
            gameObject.transform.SetParent(mainCanvas.transform);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
    	if(collision.gameObject.tag == "PlayerField")
    	{
	        isAboveActiveField = true;
        	activeField = collision.gameObject;
    	}
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
    	if(collision.gameObject.tag == "PlayerField")
    	{
	        isAboveActiveField = false;
	        activeField = null;
    	}
    }

    public void PressCard()
    {
        if(visible)
        {
            detailedView = true;
            GiveDetails();
        }
    }

    public void ReleaseCard()
    {
        detailedView = false;
        HideDetails();
    }

    public void StartDrag()
    {
    	//Megnézzük hogy a kártya a miénk-e, illetve nem aktivált-e már
    	if(playerField == GetFieldOfCard() && !activated
            && playerField.GetComponent<PlayerUIelements>().GetDraggableStatus() )
    	{
	    	this.transform.rotation = Quaternion.identity;
	        startParent = gameObject.transform.parent.gameObject;
	        startingPosition = gameObject.transform.position;
	        isDragged = true;
    	}
    }

    public void EndDrag()
    {
    	if(isDragged)
    	{
	        isDragged = false;
	        detailedView = false;
	        if (isAboveActiveField)
	        {
	            gameObject.transform.SetParent(activeField.transform);
	            ActivateCard();
	        }
	        else
	        {
	            gameObject.transform.position = startingPosition;
	            gameObject.transform.SetParent(startParent.transform);
                //Kézbe visszahelyezéskor eredeti pozícióját foglalja el és rendezzük a kezet
                gameObject.transform.SetSiblingIndex(siblingIndex);
                playerField.GetComponent<PlayerUIelements>().SortHand();
	        }
    	}
    }

    public void ActivateCard()
    {
    	playerField.GetComponent<PlayerUIelements>().PutCardOnField(gameObject);
    	activated = true;

        this.siblingIndex = gameObject.transform.GetSiblingIndex();
    }

    public void SetupCard(Card data)
    {
        this.cardData = data;
    }

    void Reveal()
    {
        gameObject.GetComponent<Image>().sprite = cardData.GetArt();
    }

    GameObject GetFieldOfCard()
    {
    	return this.transform.parent.parent.gameObject;
    }

    void GiveDetails()
    {
        playerField.GetComponent<PlayerUIelements>().DisplayDetails(cardData);
    }

    void HideDetails()
    {
        playerField.GetComponent<PlayerUIelements>().HideDetails();
    }

    public void SetVisibility(bool status)
    {
        this.visible = status;

        if(this.visible)
        {
            Reveal();
        }

    }
}
