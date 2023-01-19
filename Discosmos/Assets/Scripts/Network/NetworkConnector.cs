using System;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkConnector : MonoBehaviourPunCallbacks
{
    [Header("INTERFACE")]
    public GameObject connectorGroup;
    public GameObject roomEnterGroup;
    
    [Header("CONNECTOR")]
    public TMP_InputField usernameInputField;
    public Button connectServerButton;
    public TextMeshProUGUI errorMessage;
    public TextMeshProUGUI serverStateMessage;

    [Header("ROOM")] 
    public TMP_InputField roomNameInputField;
    public Button enterRoomButton;
    
    private TypedLobby lobby = new ("World", LobbyType.Default);

    [Header("CLIENT")] 
    public GameObject clientPrefab;

    [Header("VISUAL")] 
    public MeshRenderer hubObjectRenderer;
    public Material connectMaterial;
    
    public void EnterServerButton()
    {
        if (usernameInputField.text != String.Empty)
        {
            PhotonNetwork.ConnectUsingSettings();
            
            serverStateMessage.text = "Connecting...";
            
            connectServerButton.interactable = false;
            usernameInputField.interactable = false;
        }
        else
        {
            errorMessage.text = "Please, enter a username.";
        }
    }

    #region INIT PLAYER

    void CreateLocalClient()
    {
        GameObject localClient = PhotonNetwork.Instantiate(clientPrefab.name, clientPrefab.transform.position, Quaternion.identity);
        GameAdministrator.localPlayer = localClient.GetComponent<PlayerManager>();
        
        GameAdministrator.localPlayer.GetComponent<PhotonView>().Owner.NickName = usernameInputField.text;
        
        GameAdministrator.localPlayer.Initialize();

        //DEBUG VISUAL FEEDBACK
        serverStateMessage.text = serverStateMessage.text + " | Nickame : " +
                                  GameAdministrator.localPlayer.pView.Owner.NickName + " | ViewID : " +
                                  GameAdministrator.localPlayer.pView.ViewID;
    }

    #endregion

    #region PHOTON METHODS

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        
        //DEBUG VISUAL FEEDBACK
        serverStateMessage.text = "Connected to server.";

        PhotonNetwork.JoinLobby(lobby);
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        
        //DEBUG VISUAL FEEDBACK
        serverStateMessage.text = "Connected to lobby.";

        switch (GameAdministrator.roomState)
        {
            case Enums.NetworkRoomState.Inside:
                break;
            
            case Enums.NetworkRoomState.Outside:
                PhotonNetwork.JoinOrCreateRoom("Hub", null,lobby);
                break;
            
            case Enums.NetworkRoomState.Switch:
                PhotonNetwork.JoinOrCreateRoom(roomNameInputField.text, null,lobby);
                break;
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        
        switch (GameAdministrator.gameState)
        {
            case Enums.GameState.Disconnected:
                
                serverStateMessage.text = "Connected to room : " + PhotonNetwork.CurrentRoom.Name + " | Current Lobby : " + PhotonNetwork.CurrentLobby.Name + " | Player in Room : " + PhotonNetwork.CurrentRoom.Players.Count;

                connectorGroup.SetActive(false);
                roomEnterGroup.SetActive(true);

                GameAdministrator.gameState = Enums.GameState.Hub;
                break;
            
            case Enums.GameState.Hub:
                serverStateMessage.text = "Connected to room : " + PhotonNetwork.CurrentRoom.Name + " | Current Lobby : " + PhotonNetwork.CurrentLobby.Name + " | Player in Room : " + PhotonNetwork.CurrentRoom.Players.Count;
                
                PhotonNetwork.AutomaticallySyncScene = true;
                
                CreateLocalClient();
                //SetRoomVisual();
                
                if (PhotonNetwork.IsMasterClient)
                {
                    CheckCountToLoadLevel(); 
                }
                
                break;
            
            case Enums.GameState.Game:
                serverStateMessage.text = "Connected to room : " + PhotonNetwork.CurrentRoom.Name + " | Current Lobby : " + PhotonNetwork.CurrentLobby.Name + " | Player in Room : " + PhotonNetwork.CurrentRoom.Players.Count;
                break;
        }
        
        GameAdministrator.roomState = Enums.NetworkRoomState.Inside;

        Debug.Log(GameAdministrator.gameState + "-" + GameAdministrator.roomState);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        PhotonNetwork.JoinLobby(lobby);
    }

    #endregion

    public void SetRoomVisual()
    {
        hubObjectRenderer.material = connectMaterial;
        roomEnterGroup.SetActive(false);
    }
    
    public void ChangeRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            GameAdministrator.roomState = Enums.NetworkRoomState.Switch;
            enterRoomButton.interactable = false;
            roomNameInputField.interactable = false;
        }
        else
        {
            PhotonNetwork.JoinOrCreateRoom(roomNameInputField.text, null,lobby);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        
        GameAdministrator.UpdatePlayersList();
        
        serverStateMessage.text = serverStateMessage.text + " | Nickame : " +
                                  GameAdministrator.localPlayer.pView.Owner.NickName + " | ViewID : " +
                                  GameAdministrator.localPlayer.pView.ViewID;

        if (PhotonNetwork.IsMasterClient)
        {
          CheckCountToLoadLevel(); 
        }
    }

    void CheckCountToLoadLevel()
    {
        if (PhotonNetwork.CurrentRoom.Name != "Hub" && PhotonNetwork.CurrentRoom.PlayerCount == GameAdministrator.connectToRoomNeedPlayer)
        {
            //LOAD THE LEVEL
            SceneManager.LoadScene(1);
        }
    }
}
