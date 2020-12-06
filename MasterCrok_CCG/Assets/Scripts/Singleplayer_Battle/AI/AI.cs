using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameControll;

namespace ClientControll
{
	public class AI : MonoBehaviour
	{
		private GameState_Controller controller;
		private Player_UI field;

		private string agentName;
		private int playerKey;
		public AI_States currentState;
		public MainGameStates previousState;
		public bool updateNeeded;

		private int choosenPlayersKey;

		//AI kontextus
        private List<Card> cardsOnField;
        private List<PlayerCardPairs> opponentCards;
        private List<Card> cardsInHand;
        private int winAmount;
        private int cardCount;

		private void Awake()
		{
			this.controller = GameObject.Find("BattleController").GetComponent<GameState_Controller>();
			this.field = transform.parent.GetComponent<Player_UI>();
			this.agentName = field.GetName();
			this.currentState = AI_States.Idle;
			this.previousState = MainGameStates.SetupGame;
			this.updateNeeded = false;
		}

	    // Update is called once per frame
	    private void Update()
	    {
	    	//Figyeli, hogy a mi körünk van-e.
	    	CheckForTurn();

	    	if(updateNeeded)
	    	{
	    		updateNeeded = false;

	    		switch (currentState) 
	    		{
	    			case AI_States.Idle: break;
	    			case AI_States.ChooseStat: controller.StartCoroutine(ChooseActiveStat()); break;
	    			case AI_States.SummonCard: Debug.Log($"{agentName}: I summon!"); controller.StartCoroutine(SummonCard()); break;
	    			case AI_States.DecideSkill: Debug.Log($"{agentName}: I decide my card skill!"); controller.StartCoroutine(DecideSkill()); break;
	    			default: break;
	    		}
	    	}   
	    }

	    private void CheckForTurn()
	    {
	    	//Ha az aktív kulcs az övé és az aktuális fázisban még nem váltottunk, illetve engedélyezett az akció
	    	if(this.playerKey == controller.GetCurrentKey()
	    		&& (controller.GetGameState() != previousState)
	    		&& GameState_Controller.readyForAction)
	    	{
	    		//Fázis meghatározása
	    		switch (controller.GetGameState()) 
	    		{
	    			case MainGameStates.SetStat: this.currentState = AI_States.ChooseStat; break;
	    			case MainGameStates.SummonCard: this.currentState = AI_States.SummonCard; break;
	    			case MainGameStates.NormalSkills: this.currentState = AI_States.DecideSkill; break;
	    			default: this.currentState = AI_States.Idle; break;
	    		}
	    		previousState = controller.GetGameState();
	    		updateNeeded = true;
	    	}
	    }

	    private IEnumerator ChooseActiveStat()
	    {
            //Várunk, hogy a főszál beérjen a lockhoz
            while (controller.GetTurnWaitStatus()) 
            {
            	yield return null;
            }
            this.cardsInHand = Module_Controller.instance.GetDataModule().GetCardsFromPlayer(playerKey, CardListTarget.Hand);
            CardStatType stat = Bot_Behaviour.ChooseFightType(this.cardsInHand);
            field.ReportStatChoice(stat);
	    }

	    private IEnumerator SummonCard()
	    {
	    	//Várunk, hogy a főszál beérjen a lockhoz
            while (controller.GetTurnWaitStatus()) 
            {
            	yield return null;
            }

            GetCurrentContext();

            //Gondolkodási idő
            yield return new WaitForSeconds(GameSettings_Controller.drawTempo * UnityEngine.Random.Range(15, 30));

            //Meghozzuk a döntést
            int cardIndex = Bot_Behaviour.ChooseRightCard(this.cardsInHand, controller.GetActiveStat());

            //Lerakatjuk a kártyát a UI-ban
            field.SummonCard(cardIndex);
	    }

	    private IEnumerator DecideSkill()
	    {
	    	//Várunk, hogy a főszál beérjen a lockhoz
            while (controller.GetTurnWaitStatus()) 
            {
            	yield return null;
            }

            GetCurrentContext();
            Debug.Log($"Card count: {cardCount}\nCurrent key: {controller.GetCurrentKey()}");
            for(var id = 0; id < cardCount; id++)
            {
                if(Module_Controller.instance.GetClientModule().AskCardSkillStatus(playerKey, id) == SkillState.NotDecided)
                {
                    //GetSkillChoiceFromAIBrain
                    SkillState choice = Bot_Behaviour.ChooseSkill(id, cardsInHand, cardsOnField, opponentCards, winAmount);
                    Module_Controller.instance.GetInputModule().ReportSkillStatusChange(SkillState.Use, id, false);

                    controller.StartCoroutine(HandleSkills());

                    //Skill lock
                    yield return controller.WaitForEndOfSkill();
                }
            }

            //Gondolkodási idő
            yield return new WaitForSeconds(0.05f);

            //Az AI végzett a körével a döntések után
            Module_Controller.instance.GetDataModule().GetPlayerWithKey(playerKey).SetStatus(PlayerTurnStatus.Finished);

            controller.TurnFinished();
	    }

	    public void AddKey(int newKey)
	    {
	    	this.playerKey = newKey;
	    }

	    //Lekéri az aktuális játékhelyzetet
        private void GetCurrentContext()
        {
            this.cardsOnField = Module_Controller.instance.GetDataModule().GetCardsFromPlayer(playerKey, CardListTarget.Field);
            this.cardCount = cardsOnField.Count;
            this.opponentCards = Module_Controller.instance.GetDataModule().GetOpponentsCard(playerKey);
            this.cardsInHand = Module_Controller.instance.GetDataModule().GetCardsFromPlayer(playerKey, CardListTarget.Hand);
            this.winAmount = Module_Controller.instance.GetDataModule().GetWinnerAmount(playerKey);
        }

        //Kezeli a képeségekkel járó különböző plusz folyamatokat
        private IEnumerator HandleSkills()
        {
        	//Várunk, hogy a főszál beérjen a lockhoz
            while (controller.GetSkillWaitStatus()) 
            {
            	yield return null;
            }

            Debug.Log($"{agentName}: I handle the skill");

            //Ha cseréről van szó
            if (controller.GetCurrentAction() == SkillEffectAction.Switch)
            {
                int handID = -1;

                //Ha kézből cserélünk
                if (controller.GetCurrentListType() == CardListTarget.Hand)
                {
                    handID = Bot_Behaviour.HandSwitch(
                        Module_Controller.instance.GetDataModule().GetCardsFromPlayer(this.playerKey, CardListTarget.Hand),
                        Module_Controller.instance.GetDataModule().GetCardsFromPlayer(this.playerKey, CardListTarget.Field),
                        Module_Controller.instance.GetDataModule().GetOpponentsCard(this.playerKey), 
                        controller.GetActiveStat());

                    Module_Controller.instance.GetSkillModule().SwitchCard(this.playerKey, controller.GetActiveCardID(), handID);
                }
            }

            //Ha skill lopásról van szó
            else if (controller.GetCurrentAction() == SkillEffectAction.SkillUse)
            {
                //Adatgyűjtés
                Card ownCard = Module_Controller.instance.GetDataModule().GetCardFromPlayer(this.playerKey, controller.GetActiveCardID(), CardListTarget.Field);
                List<PlayerCardPairs> cards = Module_Controller.instance.GetDataModule().GetOtherLosers(this.playerKey);
                int index = Bot_Behaviour.WhichSkillToUse(cards);

                Module_Controller.instance.GetInputModule().HandleCardSelection(cards[index].cardPosition, cards[index].playerKey);
                yield break;
            }

            //Ha felélesztésről van szó
            else if (controller.GetCurrentAction() == SkillEffectAction.Revive)
            {
                List<Card> cards = Module_Controller.instance.GetDataModule().GetCardsFromPlayer(this.playerKey, CardListTarget.Losers);
                int cardID = Bot_Behaviour.WhomToRevive(cards);
                Module_Controller.instance.GetInputModule().HandleCardSelection(cardID, this.playerKey);
            }

            else if(controller.GetCurrentAction() == SkillEffectAction.Execute)
             {
                List<int> temp = Module_Controller.instance.GetDataModule().GetOtherKeyList(this.playerKey);
                this.choosenPlayersKey = Bot_Behaviour.WhichPlayerToExecute(temp);

                Module_Controller.instance.GetInputModule().ReportNameBoxTapping(this.choosenPlayersKey);
                yield return controller.WaitForEndOfAction();
             }

             else if(controller.GetCurrentAction() == SkillEffectAction.SwitchOpponentCard)
             {
                List<int> temp = Module_Controller.instance.GetDataModule().GetOtherKeyList(this.playerKey);
                this.choosenPlayersKey = Bot_Behaviour.WhichPlayerToExecute(temp);

                Module_Controller.instance.GetInputModule().ReportNameBoxTapping(this.choosenPlayersKey);
                yield return controller.WaitForEndOfAction();
             }

             else if(controller.GetCurrentAction() == SkillEffectAction.PickCardForSwitch)
             {
                List<Card> cardsOnField = Module_Controller.instance.GetDataModule().GetCardsFromPlayer(this.choosenPlayersKey, CardListTarget.Field);
                int choosenCardID = Bot_Behaviour.WhichCardToSwitch(cardsOnField);
                int deckCount = (Module_Controller.instance.GetDataModule().GetDeckAmount(this.choosenPlayersKey)) - 1;
                controller.SetSwitchType(CardListTarget.Deck);
                Module_Controller.instance.GetSkillModule().SwitchCard(this.choosenPlayersKey, choosenCardID, deckCount);
             }

             else if(controller.GetCurrentAction() == SkillEffectAction.CheckWinnerAmount)
             {
                List<int> keyList = Module_Controller.instance.GetDataModule().GetOtherKeyList(this.playerKey);
                List<int> winAmounts = Module_Controller.instance.GetDataModule().GetOthersWinAmount(this.playerKey);

                int playerID = Bot_Behaviour.WhichPlayerToChoose(winAmounts);
                this.choosenPlayersKey = keyList[playerID];
                Module_Controller.instance.GetInputModule().ReportNameBoxTapping(this.choosenPlayersKey);
             }

            //Ha kézből eldobásról van szó
            else if (controller.GetCurrentAction() == SkillEffectAction.TossCard)
            {
                //Csak akkor dobunk el lapot, ha van a kezünkben
                if(cardsInHand.Count > 1)
                {
                    int cardID = Bot_Behaviour.WhomToToss(cardsInHand);
                    Module_Controller.instance.GetInputModule().HandleCardSelection(cardID, this.playerKey);
                }

                else 
                {
                    Debug.Log("Nincs mit eldobni");
                    controller.ActionFinished();
                }
            }

            else if(controller.GetCurrentAction() == SkillEffectAction.SacrificeFromHand)
            {
                int cardID = Bot_Behaviour.WhomToToss(cardsInHand);
                Module_Controller.instance.GetInputModule().HandleCardSelection(cardID, this.playerKey);
            }

            else if(controller.GetCurrentAction() == SkillEffectAction.SacrificeDoppelganger)
            {
                List<Card> cardList = Module_Controller.instance.GetDataModule().GetCardsFromPlayer(this.playerKey, CardListTarget.Hand, CardListFilter.EnemyDoppelganger);
                if(cardList.Count > 0)
                {
                    int cardID = Bot_Behaviour.WhomToToss(cardList);
                    Module_Controller.instance.GetInputModule().HandleCardSelection(cardID, this.playerKey);
                }

                else 
                {
                    controller.ActionFinished();
                }
            }

            else if(controller.GetCurrentAction() == SkillEffectAction.Reorganize)
            {
                List<Card> cardsInDeck = Module_Controller.instance.GetDataModule().GetCardsFromPlayer(this.playerKey, CardListTarget.Deck, CardListFilter.None, 6);
                cardsInDeck = Bot_Behaviour.OrganiseCardsInDeck(cardsInDeck, Module_Controller.instance.GetDataModule().GetRNG());
                Module_Controller.instance.GetInputModule().HandleChangeOfCardOrder(cardsInDeck, this.playerKey);
            }

            //Skill lock feloldása
            controller.SkillFinished();
        }
	}
}
