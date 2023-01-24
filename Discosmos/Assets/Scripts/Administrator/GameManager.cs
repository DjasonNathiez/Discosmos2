using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Tools;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
   public static GameManager instance;
   [Header("REFERENCES")] 
   public ConvoyBehavior convoy;
   
   [Header("SPAWN")]
   public Transform spawnPoint;
   public List<PlayerInRoom> playersInRoom;
   
   public Transform[] spawnPink;
   public Transform[] spawnGreen;

   [Header("PLAYERS")]
   public List<PlayerInRoom> pinkPlayers;
   public List<PlayerInRoom> greenPlayers;
   public Transform localSpawnPoint;
   
   private bool localIsReady = false;
   
   public bool gameIsFull;
   public float timeBeforeStart = 60;
   public float startTimer;
   public bool gameIsStarted;
   
   [Header("UI")]
   public InterfaceManager interfaceManager;
   public double timer;
   private double lastTickTime;
   public Enums.Team defaultWinner = Enums.Team.Neutral;
   public float gameTimer = 300;
   
   public TMP_FontAsset pinkFontWin;
   public TMP_FontAsset greenFontWin;
   public TextMeshProUGUI winText;
   public GameObject winObj;
   public GameObject readyObj;
   public TextMeshProUGUI readyCount;
   
   private void Awake()
   {
      if (instance == null)
      {
         instance = this;
      }
      else
      {
         Destroy(gameObject);
      }

      pinkPlayers = new List<PlayerInRoom>();
      greenPlayers = new List<PlayerInRoom>();
      playersInRoom = new List<PlayerInRoom>();
      
      GameAdministrator.gameState = Enums.GameState.Game;
   }

   private void Start()
   {
     // GameAdministrator.localPlayer.controller.transform.localPosition = spawnPoint.position;
     GameAdministrator.localPlayer.interfaceManager.championSelectCanvas.SetActive(true);
     GameAdministrator.localPlayer.controller.agent.Warp(spawnPoint.position);
     GameAdministrator.UpdatePlayersList();
     interfaceManager = GameAdministrator.localPlayer.interfaceManager;
   }

   public void ReadyButton()
   {
      localIsReady = !localIsReady;
      ReadyCheck(localIsReady, GameAdministrator.localPlayer.pView.ViewID);
   }

   public void ReadyCheck(bool isReady, int sender)
   {
      
      Hashtable data = new Hashtable()
      {
         {"Sender", sender},
         {"IsReady", isReady}
      };
      
      RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCacheGlobal};

      PhotonNetwork.RaiseEvent(RaiseEvent.ReadySelect, data, raiseEventOptions, SendOptions.SendReliable);
   }

   public void StartGame()
   {
      winObj.SetActive(false);
      convoy.pinkPoints = 0;
      convoy.greenPoints = 0;
      
      interfaceManager.UpdateScore(convoy.greenPoints, convoy.pinkPoints);

      foreach (var p in playersInRoom)
      {
         p.isReady = false;
      }
      localIsReady = false;
      
      switch (GameAdministrator.localPlayer.currentTeam)
      {
         case Enums.Team.Green:
            for (int i = 0; i < greenPlayers.Count; i++)
            {
               if (greenPlayers[i].pManager == GameAdministrator.localPlayer && i <= spawnGreen.Length)
               {
                  localSpawnPoint = spawnGreen[i];
               }
            }
            break;
         
         case Enums.Team.Pink:
            for (int i = 0; i < pinkPlayers.Count; i++)
            {
               if (pinkPlayers[i].pManager == GameAdministrator.localPlayer && i <= spawnPink.Length)
               {
                  localSpawnPoint = spawnPink[i];
               }
            }
            break;
      }
      
      GameAdministrator.localPlayer.interfaceManager.championSelectCanvas.SetActive(false);
      GameAdministrator.localPlayer.interfaceManager.scoreCanvas.SetActive(true);
      
      for (int i = 0; i < playersInRoom.Count; i++)
      {
         if (playersInRoom[i].pManager != GameAdministrator.localPlayer)
         {
            GameAdministrator.localPlayer.interfaceManager.CreateIndics(playersInRoom[i].pManager);
         }
      }
      
      GameAdministrator.localPlayer.Respawn();
      
      timer = 0;
      interfaceManager.UpdateGameTimer((float)timer);
      lastTickTime = PhotonNetwork.Time;
      
      GameAdministrator.NetworkUpdate += GameTimer;
      
      convoy.InitConvoy();
   }
   
   public void GameTimer()
   {
      
      interfaceManager.UpdateGameTimer((float)timer);
      
      
         if (timer >= gameTimer)
         {
            EndGame(defaultWinner);
            GameAdministrator.NetworkUpdate -= GameTimer;
         }
         else
         {
            timer = PhotonNetwork.Time - lastTickTime;
         }
      
   }

   public void EndGame(Enums.Team winner)
   {
      gameIsStarted = false;
      GameAdministrator.localPlayer.interfaceManager.scoreCanvas.SetActive(false);
      readyCount.text = "0/4";

      switch (winner)
      {
         case Enums.Team.Green:
            winObj.SetActive(true);
            winText.font = greenFontWin;
            winText.text = "Green Win !";
            break;
         
         case Enums.Team.Pink:
            winObj.SetActive(true);
            winText.font = greenFontWin;
            winText.text = "Pink Win !";
            break;
      }
      
   }

   public void CheckPlayersInRoom(Hashtable data = null)
   {
      if (data == null) return;

      bool ready = false;
      int id = 0;

      id = (int) data["Sender"];
      ready = (bool) data["IsReady"];
      
      PlayerInRoom pim = new PlayerInRoom()
      {
         isReady = ready,
         pManager = PhotonView.Find(id).GetComponent<PlayerManager>(),
         pTeam = PhotonView.Find(id).GetComponent<PlayerManager>().currentTeam
      };

      if (playersInRoom.Count == 0)
      {
         playersInRoom.Add(pim);
      }
      else
      {
         for (int i = 0; i < playersInRoom.Count; i++)
         {
            if (i >= playersInRoom.Count -1)
            {
               if (playersInRoom[i].pManager == pim.pManager)
               {
                  playersInRoom[i].isReady = pim.isReady;
                  playersInRoom[i].pTeam = pim.pTeam;
                  
               }
               else
               {
                  playersInRoom.Add(pim);
               }
            }
            else
            {
               if (playersInRoom[i].pManager == pim.pManager)
               {
                  playersInRoom[i].isReady = pim.isReady;
                  playersInRoom[i].pTeam = pim.pTeam;
                  break;
               }
            }
         }
      }
      
      pinkPlayers.Clear();
      greenPlayers.Clear();

      foreach (var player in playersInRoom)
      {
         switch (player.pTeam)
         {
            case Enums.Team.Green:
               greenPlayers.Add(player);
               break;
            
            case Enums.Team.Pink:
               pinkPlayers.Add(player);
               break;
         }
      }
      
      int readyCount = 0;
      
      foreach (var playerInRoom in playersInRoom)
      {
            if (playerInRoom.isReady)
            {
               readyCount++;
            }

            if (readyCount >= GameAdministrator.instance.playerPerGame)
            { 
               if(pinkPlayers.Count != 2 || greenPlayers.Count != 2) return;
               StartGame();
            }
      }

      this.readyCount.text = readyCount.ToString() + "/4";
   }
   
   public void OnEvent(EventData photonEvent)
   {
      if (photonEvent.Code == RaiseEvent.ReadySelect)
      {
         Hashtable data = (Hashtable) photonEvent.CustomData;
         
         CheckPlayersInRoom(data);
      }
   }

   #region DEBUG

   public void DEBUG_TeleportToLevel()
   {
      GameAdministrator.localPlayer.controller.agent.Warp(spawnGreen[0].position);
      gameIsStarted = true;
      lastTickTime = PhotonNetwork.Time;
   }

   #endregion
}

[Serializable] public class PlayerInRoom
{
   public PlayerManager pManager;
   public bool isReady;
   public Enums.Team pTeam;
}
