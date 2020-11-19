using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using GameControll;


namespace ClientSide
{
	public class Profile_Controller : MonoBehaviour
	{
		[Header("Referenciák")]
		public CardFactory factory;
		public GameObject nameFormPrefab;
		private static Profile_Controller instance = null;
		private Player playerProfile;
		private string profileName;
		private string fileName = "NotTheSaveGame.topsecret";
		private Main_Menu_Controller mainMenuController;
		private BinaryFormatter formatter;
		private GameObject nameForm;

		//Statikus állapotváltozók
		public static ProfileSettings settingsState;
		public static bool needsToBeUpdated;

		void Awake()
		{
			if(instance == null)
	        {
	            instance = this;
	        }

	        formatter = new BinaryFormatter();
			mainMenuController = GameObject.Find("Menu_Controller").GetComponent<Main_Menu_Controller>();
			settingsState = ProfileSettings.Default;
			needsToBeUpdated = false;
			LoadProfile();
		}

		void Start()
		{
			DontDestroyOnLoad(this.gameObject);
		}

		void Update()
		{
			if(needsToBeUpdated && settingsState != ProfileSettings.Default)
			{
				needsToBeUpdated = false;
				AskForName();
			}
		}

	    public void LoadProfile()
	    {
	        string path = Path.Combine(Application.persistentDataPath, fileName);

	        //Ellenőrzi, hogy van-e már mentésünk
	        if(File.Exists(path))
	        {
	            FileStream stream = OpenFile(FileMode.Open);
	            Profile_Data data = null;
	            try
	            {
	            	data = formatter.Deserialize(stream) as Profile_Data;
	            }

	            catch(SerializationException e)
	            {
	            	Debug.Log(e);
	            	throw;
	            }
	            
	            stream.Close();

	            //Megnézzük, hogy a betöltött adatok érvényesek-e
	            if(data != default(Profile_Data))
	            {
	            	//Amennyiben igen, úgy betöltjük őket a profilba
	            	Deck active = factory.GetDeckFromList(data.activeDeck);
	            	Deck secondary = factory.GetDeckFromList(data.secondaryDeck);
	            	playerProfile = new Player(data, active, secondary);

	            	//Jelezzük, hogy megtörtént a belépés.
					mainMenuController.PlayerLoginStatus(true);
	            }

	            else
	            {
	                Debug.Log("A betöltés sikertelen!");
	                AskForName();
	                NewProfile();
	            }

	        } 

	        //Ha nincs még mentés file
	        else
	        {
	            Debug.Log("A mentést tartalmazó fájl nem található!");
	            AskForName();
	        }
	    }

	    //Paraméterek: FileMode.Create, FileMode.Open
	    private FileStream OpenFile(FileMode mode)
	    {
	        string path = Path.Combine(Application.persistentDataPath, fileName); 
	       	return new FileStream(path, mode);
	    }



		//Karakter és játék adatok elmentése
	    private void SaveProfile()
	    {
	        FileStream stream = OpenFile(FileMode.Create);
	        Profile_Data data = playerProfile.GetData();

	        try
	        {
	        	formatter.Serialize(stream,data);
	        }

	        catch(SerializationException e)
	        {
	        	Debug.Log(e);
	        	throw;
	        }

	        stream.Close();
	        Debug.Log("Sikeres mentés!");
	    }

	    public void AutoSave(Profile_Data data)
	    {
	        FileStream stream = OpenFile(FileMode.Create);
	        try
	        {
	        	formatter.Serialize(stream,data);
	        }

	        catch(SerializationException e)
	        {
	        	Debug.Log(e);
	        	throw;
	        }

	        stream.Close();
	        Debug.Log("Sikeres automatikus mentés!");
	    }

	    private void AskForName()
	    {
	    	//UI megjelenítése
	    	GameObject mainCanvas = GameObject.Find("MainMenu_Canvas");
	    	nameForm = Instantiate(nameFormPrefab, mainCanvas.transform.position, Quaternion.identity, mainCanvas.transform);
	    }

	    public void SetNewName(string newName)
	    {
	    	profileName = newName;

	    	if(settingsState == ProfileSettings.NewProfile)
	    	{
	    		NewProfile();
	    		settingsState = ProfileSettings.Default;
	    	}

	    	else if(settingsState == ProfileSettings.ChangeName)
	    	{
	    		RefreshName();
	    		settingsState = ProfileSettings.Default;
	    	}
	    }


		private void NewProfile()
		{
			//Létrehozzuk az új játékost
			playerProfile = new Player(profileName);
			playerProfile.AddActiveDeck(factory.GetStarterDeck());
			playerProfile.SetUniqueID(GenerateRandomID());

			//Kiírjuk fájlba
			SaveProfile();

			//Aztán betöltjük a játékba
			LoadProfile();

			//Aztán eltüntetjük a névbekérő mezőt
			Destroy(nameForm);
		}

		private void RefreshName()
		{
			playerProfile.ChangeName(profileName);
			SaveProfile();
		}

		public int GenerateRandomID()
		{
			return UnityEngine.Random.Range(5,9999);
		}

		public Player GetActivePlayer()
	    {
	        return this.playerProfile;
	    }
	}
}