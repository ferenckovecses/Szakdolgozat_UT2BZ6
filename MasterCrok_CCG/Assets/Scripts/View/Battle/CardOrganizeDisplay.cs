using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace ClientControll
{
	public class CardOrganizeDisplay : MonoBehaviour
	{

		public GameObject cardPanel;
		public GameObject hoverPanel;
		public GameObject placeholderObj;
		public Button finishButton;
		public GameObject cardPrefab;
		public TMP_Text title;
		private List<GameObject> objects;
		private Client client;
		public GameObject cardHovering = null;
		public GameObject cardForSwitch = null;
		public bool switchState;
		private bool dragState;
		private GameObject placeholder;

		public void SetupScreen(Client client_m, string msg)
		{
			client = client_m;
			title.text = msg;
			objects = new List<GameObject>();
			SetHoverPanel(false);
			switchState = false;
			dragState = false;
		}

		public void AddCard(Card card)
		{
			int id = objects.Count + 1;
			GameObject temp = Instantiate(cardPrefab, cardPanel.transform.position, Quaternion.identity, cardPanel.transform);
			temp.GetComponent<OrganizableCard>().SetupCard(card, id, cardPanel, hoverPanel, this);
			temp.name = id.ToString();
			objects.Add(temp);
		}

		public void SetHoverPanel(bool status)
		{
			hoverPanel.SetActive(status);
		}

		public void SetHoveringCard(GameObject obj)
		{
			this.cardHovering = obj;

			placeholder = Instantiate(placeholderObj, cardPanel.transform.position, Quaternion.identity, cardPanel.transform);
			int temp = obj.transform.GetSiblingIndex();
			placeholder.transform.SetSiblingIndex(temp);
			switchState = true;
		}

		public void SetCardToSwitch(GameObject newObj, GameObject setter)
		{
			if(newObj == null)
			{
				//Csak akkor nullázzuk ki, ha a setter a jelenlegi eltárolt GO
				if(this.cardForSwitch == setter)
				{
					this.cardForSwitch = null;
					switchState = false;
				}
			}

			else
			{
				this.cardForSwitch = newObj;
				switchState = true;
			}
		}

		public bool GetSwitchState()
		{
			return this.switchState;
		}

		public void SetDragState(bool newState)
		{
			this.dragState = newState;
			SetHoverPanel(newState);
		}

		public bool GetDragState()
		{
			return this.dragState;
		}

		public void DestroyPlaceholder()
		{
			placeholder.transform.SetParent(hoverPanel.transform);
			Destroy(placeholder);
		}

		public void Switch()
		{
			if(switchState && cardForSwitch != null)
			{
				switchState = false;
				SwitchCards();
			}

			cardHovering = null;
			cardForSwitch = null;
		}

		private void SwitchCards()
		{
			int temp = cardHovering.transform.GetSiblingIndex();
			int temp2 = cardForSwitch.transform.GetSiblingIndex();


			//GameObject order frissítése a Card Fielden
			cardHovering.transform.SetSiblingIndex(temp2);
			cardForSwitch.transform.SetSiblingIndex(temp);

			cardHovering.GetComponent<OrganizableCard>().ResetPosition();
			cardForSwitch.GetComponent<OrganizableCard>().ResetPosition();
		}

		public void Finish()
		{
			List<Card> temp = new List<Card>();
			foreach (Transform child in cardPanel.transform) 
			{
				temp.Add(child.gameObject.GetComponent<OrganizableCard>().GetCardData());
			}
			client.ReportOrderChange(temp);
		}
	}
}
