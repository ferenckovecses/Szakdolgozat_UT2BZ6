using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Main_Menu_Controller : MonoBehaviour
{

    void Start()
    {
        List<int> test = new List<int>();
    }

	public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenEncyclopedia()
    {
    	SceneManager.LoadScene("Card_Encyclopedia");
    }

    public void StartNewGame()
    {
    	Debug.Log("Majd...");
    }
}
