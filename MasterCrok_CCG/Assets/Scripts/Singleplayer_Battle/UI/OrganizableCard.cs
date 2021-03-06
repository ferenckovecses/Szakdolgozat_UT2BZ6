﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClientControll
{
	public class OrganizableCard : BaseCard
	{
		public Image cardArt;
		public TMP_Text idText;

		private GameObject cardField;
		private GameObject hoverField;
		private int orderID;
		public int siblingIndex;
		private Card data;
		private CardOrganizeDisplay mainPanel;

		private void Awake()
		{
			isDragged = false;
		}

		private void Update()
	    {
	        if (isDragged)
	        {
	        	//Pozíció változtatása
	            gameObject.transform.position = new Vector2(Input.mousePosition.x, startingPosition.y);
                gameObject.transform.SetParent(hoverField.transform);
	        }
	    }

		public void SetupCard(Card card, int id, GameObject cardList_m, GameObject hoverPanel, CardOrganizeDisplay panel_m)
		{
			data = card;
			orderID = id;
			cardArt.sprite = data.GetArt();
			idText.text = orderID.ToString();
			cardField = cardList_m;
			mainPanel = panel_m;
			hoverField = hoverPanel;
			siblingIndex = gameObject.transform.GetSiblingIndex();
		}

		public override void OnCollisionEnter2D(Collision2D collision)
	    {
	    	//Ha pályán lévő kártya
	    	if(!isDragged && mainPanel.GetDragState())
	    	{
	    		mainPanel.SetCardToSwitch(gameObject, gameObject);
	    	}
	   	}

	    public override void OnCollisionExit2D(Collision2D collision)
	    {
	    	if(!isDragged && mainPanel.GetDragState())
	    	{
	    		mainPanel.SetCardToSwitch(null, gameObject);
	    	}
	    }

	    public override void PressCard()
	    {

	    }

	    public override void ReleaseCard()
	    {

	    }

	    public override void StartDrag()
	    {
            startingPosition = gameObject.transform.position;
            mainPanel.SetDragState(true);
            mainPanel.SetHoveringCard(gameObject);
            isDragged = true;
            idText.gameObject.SetActive(false);
	    }

	    public override void EndDrag()
	    {
	    	if(isDragged)
	    	{
	    		isDragged = false;
	    		mainPanel.DestroyPlaceholder();
                gameObject.transform.SetParent(cardField.transform);
            	gameObject.transform.position = startingPosition;
            	gameObject.transform.SetSiblingIndex(siblingIndex);

		        if(mainPanel.GetSwitchState())
                {
                    mainPanel.Switch();
                }

                idText.gameObject.SetActive(true);
	    	}
            mainPanel.SetDragState(false);
	    }

	    public void ResetPosition()
	    {
	    	siblingIndex = gameObject.transform.GetSiblingIndex();
	    	idText.text = (siblingIndex + 1).ToString();
	    }

	    public Card GetCardData()
	    {
	    	return data;
	    }
	}
}
