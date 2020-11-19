using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSettings_Controller : MonoBehaviour
{
	private int numberOfOpponents;
    public static float drawTempo = 0.01f;
    public static float textTempo = 0.5f;
    public static int winsNeeded = 5;

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
            Debug.Log("Hiba! Érvénytelen ellenség mennyiség: " + newValue.ToString());    
            numberOfOpponents = 1;
        }
    }

    public int GetOpponents()
    {
    	return this.numberOfOpponents;
    }
}
