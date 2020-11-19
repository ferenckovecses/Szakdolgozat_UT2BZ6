

[System.Serializable]
public class Player
{
	private string username;
	private int coinBalance;
	private Deck activeDeck;
	private Deck secondaryDeck;
	private int uniqueID;

	public Player(string username)
	{
		this.username = username;
		this.coinBalance = 100;
		this.activeDeck = new Deck();
		this.secondaryDeck = new Deck();
	}

	public Player(Profile_Data data, Deck active, Deck secondary)
	{
		ChangeName(data.username);
		SetUniqueID(data.profileID);
		SetBalance(data.coinBalance);
		AddActiveDeck(active);
		AddSecondaryDeck(secondary);
	}

	public void ChangeName(string newName)
	{
		this.username = newName;
	}

	private void SetBalance(int coins)
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

}
