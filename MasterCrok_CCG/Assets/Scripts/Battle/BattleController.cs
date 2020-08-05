using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleController : MonoBehaviour
{
	public int numberOfPlayers = 2;
	public BattleUI_Controller UI;

    // Start is called before the first frame update
    void Start()
    {
        UI.CreatePlayerFields(numberOfPlayers);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
