using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Realtime;
using System.Text;

public class GameSceneManager : MonoBehaviourPunCallbacks
{
    [SerializeField] List<string> messageList = new List<string>();
    [SerializeField] TextMeshProUGUI messageText;

    private PhotonView _pv;

    public Dictionary<Player, bool> alivePlayerMap = new Dictionary<Player, bool>();

    void Start()
    {
        _pv = this.gameObject.GetComponent<PhotonView>();

        if(PhotonNetwork.CurrentRoom == null)
        {
            SceneManager.LoadScene("Lobby");
            return;
        }
        else
        {
            InitGame();
        }
    }

    public void InitGame()
    {
        alivePlayerMap.Clear();

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            alivePlayerMap[player] = true;
        }

        float spawnPointX = Random.Range(-5f, 5f);
        float spawnPointY = 1;

        PhotonNetwork.Instantiate("Player", new Vector3(spawnPointX, spawnPointY, 0), Quaternion.identity);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        alivePlayerMap[newPlayer] = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (alivePlayerMap.ContainsKey(otherPlayer))
        {
            alivePlayerMap.Remove(otherPlayer);
        }

        if(PhotonNetwork.IsMasterClient)
        {
            _pv.RPC("RPC_SendMessage", RpcTarget.All, $"{otherPlayer.NickName} has left the game.");
        }
    }
}
