using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	TODO:
	-Kártya értékének kalkulálása: (1.5 * aktív érték) + (másik kettő érték összege) + (képesség hasznossági index * képesség használhatóság[0,1]).
	-Képesség hasznossági index: A kártyák által tárolt változó, maunálisan beállított a képesség hasznosságát figyelembe véve.
	-
*/

public static class Bot_Behaviour
{
	static int randomMaxTreshold = 1000;

	//Nehézségi szint szerint változtatható: minél könnyebb a játék, annál több potencionálisan szuboptimális, véletlen lépés
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
	//TODO: Képességeket is nézze és vegye számításba a fenti formula szerint
	public static int ChooseRightCard(List<Card> cardsInHand, CardStatType type)
	{

		int choiceValue = UnityEngine.Random.Range(1,randomMaxTreshold);

		//Véletlen döntés
		if(choiceValue < randomMaxTreshold * errorTreshold)
		{
			return UnityEngine.Random.Range(0, cardsInHand.Count);
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
	//TODO: A legértéktelenebb lapot dobja el, aktív stat szorzó nélkül kalkulált értékekkel
	public static int WhichCardToSacrifice(List<Card> winners)
	{
		int choiceID = UnityEngine.Random.Range(0,winners.Count);
		return choiceID;
	}

	//Döntést ad arról, hogy a bot mit kezdjen a képességével.
	//TODO: Ellenséges lapokat ellenörző behaviour tree
	public static SkillState ChooseSkill(int activeCardID, List<Card> cardsInHand, List<Card> cardsOnField, List<PlayerCardPairs> opponentCards, int numberOfWinners)
	{
		int choiceValue = UnityEngine.Random.Range(1,randomMaxTreshold);

		if(choiceValue < (randomMaxTreshold * (errorTreshold / 2)))
		{
			return SkillState.Pass;
		}

		else if(choiceValue >= (randomMaxTreshold * (errorTreshold / 2)) && choiceValue < (randomMaxTreshold * errorTreshold )
			&& numberOfWinners > 0f)
		{
			return SkillState.Store;
		}

		else 
		{
			return SkillState.Use;	
		}

	}

	//Megadja, hogy váltson-e ki a gép, vagy tartsa benn a kártyáját
	public static int HandSwitch(List<Card> cardsInHand, List<Card> activeCards, List<PlayerCardPairs> opponentCards, CardStatType stat)
	{
		return UnityEngine.Random.Range(0,cardsInHand.Count);
	}

	//Az új információk birtokában megváltoztatja vagy sem a harc típusát
	//TODO: Viselkedés fa: Ellenfelek és saját értékek összevetése, annak a választása, amelyikben a legmagasabbak vagyunk más lapokhoz képest
	public static CardStatType ChangeFightType(List<Card> activeCards, List<PlayerCardPairs> opponentCards, CardStatType currentStat)
	{
		int result = UnityEngine.Random.Range(0,3);

		switch (result) 
		{
			case 0: return CardStatType.Power;
			case 1: return CardStatType.Intelligence;
			case 2: return CardStatType.Reflex;
			default: return currentStat;
		}
	}

	public static int WhichSkillToUse(List<PlayerCardPairs> cardList)
	{
		int index = UnityEngine.Random.Range(0,cardList.Count);
		return index;
	}

	public static int WhomToRevive(List<Card> cardList)
	{
		int choice = UnityEngine.Random.Range(0,cardList.Count);
		return choice;
	}

	public static int WhomToToss(List<Card> cardList)
	{
		int choice = UnityEngine.Random.Range(0,cardList.Count);
		return choice;
	}

	public static int WhichPlayerToExecute(List<int> playerKeys)
	{
		int choice = UnityEngine.Random.Range(0,playerKeys.Count);
		return playerKeys[choice];
	}

	public static int WhichPlayerToChoose(List<int> playerWins)
	{
		int max = 0;
		int index = -1;
		int i = 0;
		foreach (int winAmount in playerWins) 
		{
			if(winAmount > max)
			{
				max = winAmount;
				index = i;
			}
			i++;
		}

		//Ha nincs 0-nál több nyertessel rendelkező, akkor véletlen értéket adunk vissza, mert úgyis mindegy
		if(index == -1)
		{
			return UnityEngine.Random.Range(0,playerWins.Count);
		}

		else {
			return index;
		}
	}

	//Az ellenséges lapok közül a legmagasabb kalkulált értékkel rendelkezőt váltjuk le
	public static int WhichCardToSwitch(List<Card> cards)
	{
		return UnityEngine.Random.Range(0, cards.Count);
	}

	public static List<Card> OrganiseCardsInDeck(List<Card> cards, System.Random rng)
	{
		int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card tmp = cards[k];
            cards[k] = cards[n];
            cards[n] = tmp;
        }

        return cards;
	}



}
