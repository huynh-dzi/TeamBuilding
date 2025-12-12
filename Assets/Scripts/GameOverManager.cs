using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameOverManager : MonoBehaviourPunCallbacks
{
    public void OnClickReturn()
    {
        SceneManager.LoadScene("Lobby");
    }
}
