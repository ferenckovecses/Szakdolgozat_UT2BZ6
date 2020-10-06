using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class Bot_Behaviour
{
	static int randomMaxTreshold = 1000;
	static float errorTreshold = 0.5f;

	//A kézben lévő lapok értékei alapján eldönti, hogy melyik típussal játsszunk
	public static CardStatType ChooseFightType(List<Card> cardsInHand)
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
			return CardStatType.Power;
		}

		else if (intelligence > reflex) 
		{
			return CardStatType.Intelligence;
		}

		else 
		{
			return CardStatType.Reflex;	
		}
	}

	//A megadott típus alapján visszaadja a kézben lévő lapok közül az optimális megoldást
	//Hogy ne legyen túl tökéletes a játék és "emberi hibát" is mutasson esélyt rá hogy random döntsön
	//TODO: Képességeket is nézze és vegye számításba
	public static int ChooseRightCard(List<Card> cardsInHand, CardStatType type)
	{

		int choiceValue = UnityEngine.Random.Range(1,randomMaxTreshold);

		//Véletlen döntés
		if(choiceValue < randomMaxTreshold * errorTreshold)
		{
			return UnityEngine.Random.Range(0, cardsInHand.Count - 1);
		}

		//"Okos" döntés
		else
		{
			int index = 0;
			int cardValue = 0;

			foreach (Card card in  cardsInHand) 
			{
				if(type == CardStatType.Power && card.GetPower() > cardValue)
				{
					cardValue = card.GetPower();
					index = cardsInHand.IndexOf(card);
				}

				else if(type == CardStatType.Intelligence && card.GetIntelligence() > cardValue)
				{
					cardValue = card.GetIntelligence();
					index = cardsInHand.IndexOf(card);
				}

				else if (type == CardStatType.Reflex && card.GetReflex() > cardValue) 
				{
					cardValue = card.GetReflex();
					index = cardsInHand.IndexOf(card);
				}
			}

			return index;
		}
	}

	//Eldönti, hogy a megadott kártyalistából melyik lapot áldozza be
	//TODO: Do the AI stuff
	public static int WhichCardToSacrifice(List<Card> winners)
	{
		int choiceID = UnityEngine.Random.Range(0,(winners.Count)-1);
		return choiceID;
	}

	//Döntést ad arról, hogy a bot mit kezdjen a képességével.
	//TODO: Do the AI stuff
	public static SkillChoises ChooseSkill(int activeCardID, List<Card> cardsInHand, List<Card> cardsOnField, List<List<Card>> opponentCards, int numberOfWinners)
	{
		int choiceValue = UnityEngine.Random.Range(1,randomMaxTreshold);

		if(choiceValue < (randomMaxTreshold * (errorTreshold / 2)))
		{
			return SkillChoises.Pass;
		}

		else if(choiceValue >= (randomMaxTreshold * (errorTreshold / 2)) && choiceValue < (randomMaxTreshold * errorTreshold )
			&& numberOfWinners > 0f)
		{
			return SkillChoises.Store;	
		}

		else 
		{
			return SkillChoises.Use;	
		}

	}

	//Megadja, hogy váltson-e ki a gép, vagy tartsa benn a kártyáját
	//TODO: AI stuff
	public static int HandSwitch(List<Card> cardsInHand, List<Card> activeCards, List<List<Card>> opponentCards, CardStatType stat)
	{
		return UnityEngine.Random.Range(0,cardsInHand.Count-1);
	}

	//Az új információk birtokában megváltoztatja vagy sem a harc típusát
	//TODO: AI stuff
	public static CardStatType ChangeFightType(List<Card> activeCards, List<List<Card>> opponentCards, CardStatType currentStat)
	{
		int result = UnityEngine.Random.Range(0,2);

		switch (result) 
		{
			case 0: return CardStatType.Power;
			case 1: return CardStatType.Intelligence;
			case 2: return CardStatType.Reflex;
			default: return currentStat;
		}
	}

	public static int WhichSkillToUse(List<Card> cardList)
	{
		int choice = UnityEngine.Random.Range(0,cardList.Count-1);
		return cardList[choice].GetCardID();
	}

	public static int WhomToRevive(List<Card> cardList)
	{
		int choice = UnityEngine.Random.Range(0,cardList.Count-1);
		return choice;
	}

	public static int WhomToToss(List<Card> cardList)
	{
		int choice = UnityEngine.Random.Range(0,cardList.Count-1);
		return choice;
	}

	public static int WhichPlayerToExecute(List<int> playerKeys)
	{
		int choice = UnityEngine.Random.Range(0,playerKeys.Count-1);
		return playerKeys[choice];
	}

}
