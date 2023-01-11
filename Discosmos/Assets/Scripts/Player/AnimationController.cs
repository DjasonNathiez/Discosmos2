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
    
    [Header("VFX")] 
    public ParticleSystem autoAttackFX;
    public ParticleSystem autoAttackImpactFX;
    public LineRenderer autoAttackTrail;
    private float trailTimer;
    public float trailDelay;
    public Transform firstPos;
    public Transform target;
    public bool attackFX;

    
    [Header("CAPACITIES")]
    public int capacity1Index;
    public int capacity2Index;

    private void Update()
    {
        if (shaking)
        {
            if (shakingTime > 0)
            {
                shakingTime -= Time.deltaTime;
                if (nextShakeTimer > 0)
                {
                    nextShakeTimer -= Time.deltaTime;
                }
                else
                {
                    nextShakeTimer = shakeFrequency;
                    Vector2 shake = new Vector2(Random.Range(0.2f, 1f) * shakingForce * (shakingTime /(shakingDuration / 0.7f) + 0.3f)* previousShake, Random.Range(0.2f, 1f) * shakingForce * (shakingTime /(shakingDuration / 0.7f) + 0.3f) * previousShake);
                    transform.localPosition = new Vector3(truePos.x + shake.x, truePos.y, truePos.z + shake.y);
                    previousShake = -previousShake;
                }
            }
            else
            {
                transform.localPosition = truePos;
                shakingTime = 0;
                shaking = false;
            }   
        }

        if (attackFX)
        {
            if (trailTimer > 0)
            {
                trailTimer -= Time.deltaTime;
                autoAttackTrail.startColor = Color.Lerp(new Color(1,1,1,0),Color.white, trailTimer/trailDelay);
                autoAttackTrail.endColor = Color.Lerp(new Color(1,1,1,0),Color.white, trailTimer/trailDelay);
            }
            else
            {
                autoAttackTrail.gameObject.SetActive(false);
                attackFX = false;
            }   
        }
    }

    #region ANIMATION EVENTS

    public void CallAttackEvent()
    {
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
