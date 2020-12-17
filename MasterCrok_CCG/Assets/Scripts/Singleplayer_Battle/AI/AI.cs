using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameControll;

namespace ClientControll
{
	public class AI : MonoBehaviour
	{
		private GameState_Controller controller;
		private PlayerField field;

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
			this.field = transform.parent.GetComponent<PlayerField>();
			this.agentName = field.GetName();
			this.currentState = AI_States.Idle;
			this.previousState = MainGameStates.SetupGame;
			this.updateNeeded = false;
		}

        public void AddKey(int newKey)
	    {
	    	this.playerKey = newKey;
	    }

	    // Update is called once per frame
	    private void Update()
	    {
	    	RefreshState();

	    	if(updateNeeded)
	    	{
	    		updateNeeded = false;

	    		switch (currentState) 
	    		{
	    			case AI_States.Idle: break;
	    			case AI_States.ChooseStat: controller.StartCoroutine(ChooseActiveStat()); break;
	    			case AI_States.SummonCard: controller.StartCoroutine(SummonCard()); break;
	    			case AI_States.DecideSkill: controller.StartCoroutine(DecideSkill()); break;
	    			default: break;
	    		}
	    	}   
	    }

        //Visszaadja, hogy a bot köre van-e épp
        private bool CheckTurnStatus()
        {
            return (this.playerKey == controller.GetCurrentKey());
        }

	    private void RefreshState()
	    {
	    	//Ha az aktív kulcs alapján a bot következik és az aktuális fázisban még nem frissítettünk
	    	if( CheckTurnStatus()
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

            if(CheckTurnStatus())
            {
                this.cardsInHand = Module_Controller.instance.GetDataModule().GetCardsFromPlayer(playerKey, CardListTarget.Hand);
                CardStatType stat = Bot_Behaviour.ChooseFightType(this.cardsInHand);
                field.ReportStatChoice(stat);
            }
	    }


	    private IEnumerator SummonCard()
	    {
	    	//Várunk, hogy a főszál beérjen a lockhoz
            while (controller.GetTurnWaitStatus()) 
            {
            	yield return null;
            }

            if(CheckTurnStatus())
            {
                GetCurrentContext();

                //Gondolkodási idő
                yield return new WaitForSeconds(GameSettings_Controller.drawTempo * UnityEngine.Random.Range(15, 30));

                //Meghozzuk a döntést
                int cardIndex = Bot_Behaviour.ChooseRightCard(this.cardsInHand, controller.GetActiveStat());

                //Lerakatjuk a kártyát a UI-ban
                field.SummonCard(cardIndex);
            }

	    }

	    private IEnumerator DecideSkill()
	    {
            Debug.Log("Skill decision lock");

	    	//Várunk, hogy a főszál beérjen a lockhoz
            while (controller.GetTurnWaitStatus()) 
            {
            	yield return null;
            }

            Debug.Log("Skill decision started");

            if(CheckTurnStatus())
            {
                GetCurrentContext();
                for(var id = 0; id < cardCount; id++)
                {
                    if(Module_Controller.instance.GetClientModule().AskCardSkillStatus(playerKey, id) == SkillState.NotDecided)
                    {
                        Debug.Log("Card Found!");
                        SkillState choice = Bot_Behaviour.ChooseSkill(id, cardsInHand, cardsOnField, opponentCards, winAmount);
                        Module_Controller.instance.GetInputModule().ReportSkillStatusChange(SkillState.Use, id, false);

                        controller.StartCoroutine(HandleSkills());

                        //Gondolkodási idő
                        yield return new WaitForSeconds(0.05f);

                        //Skill lock
                        yield return controller.WaitForEndOfSkill();
                    }
                }

                //Az AI végzett a körével a döntések után
                Module_Controller.instance.GetDataModule().GetPlayerWithKey(playerKey).SetStatus(PlayerTurnStatus.Finished);

                controller.TurnFinished();
            }

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

        //Jelzés a Botnak, hogy bizonyos akciót el kell végeznie
        public void TriggerAction()
        {
            switch(controller.GetCurrentAction())
            {
                case SkillEffectAction.TossCard: controller.StartCoroutine(TossCard()); break;
                case SkillEffectAction.Switch: controller.StartCoroutine(SwitchCards()); break;
                case SkillEffectAction.StatChange: controller.StartCoroutine(StatChange()); break;
                case SkillEffectAction.Revive: controller.StartCoroutine(ReviveCard()); break;
                case SkillEffectAction.Execute: controller.StartCoroutine(ExecuteCard()); break;
                default: break;
            }
        }

        #region Actions

        //Eldob a kézből egy lapot
        private IEnumerator TossCard()
        {
            //Várunk, hogy a főszál beérjen a lockhoz
            while (controller.GetSkillWaitStatus()) 
            {
            	yield return null;
            }

            //Csak akkor dob el lapot, ha van
            if(cardsInHand.Count >= 1)
            {
                int cardID = Bot_Behaviour.WhomToToss(cardsInHand);
                Module_Controller.instance.GetInputModule().HandleCardSelection(cardID, this.playerKey);
            }

            else 
            {
                controller.ActionFinished();
            }
        }

        //Lapot cserél
        private IEnumerator SwitchCards()
        {
            //Várunk, hogy a főszál beérjen a lockhoz
            while (controller.GetSkillWaitStatus()) 
            {
            	yield return null;
            }

            int handID = -1;

            //Csere kézből
            if (controller.GetCurrentListType() == CardListTarget.Hand)
            {
                //Adatok lekérése
                List<Card> cardsInHand = Module_Controller.instance.GetDataModule().GetCardsFromPlayer(this.playerKey, CardListTarget.Hand);
                List<Card> cardsOnField = Module_Controller.instance.GetDataModule().GetCardsFromPlayer(this.playerKey, CardListTarget.Field);
                List<PlayerCardPairs> opponentCards = Module_Controller.instance.GetDataModule().GetOpponentsCard(this.playerKey);

                //Döntés meghozása
                handID = Bot_Behaviour.HandSwitch(cardsInHand, cardsOnField, opponentCards, controller.GetActiveStat());

                //Cserélés
                Module_Controller.instance.GetInputModule().HandleCardSelection(handID, this.playerKey);
                //Module_Controller.instance.GetSkillModule().SwitchCard(this.playerKey, controller.GetActiveCardID(), handID);
            }

            Debug.Log("Csere volt!");
        }

        //Megváltoztatja a harctípust
        private IEnumerator StatChange()
        {
            //Várunk, hogy a főszál beérjen a lockhoz
            while (controller.GetSkillWaitStatus()) 
            {
            	yield return null;
            }

            GetCurrentContext();

            //AI agy segítségét hívjuk a döntésben
            CardStatType newStat = Bot_Behaviour.ChangeFightType(cardsOnField,
                opponentCards, controller.GetActiveStat());

            Module_Controller.instance.GetInputModule().ReportStatChange(newStat);
        }

        //Visszavesz egy lapot a vesztesek közül
        private IEnumerator ReviveCard()
        {
            //Várunk, hogy a főszál beérjen a lockhoz
            while (controller.GetSkillWaitStatus()) 
            {
            	yield return null;
            }

            List<Card> cards = Module_Controller.instance.GetDataModule().GetCardsFromPlayer(this.playerKey, CardListTarget.Losers);
            int cardID = Bot_Behaviour.WhomToRevive(cards);
            Module_Controller.instance.GetInputModule().HandleCardSelection(cardID, this.playerKey);
        }

        //Eldobat egy lapot egy másik játékossal
        private IEnumerator ExecuteCard()
        {
            //Várunk, hogy a főszál beérjen a lockhoz
            while (controller.GetSkillWaitStatus()) 
            {
            	yield return null;
            }

            List<int> temp = Module_Controller.instance.GetDataModule().GetOtherKeyList(this.playerKey);
            this.choosenPlayersKey = Bot_Behaviour.WhichPlayerToExecute(temp);

            Module_Controller.instance.GetInputModule().ReportNameBoxTapping(this.choosenPlayersKey);

            //Várunk, amíg véget ér a skill
            yield return controller.WaitForEndOfAction();
        }

        #endregion

        //Kezeli a képeségekkel járó különböző plusz folyamatokat
        private IEnumerator HandleSkills()
        {
        	//Várunk, hogy a főszál beérjen a lockhoz
            while (controller.GetSkillWaitStatus()) 
            {
            	yield return null;
            }

            //Ha cseréről van szó
            if (controller.GetCurrentAction() == SkillEffectAction.Switch)
            {
                
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
            //controller.SkillFinished();
        }
	}
}
