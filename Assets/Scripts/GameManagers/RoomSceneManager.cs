using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Realtime;
using System.Text;

public class RoomSceneManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text connectionStatusText;
    [SerializeField] private TextMeshProUGUI textRoomName;
    [SerializeField] private TextMeshProUGUI playerListText;

    [SerializeField] private Button buttonStartCoopMode;
    [SerializeField] private Button buttonStartCompeteMode;
    [SerializeField] private Image panelToChangeColor;
    [SerializeField] private TMP_Text nonMasterMessage;
    void Start()
    {
        if (PhotonNetwork.IsConnected == false)
        {
            SceneManager.LoadScene("StartMenu");
        }

        if (PhotonNetwork.CurrentRoom == null)
        {
            SceneManager.LoadScene("LobbyScene");
            return;
        }

        textRoomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Players in Room:");

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            sb.AppendLine("-" + player.NickName);
        }

        playerListText.text = sb.ToString();

        if (buttonStartCoopMode != null)
        {
            buttonStartCoopMode.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            buttonStartCoopMode.interactable = PhotonNetwork.IsMasterClient;
        }
            

        if (buttonStartCompeteMode != null)
        {
            buttonStartCompeteMode.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            buttonStartCompeteMode.interactable = PhotonNetwork.IsMasterClient;
        }

        if(!PhotonNetwork.IsMasterClient)
        {
            nonMasterMessage.gameObject.SetActive(true);
        }

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdatePlayerList();
    }

    public void OnClickStartGameCoop()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 1)
        {
            connectionStatusText.text = "Starting game...";
            SceneManager.LoadScene("MainGame1");
        }
        else
        {
            connectionStatusText.text = "At least 2 players are required to start game.";
        }
    }

    public void OnClickStartGameCompete()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 1)
        {
            connectionStatusText.text = "Starting game...";
            SceneManager.LoadScene("MainGame2");
        }
        else
        {
            connectionStatusText.text = "At least 2 players are required to start game.";
        }
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }
}
