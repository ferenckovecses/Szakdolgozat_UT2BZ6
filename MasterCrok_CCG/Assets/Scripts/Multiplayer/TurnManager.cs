using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class TurnManager : NetworkBehaviour
{
    List<Multiplayer_Player> playerList = new List<Multiplayer_Player>();

    public void AddPlayer(Multiplayer_Player player)
    {
        playerList.Add(player);
    }
}
