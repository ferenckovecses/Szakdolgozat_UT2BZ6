using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ClientSide;

namespace GameControll
{
    public class Data_Controller
    {
        //Referenciák
        private CardFactory cardFactory;

        //Adattárolók
        private GameSettings_Controller applicationData;
        private Profile_Controller profileData;
        private int numberOfOpponents;
        private Dictionary<int, Player_Model> playerDictionary;
        private List<int> playerKeyList;
        private System.Random rng;

        public Data_Controller(CardFactory factory_ref)
        {
            this.cardFactory = factory_ref;

            this.applicationData = GameObject.Find("GameData_Controller").GetComponent<GameSettings_Controller>();
            this.profileData = GameObject.Find("Profile_Controller").GetComponent<Profile_Controller>();
            this.playerKeyList = new List<int>();
            this.playerDictionary = new Dictionary<int, Player_Model>();
            this.numberOfOpponents = this.applicationData.GetOpponents();
            rng = new System.Random();
        }

        #region Data Creation

        //A játékos referenciájának eltárolása
        public void AddPlayer()
        {
            int id = profileData.GetActivePlayer().GetUniqueID();
            this.playerDictionary.Add(id, new Player_Model(profileData.GetActivePlayer(), true));
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

        public void ChangeCardOrderInDeck(List<Card> cards, int playerKey, int startIndex)
        {
            List<Card> deck = playerDictionary[playerKey].GetDeck(0);
            int loopAmount = cards.Count-1;
            for(var i = startIndex + loopAmount; i >= startIndex ; i-- )
            {
                deck.RemoveAt(i);
                deck.Insert(i, cards[i]);
            }
        }

        #endregion


        #region Getters

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

        public List<int> GetOtherCardsID(int currentPlayer)
        {
            List<int> temp = new List<int>();

            foreach (int key in GetKeyList())
            {
                if (key != currentPlayer)
                {
                    foreach (Card card in GetCardsFromPlayer(key, CardListTarget.Field)) 
                    {
                        temp.Add(card.GetCardID());
                    }
                }
            }

            return temp;
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


        public int GetHandCount(int playerKey)
        {
            return GetCardsFromPlayer(playerKey, CardListTarget.Hand).Count;
        }

        public int GetNumberOfOpponents()
        {
            return this.numberOfOpponents;
        }

        public int GetWinnerAmount(int playerKey)
        {
            return GetCardsFromPlayer(playerKey, CardListTarget.Winners).Count;
        }

        public int GetLostAmount(int playerKey)
        {
            return GetCardsFromPlayer(playerKey, CardListTarget.Losers).Count;
        }

        public int GetDeckAmount(int playerKey)
        {
            return GetPlayerWithKey(playerKey).GetDeckSize();
        }

        public PlayerTurnRole GetPlayerRole(int key)
        {
            return GetPlayerWithKey(key).GetRole();
        }

        public PlayerTurnStatus GetPlayerStatus(int key)
        {
            return GetPlayerWithKey(key).GetStatus();
        }

        public string GetPlayerName(int key)
        {
            return GetPlayerWithKey(key).GetUsername();
        }

        public Player_Model GetPlayerAtIndex(int index)
        {
            return this.playerDictionary.Values.ElementAt(index);
        }

        public List<PlayerCardPairs> GetOpponentsCard(int currentPlayer)
        {
            List<PlayerCardPairs> activeCardsOnTheField = new List<PlayerCardPairs>();
            foreach (int key in GetKeyList())
            {
                if (key != currentPlayer)
                {
                    int i = 0;
                    foreach (Card card in GetCardsFromPlayer(key, CardListTarget.Field)) 
                    {
                        activeCardsOnTheField.Add(new PlayerCardPairs(key, card, i));
                        i++;
                    }
                }
            }

            return activeCardsOnTheField;
        }

        //Visszaadja a nem aktív játékosok vesztes kártyáit
        public List<PlayerCardPairs> GetOtherLosers(int playerKey)
        {
            List<PlayerCardPairs> temp = new List<PlayerCardPairs>();
            foreach (int key in GetKeyList())
            {
                if (key != playerKey)
                {
                    int i = 0;
                    foreach (Card card in GetCardsFromPlayer(key, CardListTarget.Losers))
                    {
                        temp.Add( new PlayerCardPairs(key, card, i));
                        i++;
                    }
                }

            }

            return temp;
        }

        public Sprite GetLastWinnerImage(int playerKey)
        {
            return this.playerDictionary[playerKey].GetLastWinnerImage();
        }

        public Sprite GetLastLostImage(int playerKey)
        {
            return this.playerDictionary[playerKey].GetLastLostImage();
        }

        public List<Card> GetCardsFromPlayer(int playerKey, CardListTarget target, CardListFilter filter = CardListFilter.None, int limit = 0)
        {
            List<Card> cardList = new List<Card>();
            List<Card> resultList = new List<Card>();

            switch (target) 
            {
                case CardListTarget.Hand: 
                    cardList = GetPlayerWithKey(playerKey).GetCardsInHand();
                    break;

                case CardListTarget.Field: 
                    cardList = GetPlayerWithKey(playerKey).GetCardsOnField();
                    break;

                case CardListTarget.Losers: 
                    cardList = GetPlayerWithKey(playerKey).GetLosers();
                    break;

                case CardListTarget.Deck: 
                    cardList = GetPlayerWithKey(playerKey).GetDeck(limit);
                    break;

                case CardListTarget.Winners: 
                    cardList = GetPlayerWithKey(playerKey).GetWinners();
                    break;

                default:
                  return null;
            }


            foreach (Card card in cardList) 
            {
                resultList.Add(card);
            }

            if(filter != CardListFilter.None)
            {
                resultList = GetFilteredList(resultList, playerKey, filter);
            }

            return resultList;
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

        public Card GetCardFromPlayer(int playerKey, int position, CardListTarget target)
        {
            switch (target) 
            {
                case CardListTarget.Hand: 
                    return this.playerDictionary[playerKey].GetCardFromHand(position);

                case CardListTarget.Field: 
                    return this.playerDictionary[playerKey].GetCardFromField(position);

                case CardListTarget.Losers: 
                    return this.playerDictionary[playerKey].GetCardFromLosers(position);

                case CardListTarget.Deck: 
                    return this.playerDictionary[playerKey].GetCardFromDeck(position);

                default:
                  return null;
            }
        }

        public bool IsCardOnTheField(int playerKey, Card card)
        {
            List<Card> cardsOnField = GetCardsFromPlayer(playerKey, CardListTarget.Field);

            if(cardsOnField.Contains(card))
            {
                return true;
            }

            else 
            {
                return false;    
            }
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
                    foreach (Card card in GetCardsFromPlayer(key, CardListTarget.Field)) 
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

        public System.Random GetRNG()
        {
            return this.rng;
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
