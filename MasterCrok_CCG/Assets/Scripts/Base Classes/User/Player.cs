

[System.Serializable]
public class Player
{
	string username;
	int coinBalance = 100;
	Deck activeDeck;
	Deck secondaryDeck;
	int uniqueID;

	public Player(string username)
	{
		this.username = username;
		this.coinBalance = 100;
		this.activeDeck = new Deck();
		this.secondaryDeck = new Deck();
	}

	public void ChangeName(string newName)
	{
		this.username = newName;
	}

	void SetBalance(int coins)
	{
		this.coinBalance = coins;
	}

	public void AddCoins(int amount)
	{
		this.coinBalance += amount;
	}

	public void SpendCoins(int price)
	{
		this.coinBalance -= price;
	}

	public int GetCoinBalance()
	{
		return this.coinBalance;
	}

	public void AddActiveDeck(Deck newDeck)
	{
		this.activeDeck = newDeck;
	}

	public void AddSecondaryDeck(Deck newDeck)
	{
		this.secondaryDeck = newDeck;
	}

	public Deck GetActiveDeck()
	{
		return this.activeDeck;
	}

	public Deck GetSecondaryDeck()
	{
		return this.secondaryDeck;
	}

	public string GetUsername()
	{
		return this.username;
	}

	public int GetCardCount()
	{
		return (this.activeDeck.GetDeckSize() + this.secondaryDeck.GetDeckSize());
	}

	public void SetUniqueID(int id)
	{
		this.uniqueID = id;
	}

	public int GetUniqueID()
	{
		return this.uniqueID;
	}

	public Profile_Data GetData()
	{
		return new Profile_Data(this.GetUniqueID(), this.GetUsername(),
			this.GetCoinBalance(), this.GetActiveDeck().GetCardList(), this.GetSecondaryDeck().GetCardList());
	}

	public static Player LoadData(Profile_Data data)
	{
		Player newPlayer = new Player(data.username);
		newPlayer.SetUniqueID(data.profileID);
		newPlayer.SetBalance(data.coinBalance);

		return newPlayer;
	}

}
