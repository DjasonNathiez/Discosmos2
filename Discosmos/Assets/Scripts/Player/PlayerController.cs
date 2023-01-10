using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
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
    
    [Header("STATE")] 
    public Enums.MovementType movementType;
    private bool isMoving;

    [Header("Movement")] 
    private Vector3 direction;
    
    [Header("RAIL")] 
    [HideInInspector] public int rampIndex;
    private Rail rail;
    [HideInInspector] public float rampProgress;
    public bool forwardOnRamp;
    private bool onRamp;

    [Header("ANIMATION")] 
    public Animator mimiAnimator;
    public Animator vegaAnimator;

    #region UNITY

    private void Start()
    {
        agent.speed = manager.currentSpeed;
        movementType = Enums.MovementType.MoveToClick;
    }

    private void Update()
    {
        MovementTypeCheck();
        CapacityInputCheck();
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
        switch (movementType)
        {
            case Enums.MovementType.MoveToClick:
                MoveToClick();
                break;
            
            case Enums.MovementType.KeepDirection:
                KeepDirection();
                break;
            
            case Enums.MovementType.FollowTarget:

                break;
            
            case Enums.MovementType.Slide:
                Slide();
                break;
            
            case Enums.MovementType.Attack:

                break;
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
            agent.ResetPath();
            agent.SetDestination(MouseWorldPosition());
            isMoving = true;
           ChangeAnimation(manager.force <= 0 ? 1 : 2);
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
        
        if (Input.GetMouseButton(1))
        {
            agent.ResetPath();
            agent.SetDestination(MouseWorldPosition());
            SwitchMovementType(Enums.MovementType.MoveToClick);
            
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
            rampProgress = 0;
        }

        if (onRamp)
        {
            transform.position = Vector3.Lerp(rail.distancedNodes[rampIndex], rail.distancedNodes[forwardOnRamp ? rampIndex + 1 : rampIndex -1], rampProgress) + Vector3.up * rail.heightOnRamp;
            if(!manager.isCasting) transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(rail.distancedNodes[forwardOnRamp ? rampIndex + 1 : rampIndex -1] - rail.distancedNodes[rampIndex]),Time.deltaTime*8);
            manager.force += rail.speedBoost.Evaluate(manager.force) * Time.deltaTime;
        }
    }


    #endregion
    
    #region RAIL

    public void OnEnterRail(Rail railLD, bool forward, int startIndex)
    {
        Debug.Log("Enter Rail");
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
        //PRESS
        if (Input.GetKeyDown(capacity1Input))
        {
            manager.capacity1Visu.SetActive(true);
        }

        if (Input.GetKeyDown(capacity2Input))
        {
            manager.capacity2Visu.SetActive(true);
        }
        
        //HOLD
        if (Input.GetKey(capacity1Input))
        {
            manager.capacity1Visu.transform.rotation = Quaternion.Euler(0,Quaternion.LookRotation(MouseWorldPosition()- transform.position).eulerAngles.y,0);
        }

        if (Input.GetKey(capacity2Input))
        {
            manager.capacity2Visu.transform.rotation = Quaternion.Euler(0,Quaternion.LookRotation(MouseWorldPosition()- transform.position).eulerAngles.y,0);
        }
        
        //RELEASE
        if (Input.GetKeyUp(capacity1Input))
        {
            transform.rotation = Quaternion.Euler(0,Quaternion.LookRotation(MouseWorldPosition()- transform.position).eulerAngles.y,0);

            if (!manager.capacity1.onCooldown)
            {
                manager.capacity1.Cast();
                manager.capacity1Visu.SetActive(false);
                ChangeAnimation(manager.currentAnimationController.capacity1Index);
            }
        }

        if (Input.GetKeyUp(capacity2Input))
        {
            transform.rotation = Quaternion.Euler(0,Quaternion.LookRotation(MouseWorldPosition()- transform.position).eulerAngles.y,0);

            if (!manager.capacity2.onCooldown)
            {
                manager.capacity2.Cast();
                manager.capacity2Visu.SetActive(false);
                ChangeAnimation(manager.currentAnimationController.capacity2Index);
            }
        }
    }

    void AskCastCapacity(ActiveCapacity capacity)
    {
    }

    #endregion
    
    #region TOOLS

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
    
    /*
     public Targetable GetTarget()
    {
        if (manager._camera == null) return null;
        
        Ray ray = manager._camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Targetable targetable = hit.transform.GetComponentInParent<Targetable>();
            ITeamable teamable = hit.transform.GetComponentInParent<ITeamable>();

            if (targetable == null || targetable == myTargetable) return null;

            if (teamable != null)
            {
                if (manager.CurrentTeam() == teamable.CurrentTeam()) return null;
            }
            if (manager.currentData == manager.mimiData)
            {
                animatorMimi.SetInteger("Target",targetable.bodyPhotonID);   
            }
            else
            {
                animatorVega.SetInteger("Target",targetable.bodyPhotonID);   
            }
            return targetable;
        }
        return null;
    }
*/
    
    
    #endregion
}
