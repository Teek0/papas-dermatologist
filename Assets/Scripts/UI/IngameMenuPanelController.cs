using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

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

    [Header("UI feedback sounds")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip sliderClickSound;
    [SerializeField, Range(0f, 1f)] private float feedbackVolume = 1f;

    private bool isOpen;
    private Transform feedbackRoot;
    private Selectable hoveredSelectable;
    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    private void Awake()
    {
        CloseMenuSilently();
    }

    private void OnDisable()
    {
        if (isOpen)
            Time.timeScale = 1f;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.pKey.wasPressedThisFrame || keyboard.mKey.wasPressedThisFrame))
            ToggleMenu();

        UpdatePointerFeedback();
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

    private void CloseMenuSilently()
    {
        isOpen = false;
        hoveredSelectable = null;
        Time.timeScale = 1f;

        SetCanvasGroupState(settingsPanel, false);
        SetCanvasGroupState(buttonContainer, true);
        SetCanvasGroupState(panelObject, false);
        SetPauseOverlayState(false);
    }

    private bool ButtonHasPersistentAudio(Button button)
    {
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) is AudioSource &&
                button.onClick.GetPersistentMethodName(i) == nameof(AudioSource.PlayOneShot))
            {
                return true;
            }
        }

        return false;
    }

    private bool ButtonCallsMethod(Button button, string methodName)
    {
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentMethodName(i) == methodName)
                return true;
        }

        return false;
    }

    private void PlayFeedbackSound(AudioClip clip)
    {
        if (menuSoundSource == null || clip == null)
            return;

        menuSoundSource.PlayOneShot(clip, feedbackVolume);
    }

    private void UpdatePointerFeedback()
    {
        Selectable currentHoverSelectable = GetSelectableUnderPointer(true);

        if (currentHoverSelectable != hoveredSelectable)
        {
            hoveredSelectable = currentHoverSelectable;

            if (hoveredSelectable != null)
                PlayFeedbackSound(hoverSound);
        }

        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return;

        Selectable currentClickSelectable = GetSelectableUnderPointer(false);
        if (currentClickSelectable == null)
            return;

        if (currentClickSelectable is Slider)
        {
            PlayFeedbackSound(sliderClickSound != null ? sliderClickSound : clickSound);
            return;
        }

        if (currentClickSelectable is Button button && !ButtonHasPersistentAudio(button) && !ButtonCallsMethod(button, nameof(ToggleMenu)))
            PlayFeedbackSound(clickSound);
    }

    private Selectable GetSelectableUnderPointer(bool isHoverCheck)
    {
        Mouse mouse = Mouse.current;
        EventSystem eventSystem = EventSystem.current;
        Transform root = GetFeedbackRoot();

        if (mouse == null || eventSystem == null || root == null)
            return null;

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = mouse.position.ReadValue()
        };

        raycastResults.Clear();
        eventSystem.RaycastAll(pointerData, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            if (result.gameObject == null || !result.gameObject.transform.IsChildOf(root))
                continue;

            Selectable selectable = result.gameObject.GetComponentInParent<Selectable>();
            if (selectable == null || !selectable.interactable)
                continue;

            if (isHoverCheck && ShouldSkipHoverFeedback(selectable))
                continue;

            if (selectable != null && selectable.interactable)
                return selectable;
        }

        return null;
    }

    private bool ShouldSkipHoverFeedback(Selectable selectable)
    {
        if (selectable is Button button && ButtonCallsMethod(button, nameof(ToggleMenu)))
            return true;

        if (selectable is Slider)
            return true;

        return false;
    }

    private Transform GetFeedbackRoot()
    {
        if (feedbackRoot == null)
            feedbackRoot = transform.parent != null ? transform.parent : transform;

        return feedbackRoot;
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
        pauseOverlay.blocksRaycasts = isVisible;
        pauseOverlay.interactable = false;
    }
}
