using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Chips", menuName = "Chips")]
public class Chips : ScriptableObject
{

	[SerializeField]
	private string itemName = "Chips név";

	[SerializeField]
	private int itemPrice = 0;

	[SerializeField]
	private int itemValue = 1;

	[SerializeField]
	private Sprite itemImage = null;


	public string GetName()
	{
		return this.itemName;
	}

	public int GetPrice()
	{
		return this.itemPrice;
	}

	public int GetValue()
	{
		return this.itemValue;
	}

	public Sprite GetImage()
	{
		return this.itemImage;
	}
}
