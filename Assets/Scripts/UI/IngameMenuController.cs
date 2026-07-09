using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class IngameMenuController : MonoBehaviour
{
    [Header("Scene configuration")]
    public string mainMenuSceneName = SceneNames.MainMenu;
    public string receptionSceneName = SceneNames.Reception;
    public float fadeOutDuration = 1.5f;
    public CanvasGroup sceneCanvasGroup;

    [Header("Menu objects")]
    public RectTransform buttonTransform;
    public CanvasGroup panelObject;
    public CanvasGroup settingsPanel;
    public CanvasGroup buttonContainer;

    [Header("Audio settings")]
    public AudioMixer mainMixer;
    public AudioSource menuSoundSource;
    public AudioClip menuOnSound;
    public AudioClip menuOffSound;

    private bool isOpen;

    public void ToggleMenu()
    {
        isOpen = !isOpen;

        Time.timeScale = isOpen ? 0f : 1f;

        if (menuSoundSource != null)
        {
            AudioClip clipToPlay = isOpen ? menuOnSound : menuOffSound;
            if (clipToPlay != null)
            {
                menuSoundSource.PlayOneShot(clipToPlay);
            }
        }

        if (isOpen)
        {
            CloseSettingsPanel();
        }

        SetCanvasGroupState(panelObject, isOpen);
    }

    public void GoToMainMenu()
    {
        GoToScene(mainMenuSceneName);
    }

    public void GoToReception()
    {
        if (GameSession.I != null)
            GameSession.I.ClearCustomer();

        GoToScene(receptionSceneName);
    }

    public void OpenSettingsPanel()
    {
        if (buttonContainer == null || settingsPanel == null)
        {
            Debug.LogError("IngameMenuController: buttonContainer or settingsPanel are null.");
            return;
        }

        SetCanvasGroupState(buttonContainer, false);
        SetCanvasGroupState(settingsPanel, true);
    }

    public void CloseSettingsPanel()
    {
        if (buttonContainer == null || settingsPanel == null)
        {
            Debug.LogError("IngameMenuController: buttonContainer or settingsPanel are null.");
            return;
        }

        SetCanvasGroupState(settingsPanel, false);
        SetCanvasGroupState(buttonContainer, true);
    }

    private void GoToScene(string sceneName)
    {
        Time.timeScale = 1f;
        StartCoroutine(TransitionToScene(sceneName));
    }

    private void SetCanvasGroupState(CanvasGroup canvasGroup, bool isVisible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.blocksRaycasts = isVisible;
        canvasGroup.interactable = isVisible;
    }

    IEnumerator TransitionToScene(string sceneName)
    {
        yield return StartCoroutine(SceneTransitionService.FadeOutAndLoadScene(
            sceneName,
            sceneCanvasGroup,
            mainMixer,
            fadeOutDuration));
    }
}
