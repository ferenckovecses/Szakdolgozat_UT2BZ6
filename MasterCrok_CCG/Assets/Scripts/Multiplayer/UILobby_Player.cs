using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILobby_Player : MonoBehaviour
{
    [SerializeField] TMP_Text playerNameText;
    Multiplayer_Player player;

    public void SetPlayer(Multiplayer_Player player)
    {
        this.player = player;
        playerNameText.text = player.playerName;
    }
}
