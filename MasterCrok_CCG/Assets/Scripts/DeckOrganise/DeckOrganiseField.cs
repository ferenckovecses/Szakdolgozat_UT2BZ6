using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientSide
{
	public class DeckOrganiseField : MonoBehaviour
	{

	    //Új kártyát ad a fieldhez
	    public void AddNewCard(GameObject card)
	    {
	    	card.transform.position = gameObject.transform.position;
	    	card.transform.SetParent(gameObject.transform);
	    }

	    //Visszaadja list a formában a fieldben található kártya adatokat
	    public List<Card> RefreshStatus()
	    {
	    	List<Card> temp = new List<Card>();

	    	foreach (Transform child in gameObject.transform) 
	    	{
	    		temp.Add(child.gameObject.GetComponent<DeckOrganiseCard>().GetData());
	    	}

	    	return temp;
	    }
	}
}

