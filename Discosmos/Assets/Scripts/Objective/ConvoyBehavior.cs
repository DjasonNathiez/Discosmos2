using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Tools;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class ConvoyBehavior : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [Header("REFERENCES")] 
    public Targetable myTargetable;
    
    [Header("PROGRESSION")] 
    public int pinkProgress;
    public int greenProgress;
    public float timeToRelease;
    public float currentTimeToRelease;
    public float releaseTimer;
    public float backupNetworkTime;
    
    [Header("MOVEMENT")]
    public float force;
    public float decceleration;
    public bool master;

    public float xMax;
    public float xMin;

    public bool pointReached;
    public Enums.Team leftSide;
    public Enums.Team rightSide;

    [Header("HIT STOP AND SHAKING")]
    public bool shaking;
    public float shakingForce;
    public float shakingTime;
    public float shakingDuration;
    public Vector3 truePos;
    public float previousShake = 1;
    public float nextShakeTimer = 0.02f;
    public float shakeFrequency = 0.02f;
    public Transform renderBody;
    public GameObject smokeEffect;

    public List<Vector3> curve;
    public int index;
    public float factor;
    public int startIndex;
    public float startFactor;


    public void ActiveGameLoop()
    {
        pinkProgress = 0;
        greenProgress = 0;
        transform.position = curve[startIndex];
        
        /*backupNetworkTime = (float)PhotonNetwork.Time;
        GameAdministrator.NetworkUpdate += TimerLoop;*/
    }

    /*public void DisableGameLoop()
    {
        releaseTimer = 0;
        GameAdministrator.NetworkUpdate -= TimerLoop;
    }
    
    public void TimerLoop()
    {
        if (releaseTimer > currentTimeToRelease)
        {
            Release();
            backupNetworkTime = (float)PhotonNetwork.Time;
            releaseTimer = 0;
        }
        else
        {
            releaseTimer = (float) (PhotonNetwork.Time - backupNetworkTime);
        }
    }*/

    private void Awake()
    {
        currentTimeToRelease = timeToRelease;
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            master = true;
        }
    }

    private void Start()
    {
    }

    void Update()
    {
        if (master)
        {
            if (!pointReached)
            {
                if (shaking)
                {
                    ApplyShaking();
                }
                else
                {
                    ApplyMovement();
                }   
            }


            // TEST APPLY FORCES
            if (Input.GetKeyDown(KeyCode.D))
            {
               ActiveGameLoop();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                ApplyForce(-10);
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ApplyForce(-50);
                InitializeHitStop(0.2f, 0.15f);
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                ApplyForce(50);
                InitializeHitStop(0.2f, 0.15f);
            }
        }
    }

    void ApplyMovement()
    {
        factor += force * Time.deltaTime;
        if (factor > 1)
        {
            factor -= 1;
            index++;
            CheckForPoint();
        }
        if (factor < 0)
        {
            factor += 1;
            index--;
            CheckForPoint();
        }

        if (!pointReached)
        {
            transform.position = Vector3.Lerp(curve[index],curve[index+1],factor);
            force = Mathf.Lerp(force, 0, decceleration * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0,((transform.position.x%70)/70)*360,0);   
            smokeEffect.transform.rotation = Quaternion.LookRotation(((curve[index+1] -curve[index])*force).normalized);
        }
    }

    void CheckForPoint()
    {
        if (index == -1)
        {
            transform.position = curve[0];

            switch (leftSide)
            {
                case Enums.Team.Green:
                    GameManager.instance.EndGame(Enums.Team.Green);
                    break;
                
                case Enums.Team.Pink:
                    GameManager.instance.EndGame(Enums.Team.Pink);
                    break;
            }
            
            StartCoroutine(PointGained());
        }
        else if (index == curve.Count)
        {
            transform.position = curve[curve.Count -1];
            
            switch (rightSide)
            {
                case Enums.Team.Green:
                    GameManager.instance.EndGame(Enums.Team.Green);
                    break;
                
                case Enums.Team.Pink:
                    GameManager.instance.EndGame(Enums.Team.Pink);
                    break;
            }
            StartCoroutine(PointGained());
        }
    }

    public IEnumerator PointGained()
    {
        pointReached = true;
        force = 0;
        yield return new WaitForSeconds(1);
        transform.position = new Vector3(0, transform.position.y, transform.position.z);
        force = 0;
        pointReached = false;
        index = startIndex;
        factor = startFactor;
    }
    
    public void ApplyForce(float impulse)
    {
        if (master)
        {
            if (!pointReached)
            {
                force += impulse;   
            }   
        }
    }
    
    public void InitializeHitStop(float time,float force)
    {
        shakingForce = force;
        shakingTime = time;
        shakingDuration = time;
        shaking = true;
    }

    void ApplyShaking()
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
                    Vector2 shake = new Vector2(
                        Random.Range(0.2f, 1f) * shakingForce * (shakingTime / (shakingDuration / 0.7f) + 0.3f) *
                        previousShake,
                        Random.Range(0.2f, 1f) * shakingForce * (shakingTime / (shakingDuration / 0.7f) + 0.3f) *
                        previousShake);
                    renderBody.localPosition = new Vector3(truePos.x + shake.x, truePos.y, truePos.z + shake.y);
                    previousShake = -previousShake;
                }
            }
            else
            {
                renderBody.localPosition = truePos;
                shakingTime = 0;
                shaking = false;
            }
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == RaiseEvent.DamageTarget)
        {
            Hashtable data = (Hashtable)photonEvent.CustomData;
            
            int[] targets = (int[])data["TargetsID"];
            if(targets == null) return;
                    
            foreach (int id in targets)
            {
                if (photonView.ViewID == id)
                {
                    TakeDamage(data);
                }
            }
        }
    }

    public void TakeDamage(Hashtable data)
    {
        byte teamByte = (byte) data["SenderTeam"];
        Enums.Team senderTeam = (Enums.Team) teamByte;
        
        int amount = (int) data["Amount"];

        switch (senderTeam)
        {
            case Enums.Team.Green:
                ApplyForce(-amount);
                break;
            
            case Enums.Team.Pink:
                ApplyForce(amount);
                break;
        }
        
    }

    public void Release()
    {
        int releaseAmount = greenProgress - pinkProgress;
        
        ApplyForce(releaseAmount);

        greenProgress = 0;
        pinkProgress = 0;
        
        Debug.Log("Convoy release with an amount of " + releaseAmount);
    }
}
