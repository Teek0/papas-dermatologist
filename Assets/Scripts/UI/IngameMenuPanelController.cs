using UnityEngine;

public class IngameMenuPanelController : MonoBehaviour
{
    [Header("Menu objects")]
    [SerializeField] private CanvasGroup panelObject;
    [SerializeField] private CanvasGroup settingsPanel;
    [SerializeField] private CanvasGroup buttonContainer;

    [Header("Audio settings")]
    [SerializeField] private AudioSource menuSoundSource;
    [SerializeField] private AudioClip menuOnSound;
    [SerializeField] private AudioClip menuOffSound;

    private bool isOpen;

    public void ToggleMenu()
    {
        isOpen = !isOpen;

        Time.timeScale = isOpen ? 0f : 1f;
        PlayToggleSound();

        if (isOpen)
            CloseSettingsPanel();

        SetCanvasGroupState(panelObject, isOpen);
    }

    public void OpenSettingsPanel()
    {
        if (!HasSettingsReferences())
            return;

        SetCanvasGroupState(buttonContainer, false);
        SetCanvasGroupState(settingsPanel, true);
    }

    public void CloseSettingsPanel()
    {
        if (!HasSettingsReferences())
            return;

        SetCanvasGroupState(settingsPanel, false);
        SetCanvasGroupState(buttonContainer, true);
    }

    private void PlayToggleSound()
    {
        if (menuSoundSource == null)
            return;

        AudioClip clipToPlay = isOpen ? menuOnSound : menuOffSound;
        if (clipToPlay != null)
            menuSoundSource.PlayOneShot(clipToPlay);
    }

    private bool HasSettingsReferences()
    {
        if (buttonContainer != null && settingsPanel != null)
            return true;

        Debug.LogError("IngameMenuPanelController: buttonContainer or settingsPanel are null.");
        return false;
    }

    private void SetCanvasGroupState(CanvasGroup canvasGroup, bool isVisible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.blocksRaycasts = isVisible;
        canvasGroup.interactable = isVisible;
    }
}
