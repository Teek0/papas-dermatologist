using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class UIController : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    [Header("Configuration")]
    public AudioMixer mainMixer;
    public UISoundPlayer uiSoundPlayer;
    [SerializeField] private CanvasGroup settingsPanel;

    [Header("Game start settings")]
    public string gameSceneName = SceneNames.Reception;
    public AudioClip btnStartAudio;
    public CanvasGroup blackScreenCanvas;
    public float fadeInDuration = 2.5f;
    public float fadeOutDuration = 1.8f;

    private void Awake()
    {
        canvasGroup = ResolveSettingsPanel();

        if (canvasGroup != null)
            CloseSettings();
        else
            Debug.LogWarning("UIController: settingsPanel and local CanvasGroup are null. Settings panel will not be controlled.");

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
        CloseSettings();

        if (uiSoundPlayer != null)
            uiSoundPlayer.PlayOneShot(btnStartAudio);

        StartCoroutine(SceneTransitionService.FadeOutAndLoadScene(
            gameSceneName,
            blackScreenCanvas,
            1f,
            false,
            mainMixer,
            fadeOutDuration));
    }

    public void OpenSettings()
    {
        if (EnsureCanvasGroup())
            SetCanvasGroupState(canvasGroup, true);
    }

    public void ToggleSettings()
    {
        if (EnsureCanvasGroup())
            SetCanvasGroupState(canvasGroup, !IsCanvasGroupVisible(canvasGroup));
    }

    public void CloseSettings()
    {
        if (EnsureCanvasGroup())
            SetCanvasGroupState(canvasGroup, false);
    }

    private bool EnsureCanvasGroup()
    {
        if (canvasGroup == null || !canvasGroup.gameObject.scene.IsValid())
            canvasGroup = ResolveSettingsPanel();

        return canvasGroup != null;
    }

    private CanvasGroup ResolveSettingsPanel()
    {
        if (settingsPanel != null && settingsPanel.gameObject.scene.IsValid())
            return settingsPanel;

        if (settingsPanel != null)
        {
            CanvasGroup sceneSettingsPanel = FindSceneSettingsPanel();
            if (sceneSettingsPanel != null)
                return sceneSettingsPanel;
        }

        CanvasGroup localCanvasGroup = GetComponent<CanvasGroup>();
        if (localCanvasGroup != null)
            return localCanvasGroup;

        return FindSceneSettingsPanel();
    }

    private CanvasGroup FindSceneSettingsPanel()
    {
        CanvasGroup[] canvasGroups = FindObjectsByType<CanvasGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (CanvasGroup candidate in canvasGroups)
        {
            if (candidate.gameObject.scene == gameObject.scene && candidate.gameObject.name == "Settings_Panel")
                return candidate;
        }

        return null;
    }

    private bool IsCanvasGroupVisible(CanvasGroup targetCanvasGroup)
    {
        return targetCanvasGroup != null && targetCanvasGroup.alpha > 0.5f;
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
