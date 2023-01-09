using Photon.Pun;
using Tools;
using UnityEngine;

public class GameAdministrator : MonoBehaviour
{
    public static PlayerManager localPlayer;
    public static NetworkDelegate.OnServerUpdate NetworkUpdate;
    public static NetworkDelegate.OnUpdated OnUpdated;
    
    public double tickRate;
    private double timer;
    private double lastTickTime;
    
    private void Awake()
    {
        OnUpdated += UpdateNetwork;
    }

    private void Update()
    {
        OnUpdated?.Invoke();
    }

    public void UpdateNetwork()
    {
        if (timer >= 1.00 / tickRate)
        {
            Tick();
            lastTickTime = PhotonNetwork.Time;
        }
        else
        {
            timer = PhotonNetwork.Time - lastTickTime;
        }
    }
    
    void Tick()
    {
        NetworkUpdate?.Invoke();
    }
}
