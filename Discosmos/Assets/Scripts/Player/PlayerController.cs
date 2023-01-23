using Photon.Pun;
using Tools;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [Header("INPUTS")] 
    public KeyCode capacity1Input = KeyCode.A;
    public KeyCode capacity2Input = KeyCode.Z;

    [Header("REFERENCES")] 
    public NavMeshAgent agent;
    public PlayerManager manager;
    public Rigidbody rb;
    public Targetable myTargetable;
    
    [Header("STATE")] 
    public Enums.MovementType movementType;
    private bool isMoving;
    private bool isAttacking;

    [Header("MOVEMENT")] public Vector3 direction;
    private bool movementEnabled;
    private float hitStopTime;
    private float hitStopTimer;
    private float networkTimeBackup;

    [Header("ATTACK")] 
    public Targetable target;

    public Targetable cursorTarget;
    private Vector3 knockbackDirection;
    public float knockBackDuration;
    public float knockBackTime;
    public bool knockBackImmediatly;
    public float knockBackForce;
    
    [Header("RAIL")] 
    [HideInInspector] public int rampIndex;
    private Rail rail;
    [HideInInspector] public float rampProgress;
    public bool forwardOnRamp;
    private bool onRamp;

    [Header("ANIMATION")] 
    public Animator mimiAnimator;
    public Animator vegaAnimator;

    private float cooldownTimer;
    private float cooldownNetBackup;

    #region UNITY METHODS

    private void Start()
    {
        agent.speed = manager.currentSpeed;
        movementType = Enums.MovementType.MoveToClick;
    }

    private void Update()
    {
        //AttackInputCheck();
        MovementTypeCheck();
        CapacityInputCheck();
        CursorCheck();
        if (knockBackTime > 0)
        {
            ApplyKnockBack();
        }
        mimiAnimator.SetFloat("Force", Mathf.Lerp(myTargetable.healthBar.speedFill.fillAmount, manager.force, Time.deltaTime * 5f));
        vegaAnimator.SetFloat("Force",Mathf.Lerp(myTargetable.healthBar.speedFill.fillAmount, manager.force, Time.deltaTime * 5f));
    }

    private void FixedUpdate()
    {
        if (rb.velocity != Vector3.zero)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.deltaTime * 5f);
        }
        if (rb.velocity.magnitude < 0.5f)
        {
            rb.velocity = Vector3.zero;
        }
        if (rb.angularVelocity != Vector3.zero)
        {
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.deltaTime * 5f);
        }
        if (rb.angularVelocity.magnitude < 0.5f)
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    #endregion

    #region STATE MACHINE

    void MovementTypeCheck()
    {
        if(!movementEnabled) return;
        
        switch (movementType)
        {
            case Enums.MovementType.MoveToClick:
                MoveToClick();
                break;
            
            case Enums.MovementType.KeepDirection:
                KeepDirection();
                break;
            
            case Enums.MovementType.FollowTarget:
                FollowTarget();
                break;
            
            case Enums.MovementType.Slide:
                Slide();
                break;
            
            case Enums.MovementType.Attack:
                Attack();
                break;
            
            case Enums.MovementType.Tornado:
                Tornado();
                break;
        }
    }

    void CursorCheck()
    {
        Targetable cursorOn = CheckForMouseDetection();
        if (cursorOn && cursorOn != target)
        {
            if (cursorOn != cursorTarget)
            {
                if(cursorTarget) cursorTarget.circleCursor.SetActive(false);   
                cursorTarget = cursorOn;
                cursorTarget.circleCursor.SetActive(true);   
            }
        }
        else if(cursorTarget)
        {
            cursorTarget.circleCursor.SetActive(false);   
            cursorTarget = null;
        }
    }

    void SwitchMovementType(Enums.MovementType movementToSwitch)
    {
        movementType = movementToSwitch;
    }

    #endregion
    
    #region MOVEMENT

    private void MoveToClick()
    {
        manager.force -= manager.slowDownCurve.Evaluate(manager.force) * Time.deltaTime;
        manager.force = Mathf.Clamp01(manager.force);
        agent.enabled = true;
        if (isMoving && agent.remainingDistance == 0)
        {
            isMoving = false;
            agent.ResetPath();
            ChangeAnimation(0);
        }
        
        
        if (Input.GetMouseButton(1))
        {
            if (target)
            {
                target.HideTarget();
            }

            target = GetTarget();

            if (target)
            {
                target.ShowTarget();

                if (onRamp)
                {
                    OnExitRail();
                }

                movementType = Enums.MovementType.FollowTarget;
                ChangeAnimation(manager.force <= 0 ? 1 : 2);
            }
            else
            {
                agent.ResetPath();
                agent.SetDestination(MouseWorldPosition());
                isMoving = true;
                ChangeAnimation(manager.force <= 0 ? 1 : 2);
            }
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            manager.pointerVFX.transform.position = new Vector3(agent.destination.x, manager.pointerVFX.transform.position.y, agent.destination.z);
            manager.pointerVFX.Play();
        }
    }

    private void KeepDirection()
    {
        transform.position += direction * (manager.currentSpeed * Time.deltaTime);
        
        manager.force -= manager.slowDownCurve.Evaluate(manager.force) * Time.deltaTime;
        manager.force = Mathf.Clamp01(manager.force);
        
        if (manager.force <= 0)
        {
            agent.ResetPath();
            SwitchMovementType(Enums.MovementType.MoveToClick);
            ChangeAnimation(0);
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            if (target)
            {
                target.HideTarget();
            }

            target = GetTarget();

            if (target)
            {
                target.ShowTarget();

                if (onRamp)
                {
                    OnExitRail();
                }

                movementType = Enums.MovementType.FollowTarget;
                ChangeAnimation(manager.force <= 0 ? 1 : 2);
            }
            else
            {
                agent.ResetPath();
                agent.SetDestination(MouseWorldPosition());
                SwitchMovementType(Enums.MovementType.MoveToClick);
            }
            
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            manager.pointerVFX.transform.position = new Vector3(agent.destination.x, manager.pointerVFX.transform.position.y, agent.destination.z);
            manager.pointerVFX.Play();
        }
    }
    
    private void Tornado()
    {
        transform.position += direction * ((manager.currentSpeed + manager.vegaData.capacity1.speedBase) * Time.deltaTime);
        Vector3 mouse = MouseWorldPosition();
        direction = Vector3.Lerp(direction,(new Vector3(mouse.x,transform.position.y,mouse.z) - transform.position).normalized,Time.deltaTime*3);
    }

    private void FollowTarget()
    {
        agent.ResetPath();
        
        agent.SetDestination(target.targetableBody.position);

        manager.force -= manager.slowDownCurve.Evaluate(manager.force) * Time.deltaTime;
        manager.force = Mathf.Clamp01(manager.force);
        agent.enabled = true;
        if (isMoving && agent.remainingDistance == 0)
        {
            isMoving = false;
            ChangeAnimation(0);
        }

        
        if (Vector3.SqrMagnitude(target.targetableBody.position - transform.position) <= manager.attackRange * manager.attackRange)
        {
            agent.ResetPath();
            movementType = Enums.MovementType.Attack;
        }


        if (Input.GetMouseButton(1))
        {
            
            if (target)
            {
                target.HideTarget();
            }

            target = GetTarget();

            if (target)
            {
                target.ShowTarget();

                if (onRamp)
                {
                    OnExitRail();
                }

                movementType = Enums.MovementType.FollowTarget;
                ChangeAnimation(manager.force <= 0 ? 1 : 2);
            }
            else
            {
              agent.ResetPath();
              ResetTarget();
             agent.SetDestination(MouseWorldPosition());

             movementType = Enums.MovementType.MoveToClick;
             isMoving = true;
             ChangeAnimation(manager.force <= 0 ? 1 : 2);
                
            }
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            manager.pointerVFX.transform.position = new Vector3(agent.destination.x, manager.pointerVFX.transform.position.y, agent.destination.z);
            manager.pointerVFX.Play();
        }
    }
    
    private void Slide()
    {
        if (rampProgress < 1)
        {
            rampProgress += (Time.deltaTime * manager.currentSpeed) / rail.distBetweenNodes;
        }
        else
        {
            if (forwardOnRamp)
            {
                rampIndex++;
                if (rampIndex == rail.distancedNodes.Count - 1)
                {
                    direction = rail.exitDirectionLastNode.normalized;
                    OnExitRail();
                    return;
                }
            }
            else
            {
                rampIndex--;
                if (rampIndex == 0)
                {
                    direction = rail.exitDirectionFirstNode.normalized;
                    OnExitRail();
                    return;
                }
            }
            rampProgress -= 1;
        }

        if (onRamp)
        {
            transform.position = Vector3.Lerp(rail.distancedNodes[rampIndex], rail.distancedNodes[forwardOnRamp ? rampIndex + 1 : rampIndex -1], rampProgress) + Vector3.up * rail.heightOnRamp;
            if(!manager.isCasting) transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(rail.distancedNodes[forwardOnRamp ? rampIndex + 1 : rampIndex -1] - rail.distancedNodes[rampIndex]),Time.deltaTime*8);
            manager.force += rail.speedBoost.Evaluate(manager.force) * Time.deltaTime;
        }
    }

    public void EnableMovement()
    {
        movementEnabled = true;
    }
    
    public void DisableMovement()
    {
        movementEnabled = false;
        agent.ResetPath();
    }
    

    #endregion

    #region ATTACK

    void AttackInputCheck()
    {
        /*
        if (Input.GetMouseButtonDown(1))
        {
            /*if (target)
            {
                target.HideTarget();
            }

            target = GetTarget();

            if (target)
            {
                target.ShowTarget();

                if (onRamp)
                {
                    OnExitRail();
                }

                movementType = Enums.MovementType.FollowTarget;
                ChangeAnimation(manager.force <= 0 ? 1 : 2);
            }
            /*
            else
            {
                movementType = Enums.MovementType.MoveToClick;
                
                if (onRamp)
                {
                    OnExitRail();
                }
                
                ChangeAnimation(0);
            } 
            */
        
        
    }
    
    public void Attack()
    {
        if (!isAttacking)
        {
            if (Vector3.SqrMagnitude(target.targetableBody.transform.position - transform.position) > manager.attackRange * manager.attackRange)
            {
                isAttacking = false;
                movementType = Enums.MovementType.FollowTarget;
                ChangeAnimation(manager.force <= 0 ? 1 : 2);
            }
            else
            {
                ChangeAnimation(4);
                isAttacking = true;
                if(manager.force > 0) InitializeKnockBack(0.1f,manager.speedCurve.Evaluate(manager.force),target.targetableBody.position - transform.position,true);
            }
        }
        else
        {
            manager.force -= manager.slowDownCurve.Evaluate(manager.force) * Time.deltaTime;

            
            if(!manager.isCasting) transform.rotation = Quaternion.LookRotation(target.targetableBody.position - transform.position);

            switch (manager.currentCharacter)
            {
                case Enums.Characters.Mimi:
                    if (mimiAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                    {
                        isAttacking = false;
                    }
                    break;
                
                case Enums.Characters.Vega:
                    if (vegaAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                    {
                        isAttacking = false;
                    }
                    break;
            }

            if (Input.GetMouseButton(1))
            {
                ResetTarget();
                isAttacking = false;
                movementType = Enums.MovementType.MoveToClick;
                agent.ResetPath();
                agent.SetDestination(MouseWorldPosition());
                isMoving = true;
                ChangeAnimation(manager.force <= 0 ? 1 : 2);
            }
        }
    }

    public void OnAttack()
    {
        int damages = Mathf.RoundToInt(manager.attackDamage * manager.damageMultiplier.Evaluate(manager.force));

        if (enabled)
        {
                manager.DealDamage(new []{target.photonID}, damages);
                manager.HitStop(new []{target.photonID}, manager.force > 0 ? 0.7f * manager.force : 0,manager.force > 0 ? 0.3f * manager.force : 0);
            
                Vector3 kbDirection = target.targetableBody.position - transform.position;
                manager.KnockBack(new []{target.photonID}, manager.force > 0 ? 0.45f * manager.force : 0,manager.force > 0 ? 9f * manager.force : 0,kbDirection.normalized);
            
                manager.force = 0;
        }
    }
    private void ApplyKnockBack()
    {
        agent.nextPosition += knockbackDirection * (knockBackForce * Time.deltaTime * (knockBackTime / knockBackDuration));
        knockBackTime -= Time.deltaTime;
    }
    
    public void InitializeKnockBack(float kbTime,float kbForce, Vector3 kbDirection,bool applyImmediatly = false)
    {
        knockbackDirection = kbDirection;
        knockBackDuration = kbTime;
        knockBackTime = kbTime;
        knockBackForce = kbForce;
        knockBackImmediatly = applyImmediatly;
    }
    
    #endregion
    
    #region RAIL

    public void OnEnterRail(Rail railLD, bool forward, int startIndex)
    {
        //ResetTarget();
        onRamp = true;
        forwardOnRamp = forward;
        rampIndex = startIndex;
        rampProgress = 0;
        rail = railLD;

        movementType = Enums.MovementType.Slide;
        agent.ResetPath();
        
        ChangeAnimation(3);
        
        if(manager.sparkles) manager.sparkles.SetActive(true);
    }

    
    private void OnExitRail()
    {
        ChangeAnimation(manager.force <= 0 ? 1 : 2);
        
        onRamp = false;
        movementType = Enums.MovementType.KeepDirection;
        rail.OnExitRamp();
        
        manager.sparkles.SetActive(false);
    }

    #endregion

    #region CAPACITY

    void CapacityInputCheck()
    {
        if (!manager.capacity1InCooldown)
        {
            if (Input.GetKeyDown(capacity1Input))
            {
                if(manager.capacity1Visu) manager.capacity1Visu.SetActive(true);
            }
            
            if (Input.GetKey(capacity1Input))
            {
                if (manager.capacity1Visu)
                {
                    manager.capacity1Visu.transform.rotation = Quaternion.Euler(0,Quaternion.LookRotation(MouseWorldPosition()- transform.position).eulerAngles.y,0);
                    manager.capacity1Visu.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                }
            }
            
            if (Input.GetKeyUp(capacity1Input))
            {
                transform.rotation = Quaternion.Euler(0,Quaternion.LookRotation(MouseWorldPosition()- transform.position).eulerAngles.y,0);

                if(manager.capacity1Visu) manager.capacity1Visu.SetActive(false);
            
                if(onRamp) OnExitRail();
                ChangeAnimation(manager.currentAnimationController.capacity1Index);
                if(manager.capacity1.stopMovement) DisableMovement();
                manager.SetCapacity1OnCooldown();
                
            }
        }

        if (!manager.capacity2InCooldown)
        {
            if (Input.GetKeyDown(capacity2Input))
            {
                if(manager.capacity2Visu) manager.capacity2Visu.SetActive(true);
            }

            if (Input.GetKey(capacity2Input))
            {
                if (manager.capacity2Visu)
                {
                    manager.capacity2Visu.transform.rotation = Quaternion.Euler(0,Quaternion.LookRotation(MouseWorldPosition()- transform.position).eulerAngles.y,0);
                    manager.capacity2Visu.transform.position =
                        new Vector3(transform.position.x, 0, transform.position.z);
                }
            }
            
            if (Input.GetKeyUp(capacity2Input))
            {
                transform.rotation = Quaternion.Euler(0,Quaternion.LookRotation(MouseWorldPosition()- transform.position).eulerAngles.y,0);
            
                if(manager.capacity2Visu) manager.capacity2Visu.SetActive(false);
            
                if(onRamp) OnExitRail();
                ChangeAnimation(manager.currentAnimationController.capacity2Index);
                if(manager.capacity1.stopMovement) DisableMovement();
                manager.SetCapacity2OnCooldown();
                
            }
        }
        
       
    }

    #endregion
    
    #region TOOLS

    public void HitStop(float time)
    {
        movementEnabled = false;
        hitStopTime = time;
        networkTimeBackup = (float)PhotonNetwork.Time;
        agent.isStopped = true;
        
        GameAdministrator.NetworkUpdate += HitStopTimer;
    }

    void HitStopTimer()
    {
        if (hitStopTimer >= hitStopTime)
        {
            movementEnabled = true;
            GameAdministrator.NetworkUpdate -= HitStopTimer;
        }
        else
        {
            hitStopTimer = (float)(PhotonNetwork.Time - networkTimeBackup);
        }
    }
    
    public Vector3 MouseWorldPosition()
    {
        Ray ray = manager._camera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        
        return Vector3.zero;
    }
    
    public void ChangeAnimation(int index)
    {
        switch (manager.currentCharacter)
        {
            case Enums.Characters.Mimi:
                mimiAnimator.SetInteger("Animation",index);

                break;
            
            case Enums.Characters.Vega:
                vegaAnimator.SetInteger("Animation",index);
                break;
        }
    }
    
    
     public Targetable GetTarget()
    {
        if (!manager._camera) return null;
        
        Ray ray = manager._camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Targetable targetable = hit.transform.GetComponent<Targetable>();

            if (!targetable || targetable == myTargetable) return null;

            if (manager.currentTeam == targetable.ownerTeam) return null;

            switch (manager.currentCharacter)
            {
                case Enums.Characters.Mimi:
                    mimiAnimator.SetInteger("Target",targetable.bodyPhotonID);
                    break;
                
                case Enums.Characters.Vega:
                    vegaAnimator.SetInteger("Target",targetable.bodyPhotonID);
                    break;
            }
            return targetable;
        }
        return null;
    }
     
     public Targetable CheckForMouseDetection()
     {
         if (!manager._camera) return null;
        
         Ray ray = manager._camera.ScreenPointToRay(Input.mousePosition);
         if (Physics.Raycast(ray, out RaycastHit hit))
         {
             Targetable targetable = hit.transform.GetComponent<Targetable>();

             if (!targetable || targetable == myTargetable) return null;

             if (manager.currentTeam == targetable.ownerTeam) return null;
             
             return targetable;
         }
         return null;
     }
     
     public void ResetTarget()
     {
         if (target)
         {
             target.HideTarget();
         }
        
         target = null;
     }

     #endregion
}
