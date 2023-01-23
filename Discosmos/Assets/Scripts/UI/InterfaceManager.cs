using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviourPunCallbacks
{
    [Header("REFERENCES")] 
    public PlayerManager manager;

    [Header("CHAMPION SELECTION")] 
    public GameObject championSelectCanvas;
    public TextMeshProUGUI playerInRoomText;

    [Header("HUD")] 
    public GameObject hud;

    public TextMeshProUGUI namePortraitText;
    public Image healthFillAmount;
    public TextMeshProUGUI healthText;
    public Image speedFillAmount;

    public Image capacity1fillAmount;
    public TextMeshProUGUI capacity1text;

    #region HUD

    public void InitializeHUD(int currentHealth, int maxHealth, float currentSpeed, string nickname)
    {
        hud.SetActive(true);
        namePortraitText.text = nickname;

        healthFillAmount.fillAmount = (float) currentHealth / maxHealth;
        healthText.text = currentHealth + "/" + maxHealth;
        speedFillAmount.fillAmount = currentSpeed;
    }
    
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        healthFillAmount.fillAmount = (float) currentHealth / maxHealth;
        healthText.text = currentHealth + "/" + maxHealth;
    }

    public void UpdateSpeedBar(float currentSpeed)
    {
        speedFillAmount.fillAmount = currentSpeed;
    }

    public void SetCapacityImageOnCooldown()
    {
        capacity1text.gameObject.SetActive(true);
    }

    public void UnsetCapacityImageOnCooldown()
    {
        capacity1text.gameObject.SetActive(false);
    }
    
    public void UpdateCapacity1Image(float timer, float duration)
    {
        capacity1fillAmount.fillAmount = timer / duration;
        capacity1text.text = (Mathf.FloorToInt(duration - timer)).ToString();
    }
    
    public void UpdatePortrait()
    {
        
    }
    

    #endregion

    public void Ready()
    {
        GameManager gm = FindObjectOfType<GameManager>();

        if (gm)
        {
            gm.ReadyCheck();
        }
    }
    
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
