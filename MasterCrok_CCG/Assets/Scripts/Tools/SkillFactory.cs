using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Skillekhez kapcsolatos fázis változók
public enum CardListType{None, Deck, Hand, Winners, Losers, Field};
public enum CardListFilter{None, NoMasterCrok};


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
			case 16: ShieldFight(); break;

			//Bármilyen hiba esetén passzolunk, hogy ne akadjon meg a játék
			default: this.gameController.Pass(); Debug.Log("Skill Error!"); break;
		}
	}

	//Cserélhet kézből egy nem Master Crok lapot
	private void UnexpectedAttack()
	{

		//Jelezzük a vezérlőnek, hogy cserére készülünk
		gameController.SetSelectionAction(CardSelectionAction.Switch);
		gameController.SetSwitchType(CardListType.Hand);

		//Ha ember
		if(gameController.IsTheActivePlayerHuman())
		{
			//Megjelenítjük a kézben lévő lapokat, hogy a játékos választhasson közülük
			gameController.ChooseCard(CardListFilter.NoMasterCrok);
		}

		//Ha bot
		else
		{
			gameController.AskBotToSwitch();
		}
	}

	private void PlotTwist()
	{
		//Ha ember
		if(gameController.IsTheActivePlayerHuman())
		{
			//Megjelenítjük a kézben lévő lapokat, hogy a játékos választhasson közülük
			gameController.ChangeActiveStat();
		}

		else 
		{
			
		}
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
