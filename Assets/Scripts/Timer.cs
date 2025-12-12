using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float remainingTime;

    [Header("Sound (optional)")]
    [SerializeField] private AudioClip tenSecondClip;

    [Header("Thresholds / Delays")]
    [SerializeField] private float tenSecondThreshold = 12f;
    [SerializeField] private float gameOverDelay = 4f; // seconds to wait before loading GameOver

    private bool playedTenSecondSound = false;
    private bool gameOverScheduled = false;

    GameSceneManager gameSceneManager;

    private void Start()
    {
        gameSceneManager = GetComponent<GameSceneManager>();
    }

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
            // timer is zero - schedule a delayed GameOver load once
            if (!gameOverScheduled)
            {
                gameOverScheduled = true;
                if (timerText != null) timerText.color = Color.red;
                StartCoroutine(DelayedLoadGameOver(gameOverDelay));
            }
        }

        // Play the 10-second sound once when threshold is reached
        if (!playedTenSecondSound && remainingTime <= tenSecondThreshold && remainingTime > 0f)
        {
            PlayTenSecondSound();
            playedTenSecondSound = true;
        }

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private IEnumerator DelayedLoadGameOver(float delay)
    {
        yield return new WaitForSeconds(delay);

        // If you need to run GameSceneManager.TimesUp, call it here instead of directly loading the scene.
        if (gameSceneManager != null)
        {
            gameSceneManager.TimesUp();
        }
        else
        {
            SceneManager.LoadScene("GameOver");
        }
    }

    private void PlayTenSecondSound()
    {
        if (tenSecondClip == null) return;
        // fallback to one-shot at camera position
        Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(tenSecondClip, pos);
    }
}
