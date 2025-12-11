using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text connectionStatusText;
    void Start()
    {

    }


    void Update()
    {

    }

    public void OnClickStart()
    {
        PhotonNetwork.ConnectUsingSettings();
        connectionStatusText.text = "Connecting to server...";
    }

    public void OnClickQuit()
    {
        PhotonNetwork.Disconnect();
        Application.Quit();
    }

    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Lobby");
        // PhotonNetwork.JoinOrCreateRoom("MainRoom", new Photon.Realtime.RoomOptions { MaxPlayers = 10 }, null);
    }
}
