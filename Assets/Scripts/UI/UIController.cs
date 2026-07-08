using System.Collections;
using UnityEngine;

public class UIController : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    [Header("Configuration")]
    public MainMenuMusicPlayer musicController;

    [Header("Game start settings")]
    public string gameSceneName = SceneNames.Reception;
    public AudioClip btnStartAudio;
    [Range(0f, 1f)] public float startButtonVolume = 0.3f;
    public CanvasGroup blackScreenCanvas;
    public float fadeInDuration = 2.5f;
    public float fadeOutDuration = 1.8f;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            CloseSettings();
        else
            Debug.LogError("UIController: CanvasGroup null.");

        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (blackScreenCanvas != null)
        {
            SetCanvasGroupState(blackScreenCanvas, true);
            StartCoroutine(FadeInRoutine(blackScreenCanvas));
        }
    }

    private IEnumerator FadeInRoutine(CanvasGroup canvas)
    {
        float elapsed = 0;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
            yield return null;
        }

        SetCanvasGroupState(canvas, false);
    }

    // ---- Button actions ----
    public void ExitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void StartGame()
    {
        if (musicController != null)
            musicController.PlayStartSound(btnStartAudio);

        StartCoroutine(SceneTransitionService.FadeOutAndLoadScene(
            gameSceneName,
            blackScreenCanvas,
            1f,
            false,
            musicController != null ? musicController.mainMixer : null,
            fadeOutDuration));
    }

    public void OpenSettings()
    {
        if (EnsureCanvasGroup())
            SetCanvasGroupState(canvasGroup, true);
    }

    public void CloseSettings()
    {
        if (EnsureCanvasGroup())
            SetCanvasGroupState(canvasGroup, false);
    }

    private bool EnsureCanvasGroup()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        return canvasGroup != null;
    }

    private void SetCanvasGroupState(CanvasGroup targetCanvasGroup, bool isVisible)
    {
        if (targetCanvasGroup == null)
            return;

        targetCanvasGroup.alpha = isVisible ? 1f : 0f;
        targetCanvasGroup.blocksRaycasts = isVisible;
        targetCanvasGroup.interactable = isVisible;
    }
}
