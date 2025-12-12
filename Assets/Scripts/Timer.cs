using UnityEngine;
using TMPro;
public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float remainingTime;

    [Header("Sound (optional)")]
    [SerializeField] private AudioClip tenSecondClip;
    [SerializeField] private AudioSource audioSource; // optional, will PlayOneShot if provided

    [SerializeField] private float tenSecondThreshold = 12f;

    private bool playedTenSecondSound = false;

    // Update is called once per frame
    void Update()
    {
        if (remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime < 0f)
                remainingTime = 0f;
        }
        else
        {
            // timer is zero
            timerText.color = Color.red;
        }

        // Play the 10-second sound once when threshold is reached
        if (!playedTenSecondSound && remainingTime <= tenSecondThreshold && remainingTime > 0f)
        {
            PlayTenSecondSound();
            playedTenSecondSound = true;
        }

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void PlayTenSecondSound()
    {
        if (tenSecondClip == null) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(tenSecondClip);
        }
        else
        {
            // fallback to one-shot at camera position
            Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(tenSecondClip, pos);
        }
    }
}
