using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    [Header("Configuration")]
    public mainMenu_Music musicController;

    [Header("Game start settings")]
    public string gameSceneName = "ReceptionScene";
    public AudioClip btnStartAudio;
    [Range(0f, 1f)] public float startButtonVolume = 0.3f;
    public CanvasGroup blackScreenCanvas;
    public float fadeInDuration = 1.5f;
    public float fadeOutDuration = 1.8f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        closeSettings();
        Time.timeScale = 1f;


        blackScreenCanvas.alpha = 1f;
        blackScreenCanvas.blocksRaycasts = true;

        StartCoroutine(actionFadeIn(blackScreenCanvas));
    }


    IEnumerator actionFadeIn(CanvasGroup canvas)
    {
        Debug.Log("Start triggered");
        float elapsed = 0;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
            yield return null;
        }

        canvas.alpha = 0f;
        canvas.blocksRaycasts = false;
    }

    public void openSettings()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

    }
    public void closeSettings()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

    }

    // ---- Button actions ----
    public void actionExit()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void actionStartGame()
    {
        if (musicController != null)
        {
            musicController.playStartSound(btnStartAudio);
            // Begin fade out and change to next scene.
            StartCoroutine(musicController.FadeOutRoutine(gameSceneName));
        }
        else SceneManager.LoadScene(gameSceneName);
    }
}
