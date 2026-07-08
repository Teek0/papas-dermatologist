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

    private bool isOpen = false;

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
            actionSettingsBack();
        }

        //if (buttonTransform != null)
        //{
        //    buttonTransform.Rotate(0, 0, -90f);
        //}

        if (panelObject != null)
        {
            panelObject.alpha = isOpen ? 1f : 0f;
            panelObject.blocksRaycasts = isOpen;
            panelObject.interactable = isOpen;
        }
    }

    public void actionMainMenu()
    {
        Time.timeScale = 1f;
        StartCoroutine(TransitionToScene(mainMenuSceneName));
    }

    public void actionReception()
    {
        Time.timeScale = 1f;
        StartCoroutine(TransitionToScene(receptionSceneName));
    }

    public void actionSettingsMini()
    {
        if (buttonContainer != null)
        {
            if (settingsPanel != null)
            {
                buttonContainer.alpha = 0f;
                settingsPanel.alpha = 1f;

                settingsPanel.blocksRaycasts = true;
                settingsPanel.interactable = true;

                buttonContainer.blocksRaycasts = false;
                buttonContainer.interactable = false;
            }
        }
        else Debug.LogError("IngameMenuController: buttonContainer or settingsPanel are null.");
    }

    public void actionSettingsBack()
    {
        if (buttonContainer != null)
        {
            if (settingsPanel != null)
            {
                settingsPanel.alpha = 0f;
                buttonContainer.alpha = 1f;

                buttonContainer.blocksRaycasts = true;
                buttonContainer.interactable = true;

                settingsPanel.blocksRaycasts = false;
                settingsPanel.interactable = false;
            }
        }
        else Debug.LogError("IngameMenuController: buttonContainer or settingsPanel are null.");
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
