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
   [Header("REFERENCES")] 
   public ConvoyBehavior convoy;
   
   [Header("SPAWN")]
   public Transform spawnPoint;
   public List<PlayerManager> playerManagers;
   
   public List<PlayerManager> pinkPlayers;
   public bool pinkTeamReady;
   public List<PlayerManager> greenPlayers;
   public bool greenTeamReady;

   public Transform[] spawnPink;
   public Transform[] spawnGreen;
   
   public bool gameIsFull;
   public float timeBeforeStart = 60;
   public float startTimer;
   public bool gameIsStarted;

   private void Awake()
   {
      pinkPlayers = new List<PlayerManager>();
      greenPlayers = new List<PlayerManager>();
      playerManagers = new List<PlayerManager>();
      
      GameAdministrator.gameState = Enums.GameState.Game;
      
      CheckPlayerCounts();
   }

   private void Start()
   {
     // GameAdministrator.localPlayer.controller.transform.localPosition = spawnPoint.position;
      GameAdministrator.localPlayer.controller.agent.Warp(spawnPoint.position);
      GameAdministrator.UpdatePlayersList();
   }

   private void Update()
   {
      if (gameIsFull && !gameIsStarted)
      {
         if (startTimer >= timeBeforeStart)
         {
            gameIsStarted = true;
            ForceStartGame();
         }
         else
         {
            startTimer += Time.deltaTime;
         }
      }
      
   }

   public void ForceStartGame()
   {

      if (PhotonNetwork.LocalPlayer.IsMasterClient)
      {
         playerManagers.Clear();
         pinkPlayers.Clear();
         greenPlayers.Clear();
         playerManagers.AddRange(FindObjectsOfType<PlayerManager>());

         pinkPlayers.Add(playerManagers[0]);
         pinkPlayers.Add(playerManagers[1]);
         greenPlayers.Add(playerManagers[2]);
         greenPlayers.Add(playerManagers[3]);

         pinkPlayers[0].SendPlayerCharacter(0);
         pinkPlayers[0].SendPlayerTeam(1);
         //pinkPlayers[0].controller.enabled = true;
         
         pinkPlayers[1].SendPlayerCharacter(1);
         pinkPlayers[1].SendPlayerTeam(1);
        // pinkPlayers[1].controller.enabled = true;

         greenPlayers[0].SendPlayerCharacter(0);
         greenPlayers[0].SendPlayerTeam(0);
        // greenPlayers[0].controller.enabled = true;

         greenPlayers[1].SendPlayerCharacter(1);
         greenPlayers[1].SendPlayerTeam(0);
        // greenPlayers[1].controller.enabled = true;

         int[] pinkId = new int[]
         {
            pinkPlayers[0].photonView.ViewID,
            pinkPlayers[1].photonView.ViewID
         };
         
         int[] greenId = new int[]
         {
            greenPlayers[0].photonView.ViewID,
            greenPlayers[1].photonView.ViewID
         };
         
         Hashtable data = new Hashtable()
         {
            {"Pink", pinkId},
            {"Green", greenId}
         };

         RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCacheGlobal};

         PhotonNetwork.RaiseEvent(RaiseEvent.ForceStart, data, raiseEventOptions, SendOptions.SendReliable);
      }
   }

   public void UpdateForceList(Hashtable data)
   {
      Debug.Log("Update Force List");
      int[] pinkId = (int[]) data["Pink"];
      int[] greenId = (int[]) data["Green"];

      pinkPlayers.Clear();
      greenPlayers.Clear();

      pinkPlayers.Add(PhotonView.Find(pinkId[0]).GetComponent<PlayerManager>());
      pinkPlayers.Add(PhotonView.Find(pinkId[1]).GetComponent<PlayerManager>());
      greenPlayers.Add(PhotonView.Find(greenId[0]).GetComponent<PlayerManager>());
      greenPlayers.Add(PhotonView.Find(greenId[1]).GetComponent<PlayerManager>());

      StartGame();
   }
   
   public void StartGame()
   {
      GameAdministrator.localPlayer.interfaceManager.championSelectCanvas.SetActive(false);

      switch (GameAdministrator.localPlayer.currentTeam)
      {
         case Enums.Team.Green:
            if (GameAdministrator.localPlayer.photonView.ControllerActorNr % 2 == 0)
            {
               GameAdministrator.localPlayer.controller.agent.Warp(spawnGreen[0].position);
            }
            else
            {
               GameAdministrator.localPlayer.controller.agent.Warp(spawnGreen[1].position);
            }
            break;
         
         case Enums.Team.Pink:
            if (GameAdministrator.localPlayer.photonView.ControllerActorNr % 2 == 0)
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
   
   public override void OnPlayerEnteredRoom(Player newPlayer)
   {
      base.OnPlayerEnteredRoom(newPlayer);

      CheckPlayerCounts();
   }

   public override void OnPlayerLeftRoom(Player otherPlayer)
   {
      base.OnPlayerLeftRoom(otherPlayer);
      
      CheckPlayerCounts();
   }

   public void CheckPlayerCounts()
   {
      if (PhotonNetwork.CurrentRoom.PlayerCount == GameAdministrator.instance.playerPerGame)
      {
         if (!gameIsFull)
         {
            gameIsFull = true;
            startTimer = 0;
         }
         
         playerManagers.Clear();
         pinkPlayers.Clear();
         greenPlayers.Clear();
         
         playerManagers.AddRange(FindObjectsOfType<PlayerManager>());

         foreach (var manager in playerManagers)
         {
            switch (manager.currentTeam)
            {
               case Enums.Team.Green:
                  if(!greenPlayers.Contains(manager)) greenPlayers.Add(manager);
                  break;
               
               case Enums.Team.Pink:
                  if(!pinkPlayers.Contains(manager)) pinkPlayers.Add(manager);
                  break;
            }
         }

         if (pinkPlayers.Count == GameAdministrator.instance.playerPerGame / 2 &&
             greenPlayers.Count == GameAdministrator.instance.playerPerGame)
         {
            if (pinkPlayers[0].currentCharacter != pinkPlayers[1].currentCharacter)
            {
               pinkTeamReady = true;
            }

            if (greenPlayers[0].currentCharacter != greenPlayers[1].currentCharacter)
            {
               greenTeamReady = true;
            }
         }

         if (pinkTeamReady && greenTeamReady)
         {
            GameAdministrator.localPlayer.interfaceManager.championSelectCanvas.SetActive(false);
            GameAdministrator.localPlayer.ResetPlayerStats();
            StartGame();
         }
      }
      else
      {
         gameIsFull = false;
      }

   }

   public void OnEvent(EventData photonEvent)
   {
      if (photonEvent.Code == RaiseEvent.ForceStart)
      {
         Hashtable data = (Hashtable) photonEvent.CustomData;
         
         UpdateForceList(data);
      }
      
      if (photonEvent.Code == RaiseEvent.SetTeam && !gameIsStarted)
      {
         CheckPlayerCounts();
      }
   }

   #region DEBUG

   public void DEBUG_TeleportToLevel()
   {
      GameAdministrator.localPlayer.controller.agent.Warp(spawnGreen[0].position);
   }

   #endregion
}
