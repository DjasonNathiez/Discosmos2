using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
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

   private bool localIsReady = false;
   
   public bool gameIsFull;
   public float timeBeforeStart = 60;
   public float startTimer;
   public bool gameIsStarted;

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
   }
   

   public void ReadyCheck()
   {
      Debug.Log("READY CHECK");
      localIsReady = !localIsReady;
      
      Hashtable data = new Hashtable()
      {
         {"Sender", GameAdministrator.localPlayer.pView.ViewID},
         {"IsReady", localIsReady}
      };
      
      RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCacheGlobal};

      PhotonNetwork.RaiseEvent(RaiseEvent.ReadySelect, data, raiseEventOptions, SendOptions.SendReliable);
   }

   public void StartGame()
   {
      GameAdministrator.localPlayer.interfaceManager.championSelectCanvas.SetActive(false);

      switch (GameAdministrator.localPlayer.currentTeam)
      {
         case Enums.Team.Green:
            if (GameAdministrator.localPlayer.pView.ControllerActorNr % 2 == 0)
            {
               GameAdministrator.localPlayer.controller.agent.Warp(spawnGreen[0].position);
            }
            else
            {
               GameAdministrator.localPlayer.controller.agent.Warp(spawnGreen[1].position);
            }
            break;
         
         case Enums.Team.Pink:
            if (GameAdministrator.localPlayer.pView.ControllerActorNr % 2 == 0)
            {
               GameAdministrator.localPlayer.controller.agent.Warp(spawnPink[0].position);
            }
            else
            {
               GameAdministrator.localPlayer.controller.agent.Warp(spawnPink[1].position);
            }
            break;
      }
      
      convoy.ActiveGameLoop();
   }

   public void EndGame(Enums.Team winner)
   {
      gameIsStarted = false;
      
      switch (winner)
      {
         case Enums.Team.Green:
            Debug.Log("Green lose");
            break;
         
         case Enums.Team.Pink:
            Debug.Log("Pink win");
            break;
      }
      
      //Set Win View
   }

   public void CheckPlayersInRoom(Hashtable data = null)
   {
      if (data == null) return;
      
      Debug.Log("CHECK");

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
            Debug.Log(playersInRoom[i].pManager + " at " + i);
            
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
      
      
         
      switch (pim.pManager.currentTeam)
      {
         case Enums.Team.Green:
            for (int i = 0; i < greenPlayers.Count; i++)
            {
               if (greenPlayers[i].pManager == pim.pManager)
               {
                  greenPlayers[i].isReady = pim.isReady;
                  greenPlayers[i].pTeam = pim.pTeam;
                  return;
               }

               if (i == greenPlayers.Count)
               {
                  if (greenPlayers[i].pManager == pim.pManager)
                  {
                     greenPlayers[i].isReady = pim.isReady;
                     greenPlayers[i].pTeam = pim.pTeam;
                     break;
                  }
                  else
                  {
                     greenPlayers.Add(pim);
                     break;
                  }
               }
               
            }
            break;
               
         case Enums.Team.Pink:
            for (int i = 0; i < pinkPlayers.Count; i++)
            {
               if (pinkPlayers[i].pManager == pim.pManager)
               {
                  pinkPlayers[i].isReady = pim.isReady;
                  pinkPlayers[i].pTeam = pim.pTeam;
                  return;
               }

               if (i == pinkPlayers.Count)
               {
                  if (pinkPlayers[i].pManager == pim.pManager)
                  {
                     pinkPlayers[i].isReady = pim.isReady;
                     pinkPlayers[i].pTeam = pim.pTeam;
                     break;
                  }
                  else
                  {
                     pinkPlayers.Add(pim);
                     break;
                  }
               }
               
            }
            break;
      }

      int readyCount = 0;
      
     
         foreach (var playerInRoom in playersInRoom)
         {
            if (playerInRoom.isReady == true)
            {
               readyCount++;
            }

            if (readyCount >= GameAdministrator.instance.playerPerGame)
            {
            StartGame(); 
            }
         }
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
   }

   #endregion
}

[Serializable] public class PlayerInRoom
{
   public PlayerManager pManager;
   public bool isReady;
   public Enums.Team pTeam;
}
