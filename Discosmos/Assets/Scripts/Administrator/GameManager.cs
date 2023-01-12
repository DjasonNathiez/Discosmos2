using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Tools;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
   [Header("SPAWN")]
   public Transform spawnPoint;
   public List<PlayerManager> playerManagers;
   
   public List<PlayerManager> pinkPlayers;
   public bool pinkTeamReady;
   public List<PlayerManager> greenPlayers;
   public bool greenTeamReady;

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
      GameAdministrator.localPlayer.controller.transform.position = spawnPoint.position;
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
      playerManagers.Clear();
      pinkPlayers.Clear();
      greenPlayers.Clear();
      playerManagers.AddRange(FindObjectsOfType<PlayerManager>());

      pinkPlayers.Add(playerManagers[0]);
      pinkPlayers.Add(playerManagers[1]);
      greenPlayers.Add(playerManagers[2]);
      greenPlayers.Add(playerManagers[3]);

      if (pinkPlayers[0] == GameAdministrator.localPlayer)
      {
         GameAdministrator.localPlayer.SendPlayerCharacter(0);
         GameAdministrator.localPlayer.SendPlayerTeam(1);
      }
      
      if (pinkPlayers[1] == GameAdministrator.localPlayer)
      {
         GameAdministrator.localPlayer.SendPlayerCharacter(1);
         GameAdministrator.localPlayer.SendPlayerTeam(1);
      }
      
      if (greenPlayers[0] == GameAdministrator.localPlayer)
      {
         GameAdministrator.localPlayer.SendPlayerCharacter(0);
         GameAdministrator.localPlayer.SendPlayerTeam(0);
      }
      
      if (greenPlayers[1] == GameAdministrator.localPlayer)
      {
         GameAdministrator.localPlayer.SendPlayerCharacter(1);
         GameAdministrator.localPlayer.SendPlayerTeam(0);
      }
      
      StartGame();
   }
   
   public void StartGame()
   {
      Debug.Log("Game Start");
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
      if (photonEvent.Code == RaiseEvent.SetTeam && !gameIsStarted)
      {
         CheckPlayerCounts();
      }
   }
}
