using UnityEngine;

public abstract class ActiveCapacitySO : ScriptableObject
{
    public ActiveCapacity activeCapacity;

    public int amount;

    public byte index;
    public float castTime;
    public float cooldownTime;

    public float durationBase;
    public float speedBase;

    public abstract void GetActiveCapacity();
}
