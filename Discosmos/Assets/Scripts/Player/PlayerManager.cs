using System.Collections;
using System.Collections.Generic;
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
    public Enums.CurrentCharacter currentCharacter;

    [Header("CAPACITIES")] 
    public ActiveCapacity capacity1;
    public GameObject capacity1Visu;
    public ActiveCapacity capacity2;
    public GameObject capacity2Visu;

    [Header("DATA")] 
    public AnimationController currentAnimationController;
    
    #region MOVEMENT DATA
    
    public float currentSpeed;
    public float baseSpeed;
    
    public float force;

    public AnimationCurve speedCurve;
    public AnimationCurve slowDownCurve;

    #endregion
}
