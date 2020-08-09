using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Feladata: A játék vezérlése, a különböző játékfázisok léptetése, a játék UI informálása, illetve a kapott adatok fogadása,
feldolgozása és továbbítása a szerver számára. Hasonló módon a szervertől kapott adatokat is feldolgozza és továbbítja a
UI vezérlőnek, hogy a játékos az aktuális állapotokat láthassa. Kezeli a játékos model-t (pakli és kártyák) és az ott tárolt
információkkal ellátja a UI vezérlőt. 

Adatfolyam:
Játékos -> UI <-> [Játékvezérlő] <-> Szerver || Player model
*/

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
