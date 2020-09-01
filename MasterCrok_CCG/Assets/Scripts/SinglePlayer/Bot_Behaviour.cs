using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bot_Behaviour
{

	//A kézben lévő lapok értékei alapján eldönti, hogy melyik típussal játsszunk
	public static ActiveStat ChooseFightType(List<Card> cardsInHand)
	{
		//Értékek
		int power = 0;
		int intelligence = 0;
		int reflex = 0;

		foreach (Card card in cardsInHand) 
		{
			if(card.GetPower() > power)
			{
				power = card.GetPower();
			}

			if(card.GetIntelligence() > intelligence)
			{
				intelligence = card.GetIntelligence();
			}

			if(card.GetReflex() > reflex)
			{
				reflex = card.GetReflex();
			}
		}

		//Visszaadja a legmagasabb stat értékkel rendelkező értéket
		if(power > intelligence && power > reflex)
		{
			return ActiveStat.Power;
		}

		else if (intelligence > reflex) 
		{
			return ActiveStat.Intelligence;
		}

		else 
		{
			return ActiveStat.Reflex;	
		}
	}

	//A megadott típus alapján visszaadja a kézben lévő lapok közül az optimális megoldást
	//Hogy ne legyen túl tökéletes a játék és "emberi hibát" is mutasson esélyt rá hogy random döntsön
	public static int ChooseRightCard(List<Card> cardsInHand, ActiveStat type)
	{

		int choiceValue = UnityEngine.Random.Range(1,100);

		if(choiceValue < 40)
		{
			return UnityEngine.Random.Range(0, cardsInHand.Count - 1);
		}

		else
		{
			int index = 0;
			int cardValue = 0;

			foreach (Card card in  cardsInHand) 
			{
				if(type == ActiveStat.Power && card.GetPower() > cardValue)
				{
					cardValue = card.GetPower();
					index = cardsInHand.IndexOf(card);
				}

				else if(type == ActiveStat.Intelligence && card.GetIntelligence() > cardValue)
				{
					cardValue = card.GetIntelligence();
					index = cardsInHand.IndexOf(card);
				}

				else if (type == ActiveStat.Reflex && card.GetReflex() > cardValue) 
				{
					cardValue = card.GetReflex();
					index = cardsInHand.IndexOf(card);
				}
			}

			return index;
		}
	}
	
}
