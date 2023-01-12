using System.Collections.Generic;
using Photon.Pun;
using Tools;
using UnityEngine;

public class GameAdministrator : MonoBehaviourPunCallbacks
{
    public static GameAdministrator instance;

    [Header("STATE")]
    public static Enums.GameState gameState = Enums.GameState.Disconnected;
    public static Enums.NetworkRoomState roomState = Enums.NetworkRoomState.Outside;
    
    [Header("NETWORK UPDATE")]  
    public static PlayerManager localPlayer;
    public static NetworkDelegate.OnServerUpdate NetworkUpdate;
    public static NetworkDelegate.OnUpdated OnUpdated;
    
    public double tickRate = 0.2f;
    private double timer;
    private double lastTickTime;

    [Header("GAME DATA")] 
    public static int connectToRoomNeedPlayer = 1;
    public int playerPerGame = 4;

    public static List<PlayerManager> players;
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        players = new List<PlayerManager>();
        
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
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
        Debug.Log("tick");
    }

    public static void UpdatePlayersList()
    {
        players.Clear();
        players.AddRange(FindObjectsOfType<PlayerManager>());
    }

   
}
