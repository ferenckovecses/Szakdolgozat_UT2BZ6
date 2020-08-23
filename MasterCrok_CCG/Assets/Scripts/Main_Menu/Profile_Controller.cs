using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System.Linq;

public class Profile_Controller : MonoBehaviour
{
	[Header("UI Elem Referenciák")]
	public GameObject header;
	public GameObject profileControllWindow;
	public GameObject profileList;
	public TMP_InputField nameField;
	public TMP_Text statusText;
	public GameObject profileAddObject;
	Main_Menu_Controller mainMenuController;

	[Header("Adattárolók")]
	public List<Player> playerProfiles;
	public List<GameObject> playerUIelements;
	bool needsUpdate;

	[Header("Prefab Elemek")]
	public CardFactory factory;
	public GameObject profilePrefab;

	int playerLimit = 100000;

	void Start()
	{
		needsUpdate = true;
		playerProfiles = new List<Player>();
		playerUIelements = new List<GameObject>();
		mainMenuController = GameObject.Find("Menu_Controller").GetComponent<Main_Menu_Controller>();
	}

	void Update()
	{
		if(needsUpdate)
		{
			needsUpdate = false;
		}
	}

	public void NewProfile()
	{
		if(VerifyName())
		{
			//Létrehozzuk az új játékost
			Player newPlayer = new Player(nameField.text);
			newPlayer.AddActiveDeck(factory.GetStarterDeck());
			newPlayer.SetUniqueID(CreateUniqueID());

			//Hozzáadjuk a játékost a listához
			playerProfiles.Add(newPlayer);

			//Kiírjuk fájlba a listát

			//Hozzáadjuk a UI listához az elemet és módosítjuk értékeit, listához adjuk a gameobjectet
			GameObject profile = Instantiate(profilePrefab, profileList.transform.position, Quaternion.identity, profileList.transform);
			profile.transform.Find("Name_Panel/Name_Text").GetComponent<TMP_Text>().text = newPlayer.GetUsername();
			profile.transform.Find("CardAmount_Panel/CardAm_Text").GetComponent<TMP_Text>().text = newPlayer.GetCardCount().ToString();
			profile.transform.Find("CoinAmount_Panel/Coin_Text").GetComponent<TMP_Text>().text = newPlayer.GetCoinBalance().ToString();
			profile.name = newPlayer.GetUsername();
			profile.transform.Find("ProfileSelect_Button").GetComponent<Button>().onClick.AddListener(delegate{ChooseProfile(newPlayer);});
			profile.transform.Find("ProfileDelete_Button").GetComponent<Button>().onClick.AddListener(delegate{RemoveProfile(profile);});
			playerUIelements.Add(profile);

			//Az Új Profil Hozzáadása objektumot a hierarchia aljára tesszük
			profileAddObject.transform.SetAsLastSibling();
		}
	}

	//A kiválasztott profilt aktiválja
	public void ChooseProfile(Player player)
	{
		//Eltároljuk a választott profil adatait a játék számára
		GameData_Controller dataController = GameObject.Find("GameData_Controller").GetComponent<GameData_Controller>();
		dataController.SetActivePlayer(player);

		//A változásról jelentést adunk a főmenünek is, hogy frissüljön
		GameObject.Find("Menu_Controller").GetComponent<Main_Menu_Controller>().PlayerLoginStatus(true);

		//Visszalépünk a Főmenübe
		GoBack();
	}

	public void RemoveProfile(GameObject profile)
	{
		GameData_Controller dataController = GameObject.Find("GameData_Controller").GetComponent<GameData_Controller>();
		int id = playerUIelements.IndexOf(profile);

		//Ha az aktív profilt töröljük
		if(dataController.GetActivePlayer() == playerProfiles[id])
		{
			dataController.SetActivePlayer(default(Player));
			//A változásról jelentést adunk a főmenünek is, hogy frissüljön
			GameObject.Find("Menu_Controller").GetComponent<Main_Menu_Controller>().PlayerLoginStatus(false);
		}

		playerProfiles.RemoveAt(id);
		Destroy(profile);
		playerUIelements.RemoveAt(id);
	}

	public void GoBack()
	{
		Destroy(profileControllWindow);
	}

	public bool VerifyName()
	{
		if(nameField.text.Length > 10)
		{
			statusText.text = "A név túl hosszú! Maximum 10 karakter lehet!";
			return false;
		}
		Regex rgx = new Regex(@"^[A-Za-z0-9]+$");
		if(rgx.IsMatch(nameField.text))
		{
			return true;
		}

		else 
		{
			statusText.text = "Szabálytalan név formátum! Csak számok és az angol ABC betűi szerepelhetnek benne!";
			return false;
		}
	}

	public int CreateUniqueID()
	{
		int newID = UnityEngine.Random.Range(5,playerLimit);
		bool result = playerProfiles.Any(player => player.GetUniqueID() == newID);

		while(result)
		{
			newID = UnityEngine.Random.Range(5,playerLimit);
			result = playerProfiles.Any(player => player.GetUniqueID() == newID);
		}
		return  newID;
	}
}
