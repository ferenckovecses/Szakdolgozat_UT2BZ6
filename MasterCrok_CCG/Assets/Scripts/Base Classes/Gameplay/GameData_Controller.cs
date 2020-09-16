using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData_Controller : MonoBehaviour
{
	int numberOfOpponents;
    public Player activePlayer;

    private static GameData_Controller instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
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
