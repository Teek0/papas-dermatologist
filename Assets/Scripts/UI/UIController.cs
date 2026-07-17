using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class UIController : MonoBehaviour
{
    private CanvasGroup settingsCanvasGroup;
    private CanvasGroup creditsCanvasGroup;
    [SerializeField] private TutorialController tutorialController;

    [Header("Configuration")]
    public AudioMixer mainMixer;
    public UISoundPlayer uiSoundPlayer;
    [SerializeField] private CanvasGroup settingsPanel;
    [SerializeField] private CanvasGroup creditsPanel;

    [Header("Game start settings")]
    public string gameSceneName = SceneNames.Reception;
    public AudioClip btnStartAudio;
    public CanvasGroup blackScreenCanvas;
    public float fadeInDuration = 2.5f;
    public float fadeOutDuration = 1.8f;

    private void Awake()
    {
        settingsCanvasGroup = ResolvePanel(settingsPanel, "Settings_Panel");
        creditsCanvasGroup = ResolvePanel(creditsPanel, "Credits_Panel");

        if (settingsCanvasGroup != null)
            CloseSettings();
        else
            Debug.LogWarning("UIController: settingsPanel and local CanvasGroup are null. Settings panel will not be controlled.");

        if (creditsCanvasGroup != null)
            CloseCredits();

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
        CloseCredits();

        if (tutorialController != null && tutorialController.ShouldShowBeforeStart())
        {
            tutorialController.OpenForStart();
            return;
        }

        StartGameDirect();
    }

    public void OpenSettings()
    {
        if (IsCreditsPanelController())
        {
            OpenCredits();
            return;
        }

        if (EnsureSettingsCanvasGroup())
        {
            CloseCredits();
            SetCanvasGroupState(settingsCanvasGroup, true);
        }
    }

    public void ToggleSettings()
    {
        if (IsCreditsPanelController())
        {
            ToggleCredits();
            return;
        }

        if (EnsureSettingsCanvasGroup())
        {
            bool shouldShow = !IsCanvasGroupVisible(settingsCanvasGroup);
            CloseCredits();
            SetCanvasGroupState(settingsCanvasGroup, shouldShow);
        }
    }

    public void CloseSettings()
    {
        if (IsCreditsPanelController())
        {
            CloseCredits();
            return;
        }

        if (EnsureSettingsCanvasGroup())
            SetCanvasGroupState(settingsCanvasGroup, false);
    }

    public void OpenCredits()
    {
        if (EnsureCreditsCanvasGroup())
        {
            CloseSettings();
            SetCanvasGroupState(creditsCanvasGroup, true);
        }
    }

    public void ToggleCredits()
    {
        if (EnsureCreditsCanvasGroup())
        {
            if (IsCanvasGroupVisible(creditsCanvasGroup))
            {
                SetCanvasGroupState(creditsCanvasGroup, false);
                return;
            }

            CloseSettings();
            SetCanvasGroupState(creditsCanvasGroup, true);
        }
    }

    public void CloseCredits()
    {
        if (EnsureCreditsCanvasGroup())
            SetCanvasGroupState(creditsCanvasGroup, false);
    }

    private bool EnsureSettingsCanvasGroup()
    {
        if (settingsCanvasGroup == null || !settingsCanvasGroup.gameObject.scene.IsValid())
            settingsCanvasGroup = ResolvePanel(settingsPanel, "Settings_Panel");

        return settingsCanvasGroup != null;
    }

    private bool EnsureCreditsCanvasGroup()
    {
        if (creditsCanvasGroup == null || !creditsCanvasGroup.gameObject.scene.IsValid())
            creditsCanvasGroup = ResolvePanel(creditsPanel, "Credits_Panel");

        return creditsCanvasGroup != null;
    }

    private bool IsCreditsPanelController()
    {
        return gameObject.name == "Credits_Panel";
    }

    private CanvasGroup ResolvePanel(CanvasGroup serializedPanel, string fallbackName)
    {
        if (serializedPanel != null && serializedPanel.gameObject.scene.IsValid())
            return serializedPanel;

        if (serializedPanel != null)
        {
            CanvasGroup scenePanel = FindScenePanel(fallbackName);
            if (scenePanel != null)
                return scenePanel;
        }

        CanvasGroup localCanvasGroup = GetComponent<CanvasGroup>();
        if (localCanvasGroup != null && gameObject.name == fallbackName)
            return localCanvasGroup;

        return FindScenePanel(fallbackName);
    }

    private CanvasGroup FindScenePanel(string panelName)
    {
        CanvasGroup[] canvasGroups = FindObjectsByType<CanvasGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (CanvasGroup candidate in canvasGroups)
        {
            if (candidate.gameObject.scene == gameObject.scene && candidate.gameObject.name == panelName)
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

    public void StartGameDirect()
    {
        CloseSettings();
        CloseCredits();

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

    public void OpenTutorial()
    {
        CloseSettings();
        CloseCredits();

        if (tutorialController != null)
            tutorialController.OpenManual();
    }
}
