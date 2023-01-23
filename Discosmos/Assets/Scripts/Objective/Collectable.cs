using System;
using Photon.Pun;
using Tools;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [Header("REFERENCES")] 
    public MeshRenderer mRenderer;

    [Header("DATA")]
    public Enums.CollectableType type;
    public float amount;
    public bool randomisedApparition;
    public bool onCooldown;
    public float cooldownDuration;
    private float timer;
    private float backupNetworkTime;

    private void Awake()
    {
        mRenderer = GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject);
        if(onCooldown) return;
        
        PlayerManager player = other.GetComponentInParent<PlayerManager>();

        Debug.Log(player);

        if (!player && !other.CompareTag("Player")) return;
        
        switch (type)
        {
            case Enums.CollectableType.Convoy_Decrease:
                ConvoyBehavior convoy = FindObjectOfType<ConvoyBehavior>();
                if(!convoy) return;
                    
                switch (player.currentTeam)
                {
                    case Enums.Team.Green:
                        convoy.pinkProgress -= Mathf.FloorToInt(amount);
                        Debug.Log("convoy pink progress down");
                        break;
                        
                    case Enums.Team.Pink:
                        convoy.greenProgress -= Mathf.FloorToInt(amount);
                        Debug.Log("convoy green progress down");
                        break;
                }
                    
                break;
                
            case Enums.CollectableType.Player_ReduceCooldown:
                player.capacity1Timer += amount;
                Debug.Log("cdr reduced");
                break;
        }
            
        mRenderer.enabled = false;
        onCooldown = true;
        backupNetworkTime = (float) PhotonNetwork.Time;
        GameAdministrator.NetworkUpdate += CooldownCollectable;

    }

    public void Appear()
    {
        if (randomisedApparition)
        {
            int random = UnityEngine.Random.Range(0, 1);

            switch (random)
            {
                case 0:
                    type = Enums.CollectableType.Convoy_Decrease;
                    break;
            
                case 1:
                    type = Enums.CollectableType.Player_ReduceCooldown;
                    break;
            }
        }

        mRenderer.enabled = true;
    }
    
    public void CooldownCollectable()
    {
        if (timer >= cooldownDuration)
        {
            onCooldown = false;
            Appear();
            GameAdministrator.NetworkUpdate -= CooldownCollectable;
        }
        else
        {
            timer = (float)(PhotonNetwork.Time - backupNetworkTime);
        }
    }
}
