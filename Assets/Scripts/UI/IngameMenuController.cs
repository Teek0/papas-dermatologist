using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class IngameMenuController : MonoBehaviour
{
    [Header("Scene configuration")]
    public string mainMenuSceneName = "mainMenu_UI";
    public float fadeOutDuration = 1.5f;
    public CanvasGroup sceneCanvasGroup;

    [Header("Menu objects")]
    public RectTransform buttonTransform;
    public CanvasGroup panelObject;
    public CanvasGroup settingsPanel;
    public CanvasGroup buttonContainer;

    [Header("Audio settings")]
    public mainMenu_Music musicController;
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
            //panelObject.alpha = isOpen ? 1f : 0f;
            panelObject.blocksRaycasts = isOpen;
            panelObject.interactable = isOpen;

            if (!isOpen)
            {
                ((Animator)panelObject.GetComponent(typeof(Animator))).SetTrigger("menu_open");
            } else
            {
                ((Animator)panelObject.GetComponent(typeof(Animator))).SetTrigger("menu_close");
            }
        }
    }

    public void actionMainMenu()
    {
        Time.timeScale = 1f;
        StartCoroutine(TransitionToMenu());
    }

    public void actionSettingsMini()
    {
        if (buttonContainer != null)
        {
            if (settingsPanel != null)
            {
                //buttonContainer.alpha = 0f;
                settingsPanel.alpha = 1f;

                settingsPanel.blocksRaycasts = true;
                settingsPanel.interactable = true;

                buttonContainer.blocksRaycasts = false;
                buttonContainer.interactable = false;

            }
        }
        
    }

    public void actionSettingsBack()
    {
        settingsPanel.alpha = 0f;
        //buttonContainer.alpha = 1f;

        buttonContainer.blocksRaycasts = true;
        buttonContainer.interactable = true;

        settingsPanel.blocksRaycasts = false;
        settingsPanel.interactable = false;
    }

    IEnumerator TransitionToMenu()
    {
        yield return StartCoroutine(FadeOutRoutine(sceneCanvasGroup, 0f, false));
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public IEnumerator FadeOutRoutine(CanvasGroup sceneGroup, float target, bool state)
    {
        float startAlpha = sceneGroup.alpha;
        musicController.mainMixer.GetFloat("masterMix", out float startVolume_dB);
        float startLinear = Mathf.Pow(10f, startVolume_dB / 20f);

        float elapsed = 0;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            // Fade visual
            float percentage = elapsed / fadeOutDuration;
            sceneGroup.alpha = Mathf.Lerp(startAlpha, target, percentage);

            //Fade audio
            float currentLinear = Mathf.Lerp(startLinear, 0f, percentage);
            float targetdB = Mathf.Log10(Mathf.Clamp(currentLinear, 0.0001f, 1f)) * 20f;
            musicController.mainMixer.SetFloat("masterMix", targetdB);

            yield return null;
        }

        sceneGroup.alpha = target;
        sceneGroup.blocksRaycasts = state;
        sceneGroup.interactable = state;
    }
}
