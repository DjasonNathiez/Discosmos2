using Photon.Pun;
using Tools;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [Header("REFERENCES")] 
    public MeshRenderer renderer;

    [Header("DATA")]
    public Enums.CollectableType type;
    public float amount;
    public bool randomisedApparition;
    public bool onCooldown;
    public float cooldownDuration;
    private float timer;
    private float backupNetworkTime;

    private void OnTriggerEnter(Collider other)
    {
        if(onCooldown) return;
        
        PlayerManager player = other.GetComponent<PlayerManager>();

        if (player)
        {
            switch (type)
            {
                case Enums.CollectableType.Convoy_Decrease:
                    ConvoyBehavior convoy = FindObjectOfType<ConvoyBehavior>();
                    
                    if(!convoy) return;
                    
                    switch (player.currentTeam)
                    {
                        case Enums.Team.Green:
                            convoy.pinkProgress -= Mathf.FloorToInt(amount);
                            break;
                        
                        case Enums.Team.Pink:
                            convoy.greenProgress -= Mathf.FloorToInt(amount);
                            break;
                    }
                    
                    break;
                
                case Enums.CollectableType.Player_ReduceCooldown:
                    player.capacity1Timer += amount;
                    break;
            }
        }

        renderer.enabled = false;
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

        renderer.enabled = true;
    }
    
    public void CooldownCollectable()
    {
        if (timer >= cooldownDuration)
        {
            onCooldown = false;
            GameAdministrator.NetworkUpdate -= CooldownCollectable;
        }
        else
        {
            timer = (float)(PhotonNetwork.Time - backupNetworkTime);
        }
    }
}
