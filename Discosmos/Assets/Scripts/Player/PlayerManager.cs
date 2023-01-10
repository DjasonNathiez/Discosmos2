using System;
using Photon.Pun;
using Tools;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("REFERENCES")] 
    public PlayerController controller;
    public Camera _camera;
    public GameObject cameraObject;
    public PhotonView photonView;
    
    //Visual
    public GameObject sparkles;

    [Header("STATE")] 
    public static bool isLoaded;
    public bool isCasting;
    public Enums.Characters currentCharacter;

    [Header("PLAYER")] 
    public string username;
    public Enums.Team currentTeam;

    [Header("CAPACITIES")] 
    public ActiveCapacity capacity1;
    public GameObject capacity1Visu;
    public ActiveCapacity capacity2;
    public GameObject capacity2Visu;

    [Header("DATA")] 
    
    #region MOVEMENT DATA
    
    public float currentSpeed;
    public float baseSpeed;
    
    public float force;

    public AnimationCurve speedCurve;
    public AnimationCurve slowDownCurve;

    #endregion
    
    [Header("VISUAL")]
    public GameObject mimiModel;
    public GameObject vegaModel;
    
    public AnimationController currentAnimationController;
    public AnimationController mimiAnimationController;
    public AnimationController vegaAnimationController;
    

    #region UNITY METHODS

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        if(photonView == null) photonView = PhotonView.Get(gameObject);
    }

    private void Start()
    {
        name = "Client" + "_" + photonView.Owner.NickName + "_" + photonView.ViewID + "_IsMine : " + photonView.IsMine;
    }

    private void Update()
    {
        if (GameAdministrator.gameState == Enums.GameState.Game && !controller.gameObject.activeSelf)
        {
            ActivePlayerVisual();
        }
    }

    #endregion

    public void Initialize()
    {
        if (photonView == null)
        {
            photonView = GetComponent<PhotonView>();
        }
        
        if (photonView.IsMine)
        {
            _camera.enabled = true;
            controller.enabled = true;
        }
    }

    public void ActivePlayerVisual()
    {
        if (photonView.IsMine)
        {
            cameraObject.SetActive(true);
        }
        
        controller.gameObject.SetActive(true);

        ChangePlayerCharacter(Enums.Characters.Vega);
        ChangePlayerTeam(Enums.Team.Green);

        isLoaded = true;
    }
    
    public void ChangePlayerCharacter(Enums.Characters character)
    {
        if(currentCharacter == character) return;

        currentCharacter = character;

        switch (currentCharacter)
        {
            case Enums.Characters.Mimi:
                vegaModel.SetActive(false);
                
                mimiModel.SetActive(true);
                currentAnimationController = mimiAnimationController;
                break;
            
            case Enums.Characters.Vega:
                mimiModel.SetActive(false);
                
                vegaModel.SetActive(true);
                currentAnimationController = vegaAnimationController;
                break;
        }
    }
    public void ChangePlayerTeam(Enums.Team team)
    {
        if(currentTeam == team) return;
        
        currentTeam = team;
        
        currentAnimationController.SetTeamMaterial();
    }
}
