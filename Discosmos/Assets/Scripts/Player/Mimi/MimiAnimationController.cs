using Photon.Pun;
using Tools;
using UnityEngine;

public class MimiAnimationController : AnimationController
{
    [Header("ATTACK VFX")] 
    public ParticleSystem autoAttackFX;
    public ParticleSystem autoAttackImpactFX;
    public LineRenderer autoAttackTrail;
    private float trailTimer;
    public float trailDelay;
    public Transform firstPos;
    public Transform target;
    public bool attackFX;

    [Header("LASER VFX")] 
    public ParticleSystem laserGreenVFX;
    public ParticleSystem laserPinkVFX;
    public CapacityHitBox laserHitBox;
    
    public override void AttackVFX()
    {
        base.AttackVFX();
        
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

    public void CallMimiAttackFX()
    {
        autoAttackFX.Play();
        autoAttackTrail.gameObject.SetActive(true);
        autoAttackTrail.SetPosition(0,firstPos.position);
        trailTimer = trailDelay;
        target = PhotonView.Find(manager.controller.mimiAnimator.GetInteger("Target")).transform;
        autoAttackTrail.SetPosition(1,target.position);
        attackFX = true;
        autoAttackImpactFX.Play();
        autoAttackImpactFX.transform.position = target.position+Vector3.up*0.5f;
    }
    
    public void CastMimiLaser()
    {
        int damages = Mathf.RoundToInt(manager.capacity1.amount * manager.damageMultiplier.Evaluate(manager.force));
        manager.DealDamage(laserHitBox.idOnIt.ToArray(), damages);
        manager.force *= 0.2f;
        manager.HitStop(laserHitBox.idOnIt.ToArray(), manager.force > 0 ? 0.7f * manager.force + 0.2f: 0.2f,manager.force > 0 ? 0.3f * manager.force + 0.1f: 0.1f);
        Vector3 kbDirection = transform.forward;
        manager.KnockBack(laserHitBox.idOnIt.ToArray(), manager.force > 0 ? 0.6f * manager.force : 0,manager.force > 0 ? 11f * manager.force : 0,kbDirection.normalized);
        manager.controller.EnableMovement(true);
    }

    public void CallMimiLaserVFX()
    {
        switch (manager.currentTeam)
        {
            case Enums.Team.Green:
                laserGreenVFX.transform.position = new Vector3(manager.controller.transform.position.x,
                    laserGreenVFX.transform.position.y, manager.controller.transform.position.z);
        
                laserGreenVFX.transform.rotation = Quaternion.Euler(0,Quaternion.LookRotation(manager.controller.MouseWorldPosition() - manager.controller.transform.position).eulerAngles.y - 90,0);
                laserGreenVFX.Play();
                break;
            
            case Enums.Team.Pink:
                laserPinkVFX.transform.position = new Vector3(manager.controller.transform.position.x,
                    laserPinkVFX.transform.position.y, manager.controller.transform.position.z);
        
                laserPinkVFX.transform.rotation = Quaternion.Euler(0,Quaternion.LookRotation(manager.controller.MouseWorldPosition() - manager.controller.transform.position).eulerAngles.y - 90,0);
                laserPinkVFX.Play();
                break;
        }
       
    }

    public void CastMimiUltimate()
    {
        
    }
}
