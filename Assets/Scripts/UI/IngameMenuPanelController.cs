using UnityEngine;
using UnityEngine.InputSystem;

public class IngameMenuPanelController : MonoBehaviour
{
    [Header("Menu objects")]
    [SerializeField] private CanvasGroup panelObject;
    [SerializeField] private CanvasGroup settingsPanel;
    [SerializeField] private CanvasGroup buttonContainer;
    [SerializeField] private CanvasGroup pauseOverlay;

    [Header("Audio settings")]
    [SerializeField] private AudioSource menuSoundSource;
    [SerializeField] private AudioClip menuOnSound;
    [SerializeField] private AudioClip menuOffSound;

    private bool isOpen;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.pKey.wasPressedThisFrame || keyboard.mKey.wasPressedThisFrame)
            ToggleMenu();
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;

        Time.timeScale = isOpen ? 0f : 1f;
        PlayToggleSound();

        if (isOpen)
        {
            SetPauseOverlayState(true);
            SetCanvasGroupState(panelObject, true);
            ResetSettingsView();
            return;
        }

        ResetSettingsView();
        SetCanvasGroupState(panelObject, false);
        SetPauseOverlayState(false);
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
        ResetSettingsView();
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

    private void ResetSettingsView()
    {
        if (!HasSettingsReferences())
            return;

        SetCanvasGroupState(settingsPanel, false);
        SetCanvasGroupState(buttonContainer, true);
    }

    private void SetCanvasGroupState(CanvasGroup canvasGroup, bool isVisible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.blocksRaycasts = isVisible;
        canvasGroup.interactable = isVisible;
    }

    private void SetPauseOverlayState(bool isVisible)
    {
        if (pauseOverlay == null)
            return;

        pauseOverlay.alpha = isVisible ? 1f : 0f;
        pauseOverlay.blocksRaycasts = false;
        pauseOverlay.interactable = false;
    }
}
