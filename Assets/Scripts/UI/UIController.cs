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
    public string gameSceneName = "CamillaScene";
    public AudioClip btnStartAudio;
    [Range(0f, 1f)] public float startButtonVolume = 0.3f;
    public float fadeOutDuration = 1.8f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        closeSettings();
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
