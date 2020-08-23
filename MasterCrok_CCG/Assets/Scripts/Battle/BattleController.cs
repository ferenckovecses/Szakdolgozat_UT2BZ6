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

public enum GamePhase {Preparation, Draw, Summon, Compare, Result};
public enum ActiveStat {Power, Intelligence, Reflex};

public class BattleController : MonoBehaviour
{
    GameData_Controller dataController;
    int numberOfPlayers;
	public BattleUI_Controller UI;
    Dictionary<int, Player_Slot> players;
    ActiveStat currentStat;
    List<int> playerKeys;
    public CardFactory factory;
    GamePhase currentPhase;

    // Start is called before the first frame update
    void Start()
    {
        dataController = GameObject.Find("GameData_Controller").GetComponent<GameData_Controller>();
        playerKeys = new List<int>();
        players = new Dictionary<int,Player_Slot>();

        currentPhase = GamePhase.Preparation;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentPhase) 
        {
            case GamePhase.Preparation: Preparation(); break;
            case GamePhase.Draw: Draw(); break;
            case GamePhase.Summon: break;
            default: break;
        }
    }

    void Preparation()
    {
        AddPlayer();
        GenerateOpponents();
        GenerateUI();
        //CreateRandomOrder();
        currentPhase = GamePhase.Draw;
    }

    void Draw()
    {
        //Paklik megkeverése
        foreach (int key in playerKeys) 
        {
            ShuffleDeck(players[key]);
        }

        //Kezdő 4 lap felhúzása
        StartCoroutine(DrawCardsUp(0.1f));

        currentPhase = GamePhase.Summon;
    }

    IEnumerator DrawCardsUp(float tempo)
    {
        for(var i = 0; i < 4; i++)
        {
            foreach (int key in playerKeys) 
            {
                DrawCard(players[key], key);
                yield return new WaitForSeconds(tempo);
            }
        }
    }


    //A játékos referenciájának eltárolása
    void AddPlayer()
    {
        int id = dataController.GetActivePlayer().GetUniqueID();
        players.Add(id, new Player_Slot(dataController.GetActivePlayer(),true));
        playerKeys.Add(id);
    }

    //Generál ellenfeleket
    void GenerateOpponents()
    {
        numberOfPlayers = dataController.GetOpponents();
        for(var i = 0; i < numberOfPlayers; i++)
        {
            Player temp = new Player("#Bot" +(i+1).ToString());
            temp.SetUniqueID(i);
            temp.AddActiveDeck(factory.GetStarterDeck());
            Player_Slot bot = new Player_Slot(temp);
            players.Add(i, bot);
            playerKeys.Add(i);
        }
    }

    void GenerateUI()
    {
        UI.CreatePlayerFields(numberOfPlayers, playerKeys, players[playerKeys[0]].GetDeckSize());
    }

    //Megkeveri a kulcsokat tartalmazó listát, ezzel a játékosok sorrendjét kialakítva
    void CreateRandomOrder()
    {
        System.Random rng = new System.Random();

        int n = playerKeys.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);
            int tmp = playerKeys[k];
            playerKeys[k] = playerKeys[n];
            playerKeys[n] = tmp;  
        }  
    }

    //Az UI managernek jelez, hogy dobja fel a stat választó képernyőt a megfelelő játékosnak
    void ChangeCurrentStat()
    {

    }

    void ShuffleDeck(Player_Slot player)
    {
        player.ShuffleDeck();
    }

    //Kártya húzás: Modellből adatok lekérése és továbbítása a megfelelő játékos oldalnak.
    void DrawCard(Player_Slot player, int key)
    {
        Card cardData = player.DrawCard();

        if(cardData != null)
        {
            UI.DisplayNewCard(cardData, key, player.GetPlayerStatus());
        }

        //Ha a játékosról van szó továbbítjuk megjelenítésre a deck új méretét
        if(player.GetPlayerStatus())
        {
            int remainingDeckSize = player.GetDeckSize();
            UI.RefreshDeckSize(key, remainingDeckSize);
        }
    }

    public void ResetDecks()
    {
        foreach (int keys in playerKeys) 
        {
            players[keys].PutEverythingBack();
        }
    }


}
