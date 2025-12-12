using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerStamina : MonoBehaviourPunCallbacks
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float stamina = 100f;
    public float regenRate = 10f;

    [Header("UI")]
    public Slider staminaBar;

    [Header("Carrying")]
    public bool isCarrying = false;
    public float carriedWeight = 0f;

    private GameObject carriedObject;
    private PhotonView pv;

    void Start()
    {
        pv = GetComponent<PhotonView>();

        if (!pv.IsMine && staminaBar != null)
            staminaBar.gameObject.SetActive(false);

        if (pv.IsMine && staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = stamina;
        }
    }

    void Update()
    {
        if (!pv.IsMine)
            return;

        HandleStamina();
        UpdateUI();
    }

    void HandleStamina()
    {
        if (isCarrying)
        {
            stamina -= carriedWeight * Time.deltaTime;

            if (stamina <= 0f)
            {
                stamina = 0f;
                ForceDropCarriedObject();
            }
        }
        else
        {
            stamina += regenRate * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);
    }

    void UpdateUI()
    {
        if (staminaBar != null)
            staminaBar.value = stamina;
    }

    public void TryPickUp(GameObject brick)
    {
        if (!pv.IsMine) return;
        if (brick == null) return;

        PhotonView brickPV = brick.GetComponent<PhotonView>();
        if (brickPV != null)
            brickPV.TransferOwnership(PhotonNetwork.LocalPlayer);

        Rigidbody rb = brick.GetComponent<Rigidbody>();
        float weight = rb != null ? rb.mass : 1f;

        pv.RPC(nameof(RPC_StartCarrying), RpcTarget.AllBuffered, weight, brickPV != null ? brickPV.ViewID : -1);
    }

    public void TryDrop()
    {
        if (!pv.IsMine) return;
        pv.RPC(nameof(RPC_StopCarrying), RpcTarget.AllBuffered);
    }

    private void ForceDropCarriedObject()
    {
        TryDrop();
    }

    [PunRPC]
    public void RPC_StartCarrying(float weight, int objectViewID)
    {
        carriedWeight = weight;
        isCarrying = true;

        if (objectViewID != -1)
        {
            PhotonView objPV = PhotonView.Find(objectViewID);
            carriedObject = objPV != null ? objPV.gameObject : null;
        }
    }

    [PunRPC]
    public void RPC_StopCarrying()
    {
        isCarrying = false;
        carriedWeight = 0f;

        if (carriedObject != null)
        {
            PhotonView o = carriedObject.GetComponent<PhotonView>();
            if (o != null)
                o.TransferOwnership(PhotonNetwork.MasterClient);
        }

        carriedObject = null;
    }

    public void OnPunObservable(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(stamina);
            stream.SendNext(isCarrying);
            stream.SendNext(carriedWeight);
        }
        else
        {
            stamina = (float)stream.ReceiveNext();
            isCarrying = (bool)stream.ReceiveNext();
            carriedWeight = (float)stream.ReceiveNext();
        }
    }
}
