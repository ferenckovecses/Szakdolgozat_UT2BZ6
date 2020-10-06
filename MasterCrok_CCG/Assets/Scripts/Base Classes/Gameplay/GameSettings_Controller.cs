using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSettings_Controller : MonoBehaviour
{
	int numberOfOpponents;
    public Player activePlayer;
    public static float drawTempo = 0.1f;

    private static GameSettings_Controller instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            //Képernyőarányok fixálása
            Screen.SetResolution(1280,720,true);
        }

        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        activePlayer = null;
        DontDestroyOnLoad(this.gameObject);
        numberOfOpponents = 0;
    }

    public void SetOpponents(int newValue)
    {
    	if(newValue >=1 && newValue <= 3)
    	{
    		this.numberOfOpponents = newValue;
    	}
        else 
        {
            Debug.Log("ERROR! Invalid opponent number: " + newValue.ToString());    
            numberOfOpponents = 1;
        }
    }

    public int GetOpponents()
    {
    	return this.numberOfOpponents;
    }

    public void SetActivePlayer(Player player)
    {
        this.activePlayer = player;
    }

    public Player GetActivePlayer()
    {
        return this.activePlayer;
    }
}
