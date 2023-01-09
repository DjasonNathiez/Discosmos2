using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class ActiveCapacity
{
    public ActiveCapacitySO data;
    public PlayerController owner;
    
    private byte caster;
    
    public bool onCooldown;
    public double serverTimeBackup;
    public float castTimer;
    public float cooldownTimer;
    
    public virtual void SetCapacityData(ActiveCapacitySO reference) { data = reference; } //Get reference of the capacity scriptable object


    public virtual void Cast()
    {
        owner.manager.isCasting = false;
        cooldownTimer = 0;
        
        if(onCooldown)
        {
            return;
        }
        onCooldown = true;

        serverTimeBackup = PhotonNetwork.Time;

        if (data.castTime != 0)
        {
            GameAdministrator.NetworkUpdate += CastRoutine;
        }
        else
        {
            Active();
        }
    }

    public virtual void Active()
    {
        GameAdministrator.NetworkUpdate += Cooldown;
    }
    
    public virtual void CastRoutine()
    {
        if (castTimer >= data.castTime)
        {
            Active();
            GameAdministrator.NetworkUpdate -= CastRoutine;
            castTimer = 0;
        }
        else
        {
            castTimer = (float)(PhotonNetwork.Time - serverTimeBackup);
        }
    }

    public virtual void Cooldown()
    {
        if (cooldownTimer >= data.cooldownTime)
        {
            Debug.Log("Capacity is no longer on cooldown");
            onCooldown = false;
            GameAdministrator.NetworkUpdate -= Cooldown;
        }
        else
        {
            cooldownTimer = (float)(PhotonNetwork.Time - serverTimeBackup);
        }
    }
}
