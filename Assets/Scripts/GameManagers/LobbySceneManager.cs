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
    [SerializeField] private TMP_Text connectionStatusText;
    [SerializeField] private TextMeshProUGUI roomListText;
    void Start()
    {
        if(PhotonNetwork.IsConnected == false)
        {
            SceneManager.LoadScene("StartMenu");
        }

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

    public void OnClickCreateRoom()
    {
        string roomName = GetRoomName();

        if (roomName.Length > 0)
        {
            PhotonNetwork.CreateRoom(roomName);
            connectionStatusText.text = "Creating room...";
        }
        else 
        { 
            connectionStatusText.text = "Room name is invalid.";
        }
    }

    public void OnClickJoinRoom()
    {
        string roomName = GetRoomName();
        if (roomName.Length > 0)
        {
            PhotonNetwork.JoinRoom(roomName);
            connectionStatusText.text = "Joining room...";
        }
        else 
        { 
            connectionStatusText.text = "Room name is invalid.";
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
