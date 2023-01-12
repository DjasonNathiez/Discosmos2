using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class InterfaceManager : MonoBehaviourPunCallbacks
{
    [Header("REFERENCES")] 
    public PlayerManager manager;

    [Header("CHAMPION SELECTION")] 
    public GameObject championSelectCanvas;
    public TextMeshProUGUI playerInRoomText;

    [Header("HUD")] 
    public GameObject hud;
    

    public void SetCharacterChoice(string choice)
    {
        switch (choice)
        {
            case "VegaPink":
                manager.SendPlayerTeam(1);
                manager.SendPlayerCharacter(1);
                break;
            
            case "VegaGreen":
                manager.SendPlayerTeam(0);
                manager.SendPlayerCharacter(1);
                break;
            
            case "MimiPink":
                manager.SendPlayerTeam(1);
                manager.SendPlayerCharacter(0);
                break;
            
            case "MimiGreen":
                manager.SendPlayerTeam(0);
                manager.SendPlayerCharacter(0);
                break;
        }
    }
    
    public void UpdatePlayerInRoomCount()
    {
        playerInRoomText.text = PhotonNetwork.CurrentRoom.PlayerCount + "/" + GameAdministrator.instance.playerPerGame;
    }
}
