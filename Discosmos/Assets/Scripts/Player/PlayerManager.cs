using Tools;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("REFERENCES")] 
    public PlayerController controller;
    public Camera _camera;
    
    //Visual
    public GameObject sparkles;

    [Header("STATE")] 
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

    private void Start()
    {
        ChangePlayerCharacter(Enums.Characters.Mimi);
        ChangePlayerTeam(Enums.Team.Green);
    }

    #endregion

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
