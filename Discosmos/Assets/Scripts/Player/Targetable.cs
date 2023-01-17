using System;
using System.Threading.Tasks;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using TMPro;
using Tools;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Targetable : MonoBehaviour
{
    public Enums.Team ownerTeam;
    public bool hideUI;
    public bool isConvoy;
    public HealthBar healthBar;
    public GameObject uiObject;
    public PhotonView masterPhotonView;
    public PhotonView bodyPhotonView;
    public int photonID;
    public int bodyPhotonID;
    public Transform targetableBody;
    public float heightUI;
    public UIType type;

    [Header("CONVOY")] 
    public TextMeshProUGUI pinkAmountText;
    public TextMeshProUGUI greenAmountText;

    private void Awake()
    {
        if (!masterPhotonView)
        {
            masterPhotonView = PhotonView.Get(gameObject);
        }

        bodyPhotonID = bodyPhotonView.ViewID;
        photonID = masterPhotonView.ViewID;
        
        if(!hideUI) CreateUI();
    }

    private void Start()
    {
    }

    public void SetConvoyUI(int pinkAmount, int greenAmount)
    {
        pinkAmountText.text = pinkAmount.ToString();
        greenAmountText.text = greenAmount.ToString();

        if (pinkAmount > greenAmount)
        {
            pinkAmountText.fontSize = 46;
            greenAmountText.fontSize = 36;
        }
        else if (greenAmount > pinkAmount)
        {
            pinkAmountText.fontSize = 36;
            greenAmountText.fontSize = 46;
        }
        else if (greenAmount == pinkAmount)
        {
            pinkAmountText.fontSize = 36;
            greenAmountText.fontSize = 36;
        }
    }

    public void CreateUI()
    {
        switch (type)
        {
            case UIType.PlayerUI:
                healthBar.transform = Instantiate(uiObject, Vector3.zero, quaternion.identity, GameAdministrator.localPlayer.canvas).transform;
                healthBar.healthFill = healthBar.transform.GetChild(0).GetComponent<Image>();
                healthBar.speedFill =  healthBar.transform.GetChild(1).GetComponent<Image>();
                healthBar.healthText =  healthBar.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                healthBar.nameText =  healthBar.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
                healthBar.target =  healthBar.transform.GetChild(4);
                if(healthBar.name != string.Empty) healthBar.nameText.text = healthBar.name;
                
                UpdateUI(true, true, GameAdministrator.localPlayer.currentHealth, GameAdministrator.localPlayer.maxHealth, false, 0, true, GameAdministrator.localPlayer.photonView.Owner.NickName);
                break;
            
            case UIType.ClassicUI:
                healthBar.transform = Instantiate(uiObject, Vector3.zero, quaternion.identity, GameAdministrator.localPlayer.canvas).transform;
                healthBar.healthFill = healthBar.transform.GetChild(0).GetComponent<Image>();
                healthBar.healthText = healthBar.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                healthBar.target = healthBar.transform.GetChild(2);
                break;
            
            case UIType.ConvoyUI:
                healthBar.transform = Instantiate(uiObject, Vector3.zero, quaternion.identity, GameAdministrator.localPlayer.canvas).transform;
                pinkAmountText = healthBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                greenAmountText = healthBar.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                
                break;
        }
       // healthBar.transform.gameObject.SetActive(false);
    }
    public void ShowTarget()
    {
        if(hideUI || isConvoy) return;
        healthBar.target.gameObject.SetActive(true);
    }
    
    public void HideTarget()
    {
        if(hideUI || isConvoy) return;
        healthBar.target.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if(hideUI) return;
        healthBar.transform.position = GameAdministrator.localPlayer._camera.WorldToScreenPoint(targetableBody.position + Vector3.up) + Vector3.up * heightUI;   
    }

    public void UpdateUI(bool updatePos,bool updateHealth = false,int currentHealth = 0,int maxHealth = 0,bool updateSpeed = false,float speed = 0,bool updateName = false, string name = "[not defined]")
    {
        if (hideUI || healthBar == null) return;    
        
        if (updateHealth)
        {
            healthBar.healthFill.fillAmount = currentHealth / (float) maxHealth;
            healthBar.healthText.text = currentHealth + " / " + maxHealth;   
        }
        if (updateSpeed)
        {
            healthBar.speedFill.fillAmount = speed;
        }
        if (updateName)
        {
            healthBar.name = name;
            if(healthBar.nameText) healthBar.nameText.text = name;
        }
    }
}

public enum UIType
{
    PlayerUI,
    ClassicUI,
    ConvoyUI
}

[Serializable]
public class HealthBar
{
    public Transform transform;
    public Image healthFill;
    public Image speedFill;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI nameText;
    public Transform target;
    public string name;
}

