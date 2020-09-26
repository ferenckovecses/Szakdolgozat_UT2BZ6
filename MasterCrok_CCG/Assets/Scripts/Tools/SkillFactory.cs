using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Skillekhez kapcsolatos fázis változók
public enum CardListType{None, Deck, Hand, Winners, Losers, Field};
public enum CardListFilter{None, NoMasterCrok};
public enum CardSelectionAction{None, Store, Switch, Organise, SkillUse, Revive};


public class SkillFactory
{
	private SinglePlayer_Controller gameController;

	public SkillFactory(SinglePlayer_Controller con)
	{
		this.gameController = con;
	}

	public void UseSkill(int skillId)
	{
		switch (skillId) 
		{
			case 1: UnexpectedAttack(); break;
			case 2: PlotTwist(); break;
			case 3: SoulStealing(); break;
			case 5: Revive(); break;
			case 6: GreatFight(); break;
			case 16: ShieldFight(); break;

			//Bármilyen hiba esetén passzolunk, hogy ne akadjon meg a játék
			default: this.gameController.Pass();  break;
		}
	}

	//Cserélhet kézből egy nem Master Crok lapot
	private void UnexpectedAttack()
	{

		//Jelezzük a vezérlőnek, hogy cserére készülünk
		gameController.SetSelectionAction(CardSelectionAction.Switch);
		gameController.SetSwitchType(CardListType.Hand);

		//Megjelenítjük a kézben lévő lapokat, hogy a játékos választhasson közülük
		gameController.ChooseCard(CardListFilter.NoMasterCrok);
	}

	//Megváltoztathatjuk a harc típusát
	private void PlotTwist()
	{
		gameController.StartCoroutine(gameController.TriggerStatChange());
	}

	//Használhatod egy másik játékos vesztes krokjának képességét
	private void SoulStealing()
	{
		if(gameController.IsThereOtherLostCards())
		{
			gameController.SetSelectionAction(CardSelectionAction.SkillUse);
			gameController.SetSwitchType(CardListType.Hand);

			//Ha bot, akkor kell egy kis rásegítés neki
			if(!gameController.IsTheActivePlayerHuman())
			{
				gameController.ChooseCard();
			}
		}

		else 
		{
			gameController.Pass();
		}
	}

	private void Revive()
	{
		if(gameController.DoWeHaveLosers())
		{
			//Jelezzük a vezérlőnek, hogy cserére készülünk
			gameController.SetSelectionAction(CardSelectionAction.Revive);
			gameController.SetSwitchType(CardListType.Losers);

			//Megjelenítjük a kézben lévő lapokat, hogy a játékos választhasson közülük
			gameController.ChooseCard();
		}

		else 
		{
			gameController.Pass();	
		}
	}

	private void GreatFight()
	{
		gameController.GreatFight();
	}

	//Erőre változtatja a harc típusát
	private void ShieldFight()
	{
		this.gameController.SetActiveStat(ActiveStat.Power);
		Response();
	}

	//Jelzünk a játékvezérlőnek, hogy a képesség végzett, mehet a játék tovább
	private void Response()
	{
		gameController.SetPlayerReaction(true);
	}

	
}
