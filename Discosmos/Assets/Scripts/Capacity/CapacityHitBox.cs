using System;
using System.Collections.Generic;
using UnityEngine;


public class CapacityHitBox : MonoBehaviour
{ 
    [SerializeField] private PlayerManager owner;
    public List<int> idOnIt;
    public List<GameObject> targets;

    private void OnEnable()
    {
        idOnIt.Clear();
        targets.Clear();
    }

    private void OnDisable()
    {
        idOnIt.Clear();
        targets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    { 
        Targetable targetable = other.GetComponent<Targetable>();
        
        if (targetable && !idOnIt.Contains(targetable.photonID) && targetable != owner.controller.myTargetable && targetable.ownerTeam != owner.currentTeam)
        {
            targets.Add(targetable.gameObject);
            idOnIt.Add(targetable.photonID);
        }
    }
    private void OnTriggerExit(Collider other) { 
        
        Targetable targetable = other.GetComponent<Targetable>();
        
        if (targetable && idOnIt.Contains(targetable.photonID))
        {
            targets.Remove(targetable.gameObject);
            idOnIt.Remove(targetable.photonID); 
            
        }
        
    }
}
