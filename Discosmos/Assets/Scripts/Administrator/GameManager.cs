using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Tools;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
   [Header("SPAWN")]
   public Transform spawnPoint;
   List<PlayerManager> playerManagers;
   private bool allHere;
   
   private void Awake()
   {
      GameAdministrator.gameState = Enums.GameState.Game;
   }

   private void Start()
   {
      GameAdministrator.localPlayer.controller.transform.position = spawnPoint.position;
      GameAdministrator.UpdatePlayersList();
      
      EnablePlayers();
   }

   private void Update()
   {
      if (!allHere)
      {
         if (GameAdministrator.gameState == Enums.GameState.Game && GameAdministrator.players.Count < GameAdministrator.playerPerGame)
         {
            GameAdministrator.UpdatePlayersList();
         }
         else if (GameAdministrator.gameState == Enums.GameState.Game && GameAdministrator.players.Count == GameAdministrator.playerPerGame)
         {
            foreach (PlayerManager player in GameAdministrator.players)
            {
               player.ActivePlayerVisual();
            }
            allHere = true;
         }
      }
     
   }

   void EnablePlayers()
   {
      foreach (PlayerManager player in GameAdministrator.players)
      {
         player.ActivePlayerVisual();
      }
   }
   
   public override void OnPlayerEnteredRoom(Player newPlayer)
   {
      base.OnPlayerEnteredRoom(newPlayer);
      
      allHere = false;
   }
}
