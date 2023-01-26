using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviourPunCallbacks
{
    [Header("REFERENCES")] 
    public PlayerManager manager;

    [Header("CHAMPION SELECTION")] 
    public GameObject championSelectCanvas;
    public TextMeshProUGUI playerInRoomText;
    public UIIndicatorsManager uiIndicatorsManager;
    
    public Color unselectColor = new Color(147, 147, 147, 1);
    public Color selectColor = new Color(255, 255, 255, 1);
    public Image[] champSelectPanel;
    
    [Header("HUD")] 
    public GameObject hud;
    public GameObject scoreCanvas;
    
    public TextMeshProUGUI namePortraitText;
    public Image healthFillAmount;
    public Sprite pinkHealthSprite;
    public Sprite greenHealthSprite;
    public TextMeshProUGUI healthText;
    public Image speedFillAmount;

    public Image portraitImage;
    public Sprite vegaPinkPortrait;
    public Sprite vegaGreenPortrait;
    public Sprite mimiPinkPortrait;
    public Sprite mimiGreenPortrait;

    public Image capacity1fillAmount;
    public GameObject capacity1Mimi;
    public GameObject capacity1Vega;
    public TextMeshProUGUI capacity1text;
    
    public TextMeshProUGUI gameTimerText;
    public double gameTimer = 0;
    
    public TextMeshProUGUI scoreTextPink;
    public TextMeshProUGUI scoreTextGreen;
    public int scorePink = 0;
    public int scoreGreen = 0;

    [Header("READY")]
    public Image buttonReadyImg;
    public TextMeshProUGUI buttonReadyTxt;
    public Sprite readyOnSprite;
    public Sprite readyOffSprite;
    public TMP_FontAsset readyOnFont;
    public TMP_FontAsset readyOffFont;
    public TextMeshProUGUI cSelectReadyCountTxt;

    public GameObject capacityObject;
    public float capacityPumpScale;

    public Transform healthObject;
    public float healthShakeScale;
    public float healthShakeDuration;

    public float portraitShakeScale;
    public float portraitShakeDuration;
    public Color portraitColorTint;
    public Transform portraitTransform;
    
    #region HUD

    public void CapacityPumpScale()
    {
        capacityObject.transform.DOPunchScale(capacityObject.transform.localScale * capacityPumpScale, 0.5f);
    }

    public void HealthShakeOnDamage()
    {
        portraitTransform.DOShakeScale(portraitShakeDuration, portraitShakeScale);
        portraitImage.DOColor(portraitColorTint, 0.2f).OnComplete(() => { portraitImage.DOColor(Color.white, 0.2f);});
        healthFillAmount.DOColor(portraitColorTint, 0.2f).OnComplete(() => { healthFillAmount.DOColor(Color.white, 0.2f);});
        healthObject.DOShakeScale(healthShakeDuration, healthShakeScale);
    }
    
    public void InitializeHUD(int currentHealth, int maxHealth, float currentSpeed, string nickname)
    {
        hud.SetActive(true);
        namePortraitText.text = nickname;

        healthFillAmount.fillAmount = (float) currentHealth / maxHealth;
        healthText.text = currentHealth + "/" + maxHealth;
        speedFillAmount.fillAmount = currentSpeed;
        
        gameTimerText.text = gameTimer.ToString();
        
        scoreTextPink.text = scorePink.ToString();
        scoreTextGreen.text = scoreGreen.ToString();
    }

    public void SetHUD(Enums.Characters character, Enums.Team team)
    {
        switch (team)
        {
            case Enums.Team.Green:
                healthFillAmount.sprite = greenHealthSprite;
                
                switch (character)
                {
                    case Enums.Characters.Mimi:
                        portraitImage.sprite = mimiGreenPortrait;
                        
                        capacity1Mimi.SetActive(true);
                        capacity1Vega.SetActive(false);
                        break;
                    
                    case Enums.Characters.Vega:
                        portraitImage.sprite = vegaGreenPortrait;

                        capacity1Mimi.SetActive(false);
                        capacity1Vega.SetActive(true);
                        break;
                }
                break;
            
            case Enums.Team.Pink:
                healthFillAmount.sprite = pinkHealthSprite;
                
                switch (character)
                {
                    case Enums.Characters.Mimi:
                        portraitImage.sprite = mimiPinkPortrait;
                        
                        capacity1Mimi.SetActive(true);
                        capacity1Vega.SetActive(false);
                        break;
                    
                    case Enums.Characters.Vega:
                        portraitImage.sprite = vegaPinkPortrait;

                        capacity1Mimi.SetActive(false);
                        capacity1Vega.SetActive(true);
                        break;
                }
                break;
        }
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
    
    public void UpdateGameTimer(float timer)
    {
        gameTimer = (int)(GameManager.instance.gameTimer - timer);
        gameTimerText.text = Mathf.FloorToInt(GameManager.instance.gameTimer - timer).ToString();
    }

    public void UpdateScore(int green, int pink)
    {
        scorePink = pink;
        scoreGreen = green;
        scoreTextPink.text = scorePink.ToString();
        scoreTextGreen.text = scoreGreen.ToString();
        scoreTextGreen.transform.DOPunchScale(scoreTextGreen.transform.localScale * 1.2f, 0.5f);
        scoreTextPink.transform.DOPunchScale(scoreTextPink.transform.localScale * 1.2f, 0.5f);
    }
    

    #endregion

    public void Ready()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        
        if (gm)
        {
            gm.ReadyButton();
        }
    }

    public void SwitchColorsChamp(int index)
    {
        for (int i = 0; i < champSelectPanel.Length; i++)
        {
            if (i == index)
            {
                champSelectPanel[i].transform.GetChild(0).GetComponent<Image>().color = selectColor;
                champSelectPanel[i].color = selectColor;
            }
            else
            {
                champSelectPanel[i].transform.GetChild(0).GetComponent<Image>().color = unselectColor;
                champSelectPanel[i].color = unselectColor;
            }
        }
    }
    
    public void SwitchUIReady(bool isReady)
    {
        switch (isReady)
        {
            case true:
                buttonReadyImg.sprite = readyOnSprite;
                buttonReadyTxt.font = readyOnFont;
                break;
            
            case false:
                buttonReadyImg.sprite = readyOffSprite;
                buttonReadyTxt.font = readyOffFont;
                break;
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
      //  playerInRoomText.text = PhotonNetwork.CurrentRoom.PlayerCount + "/" + GameAdministrator.instance.playerPerGame;
    }

    public void CreateIndics(PlayerManager manager)
    {
        int sprite;
        int arrow;
        if (manager.currentCharacter == Enums.Characters.Mimi)
        {
            if (manager.currentTeam == Enums.Team.Green)
            {
                sprite = 0;
                arrow = 4;
            }
            else
            {
                sprite = 2;
                arrow = 5;
            }
        }
        else
        {
            if (manager.currentTeam == Enums.Team.Green)
            {
                sprite = 1;
                arrow = 4;
            }
            else
            {
                sprite = 3;
                arrow = 5;
            }
        }
        
        uiIndicatorsManager.AddIndic(manager.controller.gameObject,sprite,arrow,out int indicNb);
    }
}
