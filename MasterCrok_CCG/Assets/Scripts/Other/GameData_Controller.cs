using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData_Controller : MonoBehaviour
{
	int numberOfOpponents;

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
    }

    public int GetOpponents()
    {
    	return this.numberOfOpponents;
    }
}
