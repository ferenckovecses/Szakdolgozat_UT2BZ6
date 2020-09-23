using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillResponse{Use, Store, Pass};

public static class Bot_Behaviour
{
	static int randomMaxTreshold = 1000;
	static float errorTreshold = 0.8f;

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
				power += card.GetPower();
			}

			if(card.GetIntelligence() > intelligence)
			{
				intelligence += card.GetIntelligence();
			}

			if(card.GetReflex() > reflex)
			{
				reflex += card.GetReflex();
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
	//TODO: Képességeket is nézze és vegye számításba
	public static int ChooseRightCard(List<Card> cardsInHand, ActiveStat type)
	{

		int choiceValue = UnityEngine.Random.Range(1,randomMaxTreshold);

		if(choiceValue < randomMaxTreshold / errorTreshold)
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

	//Döntést ad arról, hogy a bot mit kezdjen a képességével.
	//TODO: Do the AI stuff
	//Check for hand skills, switch, opponent skills and stats, own stats and skill
	public static SkillResponse ChooseSkill(int activeCardID, List<Card> cardsInHand, List<Card> cardsOnField, List<List<Card>> opponentCards)
	{
		int choiceValue = UnityEngine.Random.Range(1,randomMaxTreshold);

		if(choiceValue < randomMaxTreshold / errorTreshold)
		{
			return SkillResponse.Pass;
		}

		else 
		{
			return SkillResponse.Use;	
		}

	}

	//Megadja, hogy váltson-e ki a gép, vagy tartsa benn a kártyáját
	//TODO: AI stuff
	public static int HandSwitch(List<Card> cardsInHand, List<Card> activeCards, List<List<Card>> opponentCards, ActiveStat stat)
	{
		int choiceValue = UnityEngine.Random.Range(1,randomMaxTreshold);

		if(choiceValue < randomMaxTreshold / errorTreshold)
		{
			return UnityEngine.Random.Range(0,cardsInHand.Count-1);
		}

		else 
		{
			return -1;
		}
	}

	//Az új információk birtokában megváltoztatja vagy sem a harc típusát
	public static ActiveStat ChangeFightType(List<Card> activeCards, List<List<Card>> opponentCards, ActiveStat currentStat)
	{
		int result = UnityEngine.Random.Range(0,2);

		switch (result) 
		{
			case 0: return ActiveStat.Power;
			case 1: return ActiveStat.Intelligence;
			case 2: return ActiveStat.Reflex;
			default: return currentStat;
		}
	}

}
