using System.Threading.Tasks;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Tools;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [Header("REFERENCES")] 
    public PlayerController controller;
    public Camera _camera;
    public GameObject cameraObject;
    public PhotonView pView;
    public InterfaceManager interfaceManager;
    
    public GameObject sparkles;
    
    [Header("STATE")] 
    public bool isCasting;
    public Enums.Characters currentCharacter;
    public static bool isLoaded;

    [Header("PLAYER")] 
    public string username;
    public Enums.Team currentTeam;

    [Header("CAPACITIES")] 
    public ActiveCapacitySO capacity1;
    public bool capacity1InCooldown;
    public GameObject capacity1Visu;
    public ActiveCapacitySO capacity2;
    public bool capacity2InCooldown;
    public GameObject capacity2Visu;
    public UtilityDelegate.OnCooldown OnCooldownCapacity1;
    public UtilityDelegate.OnCooldown OnCooldownCapacity2;

    [Header("DATA")] 
    public CharacterDataSO mimiData;
    public CharacterDataSO vegaData;
    public static CharacterDataSO currentData;

    #region STATE DATA

    public int currentHealth;
    public int maxHealth;
    public int currentShield;

    #endregion
    
    #region MOVEMENT DATA
    
    public float currentSpeed;
    public float baseSpeed;
    public float force;
    private float visualForce;
    public AnimationCurve speedCurve;
    public AnimationCurve slowDownCurve;

    #endregion
    
    #region COMBAT DATA

    public int attackDamage;
    public float attackRange;
    public float attackSpeed;
    public AnimationCurve damageMultiplier;
    public float pushForce;

    #endregion

    [Header("INTERFACE")]
    public Transform canvas;
    public GameObject textDamage;   
    public GameObject localCanvas;
    
    [Header("VISUAL")]
    public GameObject mimiModel;
    public GameObject vegaModel;
    
    public AnimationController currentAnimationController;
    public AnimationController mimiAnimationController;
    public AnimationController vegaAnimationController;

    public ParticleSystem pointerVFX;

    #region UNITY METHODS

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        if(pView == null) pView = PhotonView.Get(gameObject);
    }
    
    private void Start()
    {
        name = "Client" + "_" + pView.Owner.NickName + "_" + pView.ViewID + "_IsMine : " + pView.IsMine;
        
        WaitNextFrame();
        
    }
    
    async void WaitNextFrame()
    {
        await Task.Yield();
        
        if (currentCharacter != Enums.Characters.Null)
        {
            controller.myTargetable.healthBar.transform.gameObject.SetActive(true);
        }
        
      //  controller.myTargetable.UpdateUI(false, true, currentHealth, maxHealth, false, 0, true, photonView.Owner.NickName);
        interfaceManager.UpdatePlayerInRoomCount();
    }
    
    private void Update()
    {
        if (controller.enabled)
        {
            currentSpeed = speedCurve.Evaluate(force) + baseSpeed;
            controller.agent.speed = currentSpeed;
        }
        
        switch (currentCharacter)
        {
            case Enums.Characters.Mimi:
                visualForce = controller.mimiAnimator.GetFloat("Force");
                break;
                    
            case Enums.Characters.Vega:
                visualForce = controller.vegaAnimator.GetFloat("Force");
                break;
        }
        controller.myTargetable.UpdateUI(false, false, 0, 0, true, visualForce);
        interfaceManager.UpdateSpeedBar(visualForce);
        
        
        OnCooldownCapacity1?.Invoke();
        OnCooldownCapacity2?.Invoke();
        
    }

    #endregion

    #region LOCAL METHODS
    
    public void Initialize()
    {
        if (pView == null)
        {
            pView = GetComponent<PhotonView>();
        }
        
        if (pView.IsMine)
        {
            _camera.enabled = true;
            controller.agent.enabled = true;
            localCanvas.SetActive(true);
            cameraObject.SetActive(true);
        }
    }

    public void EnablePlayer()
    {
        if (pView.IsMine)
        {
            controller.enabled = true;
            controller.EnableMovement();
        }
        
    }

    public void SetData()
    {
        maxHealth = currentData.maxHealth;
        
        baseSpeed = currentData.baseSpeed;
        speedCurve = currentData.speedCurve;
        slowDownCurve = currentData.slowDownCurve;

        attackDamage = currentData.attackDamage;
        attackRange = currentData.attackRange;
        attackSpeed = currentData.attackSpeed;
        damageMultiplier = currentData.damageMultiplier;

        capacity1 = currentData.capacity1;
        capacity2 = currentData.capacity2;
        
    }

    public void ResetPlayerStats()
    {
        currentHealth = maxHealth;
        currentSpeed = baseSpeed;
        force = 0;
        
        if(photonView.IsMine) interfaceManager.InitializeHUD(currentHealth, maxHealth, Mathf.Lerp(controller.myTargetable.healthBar.speedFill.fillAmount, force, Time.deltaTime * 5f), pView.Owner.NickName);
        if(photonView.IsMine) interfaceManager.SetHUD(currentCharacter, currentTeam);
        controller.myTargetable.UpdateUI(true, true, currentHealth, maxHealth, false, 0, true, pView.Owner.NickName, true);
    }
    
    public void ChangePlayerCharacter(int index)
    {
        if(!controller.gameObject.activeSelf) controller.gameObject.SetActive(true);
        
        switch (index)
        {
            case 0:
                currentCharacter = Enums.Characters.Mimi;
                break;
            
            case 1:
                currentCharacter = Enums.Characters.Vega;
                break;
        }

        switch (currentCharacter)
        {
            case Enums.Characters.Mimi:
                vegaModel.SetActive(false);
                
                mimiModel.SetActive(true);
                currentAnimationController = mimiAnimationController;

                currentData = mimiData;
                break;
            
            case Enums.Characters.Vega:
                mimiModel.SetActive(false);
                
                vegaModel.SetActive(true);
                currentAnimationController = vegaAnimationController;

                currentData = vegaData;
                break;
        }
        
        currentAnimationController.SetTeamMaterial();
        
        SetData();
        
        currentHealth = maxHealth;

        ResetPlayerStats();
        
        if (!controller.myTargetable.healthBar.transform.gameObject.activeSelf)
        {
            controller.myTargetable.healthBar.transform.gameObject.SetActive(true);
            controller.myTargetable.UpdateUI(true, true, currentHealth, maxHealth, false, 0, true, pView.Owner.NickName, true);
        }

        isLoaded = true;

        GameManager gm = FindObjectOfType<GameManager>();

        if (gm)
        {
            gm.ReadyCheck(false, photonView.ViewID);
        }
        
        interfaceManager.SetHUD(currentCharacter, currentTeam);
    }
    
    public void ChangePlayerTeam(int index)
    {
        if (!controller.gameObject.activeSelf)
        {
            controller.gameObject.SetActive(true);
        }

        switch (index)
        {
            case 0:
                currentTeam = Enums.Team.Green;
                break;
            
            case 1 :
                currentTeam = Enums.Team.Pink;
                break;
        }

        controller.myTargetable.ownerTeam = currentTeam;

        currentAnimationController.SetTeamMaterial();
        
        controller.myTargetable.UpdateUI(true, true, currentHealth, maxHealth, false, 0, true, pView.Owner.NickName, true);

      
        GameManager gm = FindObjectOfType<GameManager>();
        
         if (gm)
         {
             gm.ReadyCheck(false, photonView.ViewID);
         }
         
         interfaceManager.SetHUD(currentCharacter, currentTeam);
    }

    #region CAPACITIES

    public float capacity1Timer;
    private float capacity1NetworkTime;
    public void SetCapacity1OnCooldown()
    {
        capacity1InCooldown = true;        
        capacity1NetworkTime = (float)PhotonNetwork.Time;
        capacity1Timer = 0;
        OnCooldownCapacity1 += CooldownCapacity1;
        interfaceManager.SetCapacityImageOnCooldown();
    }

    void CooldownCapacity1()
    {
        interfaceManager.UpdateCapacity1Image(capacity1Timer, capacity1.cooldownTime);
        
        if (capacity1Timer >= capacity1.cooldownTime)
        {
            interfaceManager.CapacityPumpScale();
            capacity1InCooldown = false;
            interfaceManager.UnsetCapacityImageOnCooldown();
            OnCooldownCapacity1 -= CooldownCapacity1;
        }
        else
        {
            capacity1Timer = (float) (PhotonNetwork.Time - capacity1NetworkTime);
        }
    }
    
    private float capacity2Timer;
    private float capacity2NetworkTime;
    public void SetCapacity2OnCooldown()
    {
        capacity2InCooldown = true;        
        capacity2Timer = 0;
        capacity2NetworkTime = (float)PhotonNetwork.Time;
        OnCooldownCapacity1 += CooldownCapacity2;
    }
    
    void CooldownCapacity2()
    {
        if (capacity2Timer >= capacity2.cooldownTime)
        {
            capacity2InCooldown = false;
            OnCooldownCapacity2 -= CooldownCapacity1;
        }
        else
        {
            capacity2Timer = (float) (PhotonNetwork.Time - capacity2NetworkTime);
        }
    }

    #endregion
    
    
    #endregion

    #region NETWORK METHODS
    
    public void DealDamage(int[] targetsID, int damageAmount)
    {
        if(!pView.IsMine) return;

        ConvoyBehavior convoy = null;
        
        foreach (var id in targetsID)
        {
            convoy = PhotonView.Find(id).GetComponent<ConvoyBehavior>();
            if(convoy != null) break;
        }
        
        Hashtable data = new Hashtable
        {
            {"TargetsID", targetsID},
            {"Amount", damageAmount},
            {"SenderTeam", (byte)currentTeam}
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCacheGlobal};

        PhotonNetwork.RaiseEvent(RaiseEvent.DamageTarget, data, raiseEventOptions, SendOptions.SendReliable);
    }

    public void HitStop(int[] targetsID,float time,float shakeForce)
    {
        if(!pView.IsMine) return;
        
        ConvoyBehavior convoy = null;
        
        foreach (var id in targetsID)
        {
            convoy = PhotonView.Find(id).GetComponent<ConvoyBehavior>();
            if(convoy != null) break;
        }

        
        if (convoy != null) { convoy.InitializeHitStop(time, shakeForce); }
        
        Hashtable data = new Hashtable
        {
            {"TargetsID", targetsID},
            {"Time", time},
            {"Force", shakeForce}
        };
        
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCacheGlobal};

        PhotonNetwork.RaiseEvent(RaiseEvent.HitStopTarget, data, raiseEventOptions, SendOptions.SendReliable);
    }
    
    public void KnockBack(int[] targetsID,float time,float force,Vector3 direction)
    {
        if(!pView.IsMine) return;
        
        Hashtable data = new Hashtable
        {
            {"TargetsID", targetsID},
            {"Time", time},
            {"Force", force},
            {"Direction", direction}
        };
        
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCacheGlobal};

        PhotonNetwork.RaiseEvent(RaiseEvent.KnockBackTarget, data, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SendPlayerCharacter(int characterID)
    {
       // if(!photonView.IsMine) return;
        
        Hashtable data = new Hashtable()
        {
            { "ID", pView.ViewID },
            { "CharacterID", characterID }
        };
        
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCacheGlobal};

        PhotonNetwork.RaiseEvent(RaiseEvent.SetCharacter, data, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SendPlayerTeam(int team)
    {
        //if(!photonView.IsMine) return;
        
        Hashtable data = new Hashtable()
        {
            { "ID", pView.ViewID },
            { "TeamID", team }
        };
        
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCacheGlobal};

        PhotonNetwork.RaiseEvent(RaiseEvent.SetTeam, data, raiseEventOptions, SendOptions.SendReliable);
    }
    
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == RaiseEvent.SetCharacter)
        {
            Hashtable data = (Hashtable)photonEvent.CustomData;

            int characterID = (int)data["CharacterID"];
            int playerID = (int)data["ID"];

            if(pView.ViewID != playerID) return;
                
            ChangePlayerCharacter(characterID);
        }
            
        if (photonEvent.Code == RaiseEvent.SetTeam)
        {
            Hashtable data = (Hashtable)photonEvent.CustomData;

            int teamID = (int)data["TeamID"];
            int playerID = (int)data["ID"];

            if(pView.ViewID != playerID) return;
                
            ChangePlayerTeam(teamID);
        }

      
            
        if (photonEvent.Code == RaiseEvent.DamageTarget)
        { 
            Hashtable data = (Hashtable)photonEvent.CustomData;
            
            int[] targets = (int[])data["TargetsID"];
            if(targets == null) return;
                    
            foreach (int id in targets)
            {
                if (pView.ViewID == id)
                {
                    TakeDamage(data);
                }
            }
        }

        if (photonEvent.Code == RaiseEvent.KnockBackTarget)
        {
            Hashtable data = (Hashtable)photonEvent.CustomData;

            int[] targets = (int[])data["TargetsID"];
            if(targets == null) return;
                    
            foreach (int id in targets)
            { 
                if (pView.ViewID == id) 
                { 
                    InitializeKnockBack(data);
                }
            }
        }
                
        if (photonEvent.Code == RaiseEvent.HitStopTarget)
        {
            Hashtable data = (Hashtable)photonEvent.CustomData;

            int[] targets = (int[])data["TargetsID"];
            if(targets == null) return;
                    
            foreach (int id in targets)
            {
                if (pView.ViewID == id) { InitializeHitStop(data); }
            }
        }

        if (photonEvent.Code == RaiseEvent.Death)
        {
            Debug.Log("Death event");
            
            Hashtable data = (Hashtable)photonEvent.CustomData;

            int targets = (int)data["ID"];
            if(targets == null) return;
                    
            if (pView.ViewID == targets)
            { 
                Death();
            }
        }
            
        
    }
      
      public void InitializeHitStop(Hashtable data)
      {
          float time = (float) data["Time"];
          float shakeForce = (float) data["Force"];
          controller.HitStop(time);
        
          if (shakeForce > 0)
          {
              currentAnimationController.Shake(shakeForce,time);
          }
      }
    
      public void InitializeKnockBack(Hashtable data)
      {
          float time = (float) data["Time"];
          float force = (float) data["Force"];
          Vector3 direction = (Vector3) data["Direction"];
          controller.InitializeKnockBack(time,force,direction);
      }
    
      public void TakeDamage(Hashtable data)
      {
          int amount = (int)data["Amount"];
        
          if (currentShield > 0)
          {
              int holdingDamage = amount - currentShield;

              currentShield -= amount;

              if (holdingDamage > 0)
              {
                  currentHealth -= amount;
              }
          }
          else
          {
              currentHealth -= amount;
          }
        
          if(currentHealth <= 0)
          {
              if(!photonView.IsMine) return;
              
              Debug.Log("Must Death");
              RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, };
              PhotonNetwork.RaiseEvent(RaiseEvent.Death, new Hashtable{{"ID", pView.ViewID}}, raiseEventOptions, SendOptions.SendReliable);
          }
          interfaceManager.HealthShakeOnDamage();
          interfaceManager.UpdateHealthBar(currentHealth, maxHealth);
          controller.myTargetable.UpdateUI(false, true, currentHealth, maxHealth);
      }

      void Death()
      {
          Debug.Log("Death");
          Respawn();
      }
      
      public void Respawn()
      {
          ResetPlayerStats();
          
          GameManager gm = FindObjectOfType<GameManager>();
          
          if (gm)
          {
              controller.agent.Warp(gm.localSpawnPoint.position);
          }
      }
      
      #endregion

    public override void OnPlayerEnteredRoom(Player newPlayer)
      {
          base.OnPlayerEnteredRoom(newPlayer);
          
          interfaceManager.UpdatePlayerInRoomCount();
      }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        
        interfaceManager.UpdatePlayerInRoomCount();
    }
}
