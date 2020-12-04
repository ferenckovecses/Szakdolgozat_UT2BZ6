using UnityEngine.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "New Main Menu Button", menuName = "Menu Buttons")]
public class MainMenuButtonData : ScriptableObject
{
	public string title;
	public string description;
	public Sprite art;
}
