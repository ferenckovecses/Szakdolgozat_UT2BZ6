using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameControll
{
    public class Data_Controller
    {
        //Referenciák
        private GameState_Controller game;
        private CardFactory cardFactory;

        //Adattárolók
        private GameSettings_Controller applicationData;
        private int numberOfOpponents;
        private Dictionary<int, Player_Model> playerDictionary;
        private List<int> playerKeyList;
        private System.Random rng;

        public Data_Controller(GameState_Controller gameController, CardFactory factory_ref)
        {
            this.game = gameController;
            this.cardFactory = factory_ref;

            this.applicationData = GameObject.Find("GameData_Controller").GetComponent<GameSettings_Controller>();
            this.playerKeyList = new List<int>();
            this.playerDictionary = new Dictionary<int, Player_Model>();
            this.numberOfOpponents = this.applicationData.GetOpponents();
            rng = new System.Random();
        }

        #region Data Creation

        //A játékos referenciájának eltárolása
        public void AddPlayer()
        {
            int id = applicationData.GetActivePlayer().GetUniqueID();
            this.playerDictionary.Add(id, new Player_Model(applicationData.GetActivePlayer(), true));
            this.playerKeyList.Add(id);
        }

        //Generál ellenfeleket
        public void GenerateOpponents()
        {
            List<string> names = new List<string> { "Bot Ond", "Andrew Id", "Robot Gida" };
            for (var i = 0; i < numberOfOpponents; i++)
            {
                Player temp = new Player(names[i]);
                temp.SetUniqueID(i);
                temp.AddActiveDeck(cardFactory.GetStarterDeck());
                Player_Model bot = new Player_Model(temp);
                playerDictionary.Add(i, bot);
                playerKeyList.Add(i);
            }
        }

        #endregion

        #region Data Manipulation

        //Megkeveri a kulcsokat tartalmazó listát, ezzel a játékosok kezdő sorrendjét kialakítva
        public void CreateRandomOrder()
        {
            int n = playerKeyList.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                int tmp = playerKeyList[k];
                playerKeyList[k] = playerKeyList[n];
                playerKeyList[n] = tmp;
            }
        }

        //A sorrend/kulcs lista elejére helyezi a győztest
        public void PutWinnerInFirstPlace(int key)
        {
            playerKeyList.Remove(key);
            playerKeyList.Insert(0, key);
        }

        //A megadott játékos pakliját megkeveri
        public void ShuffleDeck(int key)
        {
            playerDictionary[key].ShuffleDeck(this.rng);
        }

        //Visszapakolja a kártyákat a játékosok paklijába
        public void ResetDecks()
        {
            foreach (int key in GetKeyList())
            {
                if (GetPlayerWithKey(key).GetPlayerStatus())
                {
                    GetPlayerWithKey(key).PutEverythingBack();
                }
            }
        }

        public void SacrificeWinnerCard(int cardID, int playerKey)
        {
            playerDictionary[playerKey].SacrificeAWinner(cardID);
        }

        public void SwitchFromHand(int playerKey, int fieldId, int handId)
        {
            playerDictionary[playerKey].SwitchFromHand(fieldId, handId);
        }

        public void SwitchFromDeck(int playerKey, int fieldId, int deckId)
        {
            playerDictionary[playerKey].SwitchFromDeck(fieldId, deckId);
        }

        public void ReviveLostCard(int playerKey, int cardID)
        {
            playerDictionary[playerKey].ReviveLostCard(cardID);
        }

        public void TossCardFromHand(int playerKey, int cardID)
        {
            playerDictionary[playerKey].TossCard(cardID);
        }

        #endregion


        #region Getters

        public List<int> GetKeyList()
        {
            return this.playerKeyList;
        }

        public List<int> GetOtherKeyList(int currentPlayerKey)
        {
            List<int> temp = new List<int>();
            foreach (int key in GetKeyList()) 
            {
                if(key != currentPlayerKey)
                {
                    temp.Add(key);
                }
            }
            return temp;
        }

        public Dictionary<int, Player_Model> GetPlayers()
        {
            return this.playerDictionary;
        }

        public Player_Model GetPlayerWithKey(int playerKey)
        {
            if (playerKeyList.Contains(playerKey))
            {
                return this.playerDictionary[playerKey];
            }

            else
            {
                return null;
            }
        }

        public int GetHandCount(int playerKey)
        {
            return GetCardsFromHand(playerKey).Count;
        }

        public PlayerTurnRole GetPlayerRole(int key)
        {
            return this.playerDictionary[key].GetRole();
        }

        public PlayerTurnStatus GetPlayerStatus(int key)
        {
            return this.playerDictionary[key].GetStatus();
        }

        public string GetPlayerName(int key)
        {
            return GetPlayerWithKey(key).GetUsername();
        }

        public int GetNumberOfOpponents()
        {
            return this.numberOfOpponents;
        }

        public Player_Model GetPlayerAtIndex(int index)
        {
            return this.playerDictionary.Values.ElementAt(index);
        }

        public List<List<Card>> GetOpponentsCard(int currentPlayer)
        {
            List<List<Card>> activeCardsOnTheField = new List<List<Card>>();
            foreach (int key in GetKeyList())
            {
                if (key != currentPlayer)
                {
                    activeCardsOnTheField.Add(GetPlayerWithKey(key).GetCardsOnField());
                }
            }

            return activeCardsOnTheField;
        }

        public List<int> GetOtherCardsID(int currentPlayer)
        {
            List<int> temp = new List<int>();

            foreach (int key in GetKeyList())
            {
                if (key != currentPlayer)
                {
                    foreach (Card card in GetCardsFromField(key)) 
                    {
                        temp.Add(card.GetCardID());
                    }
                }
            }

            return temp;
        }

        public List<Card> GetWinnerList(int playerKey)
        {
            return this.playerDictionary[playerKey].GetWinners();
        }

        public List<Card> GetLostList(int playerKey)
        {
            return this.playerDictionary[playerKey].GetLosers();
        }

        public Sprite GetLastWinnerImage(int playerKey)
        {
            return this.playerDictionary[playerKey].GetLastWinnerImage();
        }

        public Sprite GetLastLostImage(int playerKey)
        {
            return this.playerDictionary[playerKey].GetLastLostImage();
        }

        public int GetWinnerAmount(int playerKey)
        {
            return GetWinnerList(playerKey).Count;
        }

        public List<int> GetOthersWinAmount(int ownKey)
        {
            List<int> temp = new List<int>();
            foreach (int key in GetKeyList()) 
            {
                if(key != ownKey)
                {
                    temp.Add(GetWinnerAmount(key));
                }     
            } 
            return temp;
        }

        public int GetLostAmount(int playerKey)
        {
            return GetLostList(playerKey).Count;
        }

        public Card GetCardFromHand(int playerKey, int handId)
        {
            return this.playerDictionary[playerKey].GetCardFromHand(handId);
        }

        public Card GetCardFromField(int playerKey, int fieldId)
        {
            return this.playerDictionary[playerKey].GetCardFromField(fieldId);
        }

        public Card GetCardFromLosers(int playerKey, int cardID)
        {
            return this.playerDictionary[playerKey].GetCardFromLosers(cardID);
        }

        public Card GetCardFromDeck(int playerKey, int cardID)
        {
            return this.playerDictionary[playerKey].GetCardFromDeck(cardID);
        }

        public List<Card> GetCardsFromHand(int playerKey, CardListFilter filter = CardListFilter.None)
        {
            List<Card> cardsInHand = this.playerDictionary[playerKey].GetCardsInHand();
            List<Card> temp = new List<Card>();

            foreach (Card card in cardsInHand) 
            {
                temp.Add(card);
            }

            if(filter != CardListFilter.None)
            {
                temp = GetFilteredList(temp, playerKey, filter);
            }

            return temp;

            
        }

        public List<Card> GetCardsFromField(int playerKey)
        {
            return this.playerDictionary[playerKey].GetCardsOnField();
        }

        private List<Card> GetFilteredList(List<Card> cardlist, int playerKey, CardListFilter filter)
        {

            //Ha nem jeleníthetünk meg Master Crockot
            if(filter == CardListFilter.NoMasterCrok)
            {
                cardlist.RemoveAll(card => card.GetCardType() == CardType.Master_Crok);
            }

            //Ha nincs különösebb kitétel arra, hogy milyen lapokat adjunk vissza
            else if(filter == CardListFilter.EnemyDoppelganger)
            {
                List<int> opponentsID = GetOtherCardsID(playerKey);
                cardlist.RemoveAll(card => !(opponentsID.Contains(card.GetCardID()) ));
            }

            return cardlist;
        }

        public int GetDeckAmount(int playerKey)
        {
            return this.playerDictionary[playerKey].GetDeckSize();
        }

        public bool IsCardOnTheField(int playerKey, Card card)
        {
            List<Card> cardsOnField = GetCardsFromField(playerKey);

            if(cardsOnField.Contains(card))
            {
                return true;
            }

            else 
            {
                return false;    
            }
        }

        //Visszaadja a nem aktív játékosok vesztes kártyáit
        public List<PlayerCardPairs> GetOtherLosers(int playerKey)
        {
            List<PlayerCardPairs> temp = new List<PlayerCardPairs>();
            foreach (int key in GetKeyList())
            {
                if (key != playerKey)
                {
                    foreach (Card card in GetLostList(key))
                    {
                        temp.Add( new PlayerCardPairs(key, card));
                    }
                }

            }

            return temp;
        }

        public List<string> GetNameList()
        {
            List<string> names = new List<string>();
            foreach (int key in GetKeyList()) 
            {
                names.Add(playerDictionary[key].GetUsername());
            }

            return names;
        }

        //Megnézi, hogy az ellenfelek kártyáinak a megadott statja magasabb-e mint a mi azonos típusú statunk értéke
        public bool IsMyStatIsTheHighest(int playerKey, int statAmount, CardStatType statType)
        {
            int otherCardStat = 0;

            foreach (int key in GetKeyList()) 
            {
                //Csak az ellenfelek pályájáól kellenek a lapok
                if(key != playerKey)
                {
                    //Megnézünk minden lapot a pályán
                    foreach (Card card in GetCardsFromField(key)) 
                    {
                        //A megadott stat típust nézzük minden alkalommal
                        switch (statType) 
                        {
                            case CardStatType.Power: otherCardStat = card.GetPower(); break;
                            case CardStatType.Intelligence: otherCardStat = card.GetIntelligence(); break;
                            case CardStatType.Reflex: otherCardStat = card.GetReflex(); break;
                            default: break;
                        }

                        //Ha az adott kártya megadott értéke magasabb vagy egyenlő: false
                        if(otherCardStat >= statAmount)
                        {
                            return false;
                        }
                    }
                }
            }

            //Ha végignéztünk mindenkit és nem találtunk magasabbat: true
            return true;
        }

        #endregion



        #region Setters

        public void SetPlayerStatus(int key, PlayerTurnStatus status)
        {
            this.playerDictionary[key].SetStatus(status);
        }

        public void SetPlayerRole(int key, PlayerTurnRole role)
        {
            this.playerDictionary[key].SetRole(role);
        }


        #endregion


    }
}
