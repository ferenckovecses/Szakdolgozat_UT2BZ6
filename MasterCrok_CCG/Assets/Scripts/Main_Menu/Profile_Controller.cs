using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
	public GameObject notificationPrefab;
	bool needsUpdate;

	[Header("Prefab Elemek")]
	public CardFactory factory;
	public GameObject profilePrefab;

	int playerLimit = 100000;

	void Awake()
	{
		needsUpdate = true;
		playerProfiles = new List<Player>();
		playerUIelements = new List<GameObject>();
		mainMenuController = GameObject.Find("Menu_Controller").GetComponent<Main_Menu_Controller>();
		LoadProfiles();
	}

	void Update()
	{
		if(needsUpdate)
		{
			needsUpdate = false;
		}
	}

    public void LoadProfiles()
    {
        string path = Path.Combine(Application.persistentDataPath, "savedProfiles.data");

        //Ellenőrzi, hogy van-e már mentésünk
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            //this.data = formatter.Deserialize(stream) as Player_Data;
            List<Profile_Data> data = new List<Profile_Data>();
            data = formatter.Deserialize(stream) as List<Profile_Data>;
            stream.Close();

            //Megnézzük, hogy a betöltött adatok érvényesek-e
            if(data != default(List<Profile_Data>))
            {
            	//Ha igen akkor végig loopolunk rajtuk és betöltjük, megjelenítjük őket
            	foreach (Profile_Data profile in data) 
            	{
		    		Player newPlayer = Player.LoadData(profile);
		    		newPlayer.AddActiveDeck(factory.GetDeckFromList(profile.activeDeck));
		    		//newPlayer.AddSecondaryDeck(factory.GetDeckFromList(profile.secondaryDeck));
		    		playerProfiles.Add(newPlayer);
		    		CreatePlayerUI(newPlayer);
            	}
            	
				//Az Új Profil Hozzáadása objektumot a hierarchia aljára tesszük
				profileAddObject.transform.SetAsLastSibling();
            }

            else
            {
                Debug.Log("Loading the game was unsuccessful!");
            }

        } 

        //Ha nincs még mentés file
        else
        {
            Debug.Log("Save file not found!");
        }
    }

	//Karakter és játék adatok elmentése
    public void SaveProfiles()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Path.Combine(Application.persistentDataPath, "savedProfiles.data");
        FileStream stream = new FileStream(path, FileMode.Create);
        List<Profile_Data> data = new List<Profile_Data>();

        foreach (Player profile in playerProfiles) 
        {
    		data.Add(profile.GetData());
        }

        formatter.Serialize(stream,data);
        stream.Close();
        Debug.Log("Save Finished!");
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
			SaveProfiles();

			//Létrehozzuk és hozzáadjuk az új UI elemet a listához
			CreatePlayerUI(newPlayer);

			//Az Új Profil Hozzáadása objektumot a hierarchia aljára tesszük
			profileAddObject.transform.SetAsLastSibling();
		}
	}

	//Hozzáadjuk a UI listához az elemet és módosítjuk értékeit, listához adjuk a gameobjectet
	void CreatePlayerUI(Player newPlayer)
	{
		GameObject profile = Instantiate(profilePrefab, profileList.transform.position, Quaternion.identity, profileList.transform);
		profile.transform.Find("Name_Panel/Name_Text").GetComponent<TMP_Text>().text = newPlayer.GetUsername();
		profile.transform.Find("CardAmount_Panel/CardAm_Text").GetComponent<TMP_Text>().text = newPlayer.GetCardCount().ToString();
		profile.transform.Find("CoinAmount_Panel/Coin_Text").GetComponent<TMP_Text>().text = newPlayer.GetCoinBalance().ToString();
		profile.name = newPlayer.GetUsername();
		profile.transform.Find("ProfileSelect_Button").GetComponent<Button>().onClick.AddListener(delegate{ChooseProfile(newPlayer);});
		profile.transform.Find("ProfileDelete_Button").GetComponent<Button>().onClick.AddListener(delegate{RemoveProfile(profile);});
		playerUIelements.Add(profile);
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
		SaveProfiles();
	}

	public void GoBack()
	{
		Destroy(profileControllWindow);
	}

	public bool VerifyName()
	{
		if(nameField.text.Length > 10)
		{
			string msg = "A név túl hosszú! Maximum 10 karakter lehet!";
			MakeNotification(msg);
			return false;
		}
		Regex rgx = new Regex(@"^[A-Za-z0-9]+$");
		if(rgx.IsMatch(nameField.text))
		{
			return true;
		}

		else 
		{
			string msg = "Szabálytalan név formátum! Csak számok és az angol ABC betűi szerepelhetnek benne!";
			MakeNotification(msg);
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

    void MakeNotification(string msg)
    {
        GameObject temp = Instantiate(notificationPrefab,profileControllWindow.transform.position,Quaternion.identity,profileControllWindow.transform);
        temp.transform.Find("Notification_Controller").GetComponent<Notification_Controller>().ChangeText(msg);
    }
}
