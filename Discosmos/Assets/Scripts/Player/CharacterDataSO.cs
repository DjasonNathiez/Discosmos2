using UnityEngine;

[CreateAssetMenu(order = 0, menuName = "Character Data", fileName = "new character data")]
public class CharacterDataSO : ScriptableObject
{
    [Header("STATE")]
    public int maxHealth;

    [Header("MOVEMENT")]
    public float baseSpeed;
    public AnimationCurve speedCurve;
    public AnimationCurve slowDownCurve;
    
    [Header("COMBAT")]
    public int attackDamage;
    public float attackRange;
    public float attackSpeed;
    public AnimationCurve damageMultiplier;

    [Header("CAPACITIES")] 
    public ActiveCapacitySO capacity1;
    public ActiveCapacitySO capacity2;
}
