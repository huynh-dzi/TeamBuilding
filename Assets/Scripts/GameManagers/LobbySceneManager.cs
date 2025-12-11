using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Realtime;
using System.Text;

public class LobbySceneManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField inputRoomName;
    [SerializeField] TMP_InputField inputPlayerName;
    [SerializeField] private TMP_Text connectionStatusText;
    [SerializeField] private TextMeshProUGUI roomListText;
    void Start()
    {
        if (PhotonNetwork.IsConnected == false)
        {
            SceneManager.LoadScene("StartMenu");
        }
        else
        {
            if (PhotonNetwork.CurrentLobby == null)
            {
                PhotonNetwork.JoinLobby();
            }

        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    // Update is called once per frame
    public override void OnJoinedLobby()
    {
        print("Joined Lobby");
    }

    public string GetRoomName()
    {
        string roomName = inputRoomName.text;
        return roomName.Trim();
    }

    public string GetPlayerName()
    {
        string playerName = inputPlayerName.text;
        return playerName.Trim();
    }

    public void OnClickCreateRoom()
    {
        string roomName = GetRoomName();
        string playerName = GetPlayerName();

        if (string.IsNullOrEmpty(playerName))
        {
            connectionStatusText.text = "Player name is invalid.";
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = playerName;

        if (string.IsNullOrEmpty(roomName) == false)
        {
            PhotonNetwork.CreateRoom(roomName);
            connectionStatusText.text = "Creating room...";
        }
        else 
        { 
            connectionStatusText.text = "Room name is invalid.";
            return;
        }
    }

    public void OnClickJoinRoom()
    {
        string roomName = GetRoomName();
        string playerName = GetPlayerName();

        if (string.IsNullOrEmpty(playerName))
        {
            connectionStatusText.text = "Player name is invalid.";
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = playerName;

        if (string.IsNullOrEmpty(roomName) == false)
        {
            PhotonNetwork.JoinRoom(roomName);
            connectionStatusText.text = "Joining room...";
        }
        else 
        { 
            connectionStatusText.text = "Room name is invalid.";
            return;
        }
    }

    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene("RoomScene");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        StringBuilder sb = new StringBuilder();

        foreach (RoomInfo room in roomList)
        {
            if (room.PlayerCount > 0)
            { 
                sb.AppendLine("Room Name: " + room.Name + " | Players: " + room.PlayerCount);
            }
            roomListText.text = sb.ToString();
        }
    }
}
