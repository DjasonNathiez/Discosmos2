using Tools;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [Header("REFERENCES")] 
    public PlayerManager manager;
    
    [Header("SHAKING")]
    public bool shaking;
    public float shakingForce;
    public float shakingTime;
    public float shakingDuration;
    public Vector3 truePos;
    public float previousShake = 1;
    public float nextShakeTimer = 0.02f;
    public float shakeFrequency = 0.02f;
    
    [Header("MODEL")] 
    public SkinnedMeshRenderer[] bodyParts;
    public Material pinkMaterial;
    public Material greenMaterial;
    public Material neutralMaterial;


    [Header("CAPACITIES")]
    public int capacity1Index;
    public int capacity2Index;

    private void Update()
    {
        AttackVFX();
        UpdateInDependencies();
    }

    #region ANIMATION EVENTS

    public virtual void AttackVFX() { }

    public virtual void UpdateInDependencies()
    {
        
    }
    
    public void CallAttackEvent()
    {
        Debug.Log("ATTACK EVENT");
        manager.controller.OnAttack();
    }

    #endregion
    
    public virtual void SetTeamMaterial()
    {
        for (int i = 0; i < bodyParts.Length; i++)
        {
            switch (manager.currentTeam)
            {
                case Enums.Team.Green:
                    if (greenMaterial) bodyParts[i].material = greenMaterial;
                    break;
                
                case Enums.Team.Pink:
                    if (pinkMaterial) bodyParts[i].material = pinkMaterial;
                    break;
                
                case Enums.Team.Neutral:
                    if (neutralMaterial) bodyParts[i].material = neutralMaterial;
                    break;
            }
        }
    }
    
    public void Shake(float force, float time)
    {
        shakingForce = force;
        shakingTime = time;
        shakingDuration = time;
        shaking = true;
    }
}
