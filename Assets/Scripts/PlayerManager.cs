using Photon.Pun;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PhotonView _pv;
    private InputManager inputManager;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
        inputManager = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();

        if (_pv == null)
            Debug.LogError("PlayerManager: missing PhotonView on player prefab.");
        if (playerMovement == null)
            Debug.LogError("PlayerManager: PlayerMovement missing on player prefab.");
    }

    private void Start()
    {
        // Only assign the local camera to the local player's movement component.
        if (_pv != null && _pv.IsMine)
        {
            if (playerMovement != null)
            {
                if (Camera.main != null)
                    playerMovement.SetCamera(Camera.main.transform);
                else
                    Debug.LogWarning("PlayerManager: Camera.main is null - assign camera manually.");
            }

            // Ensure InputManager is enabled only for local player (prefer InputManager on the prefab)
            if (inputManager == null)
                Debug.LogWarning("PlayerManager: InputManager missing on local player prefab.");
        }
        else
        {
            // remote instances: make sure inputManager (if present) is disabled
            if (inputManager != null)
                inputManager.enabled = false;
        }
    }

    private void Update()
    {
        // Only process input for local player
        if (_pv == null || _pv.IsMine)
        {
            inputManager?.HandleAllInputs();
        }
    }

    private void FixedUpdate()
    {
        // Only run movement for the local player
        if (_pv == null || _pv.IsMine)
        {
            playerMovement?.HandleAllMovement();
        }
    }
}
