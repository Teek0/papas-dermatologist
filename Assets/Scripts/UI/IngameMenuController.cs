using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IngameMenuController : MonoBehaviour
{
    [Header("Scene configuration")]
    [SerializeField] private string mainMenuSceneName = SceneNames.MainMenu;
    [SerializeField] private string receptionSceneName = SceneNames.Reception;
    [SerializeField] private float fadeOutDuration = 1.5f;
    [SerializeField] private CanvasGroup sceneCanvasGroup;

    [Header("Audio settings")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Return confirmation")]
    [SerializeField] private GameObject returnToReceptionDialog;
    [SerializeField] private Button confirmReturnButton;
    [SerializeField] private Button cancelReturnButton;
    [SerializeField] private UIDragHandle returnDialogDragHandle;

    private IngameMenuPanelController menuPanelController;
    private bool isReturnDialogConfigured;
    private bool isReturningToReceptionConfirmed;

    private void Awake()
    {
        ResolveReturnToReceptionDialog();
        ConfigureReturnToReceptionDialog();
        SubscribeToMenuPanel();
        SetReturnToReceptionDialogVisible(false);
    }

    private void OnDestroy()
    {
        if (menuPanelController != null)
            menuPanelController.MenuVisibilityChanged -= HandleMenuVisibilityChanged;
    }

    public void GoToMainMenu()
    {
        GoToScene(mainMenuSceneName);
    }

    public void GoToReception()
    {
        if (!isReturningToReceptionConfirmed && ShouldConfirmReturnToReception())
        {
            ShowReturnToReceptionDialog();
            return;
        }

        isReturningToReceptionConfirmed = false;

        if (GameSession.I != null)
            GameSession.I.ClearCustomer();

        GoToScene(receptionSceneName);
    }

    private void GoToScene(string sceneName)
    {
        Time.timeScale = 1f;
        StartCoroutine(TransitionToScene(sceneName));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        yield return StartCoroutine(SceneTransitionService.FadeOutAndLoadScene(
            sceneName,
            sceneCanvasGroup,
            mainMixer,
            fadeOutDuration));
    }

    private bool ShouldConfirmReturnToReception()
    {
        return SceneManager.GetActiveScene().name == SceneNames.Camilla;
    }

    private void ShowReturnToReceptionDialog()
    {
        ResolveReturnToReceptionDialog();

        if (returnToReceptionDialog == null)
        {
            Debug.LogWarning("IngameMenuController: returnToReceptionDialog is null. Cannot show confirmation dialog.");
            return;
        }
        else
        {
            ConfigureReturnToReceptionDialog();
        }

        SetReturnToReceptionDialogVisible(true);
        returnToReceptionDialog.transform.SetAsLastSibling();
    }

    private void CancelReturnToReception()
    {
        CloseReturnToReceptionDialog(false);
    }

    private void ContinueTreatment()
    {
        CloseReturnToReceptionDialog(true);
    }

    private void CloseReturnToReceptionDialog(bool closeMenu)
    {
        SetReturnToReceptionDialogVisible(false);

        if (closeMenu)
            menuPanelController?.CloseMenu();
    }

    private void HandleMenuVisibilityChanged(bool isMenuOpen)
    {
        if (!isMenuOpen)
            CancelReturnToReception();
    }

    private void ConfirmReturnToReception()
    {
        isReturningToReceptionConfirmed = true;
        CancelReturnToReception();
        GoToReception();
    }

    private void ConfigureReturnToReceptionDialog()
    {
        if (returnToReceptionDialog == null || isReturnDialogConfigured)
            return;

        DisableTextRaycastsInDialog();
        ConfigureReturnDialogDragHandle();

        if (confirmReturnButton == null)
            confirmReturnButton = FindOrAddButtonInDialog("ConfirmButton");

        if (confirmReturnButton != null)
        {
            confirmReturnButton.onClick.RemoveListener(ConfirmReturnToReception);
            confirmReturnButton.onClick.AddListener(ConfirmReturnToReception);
        }

        if (cancelReturnButton == null)
            cancelReturnButton = FindOrAddButtonInDialog("CancelButton");

        if (cancelReturnButton != null)
        {
            cancelReturnButton.onClick.RemoveListener(CancelReturnToReception);
            cancelReturnButton.onClick.RemoveListener(ContinueTreatment);
            cancelReturnButton.onClick.AddListener(ContinueTreatment);
        }

        isReturnDialogConfigured = true;
    }

    private void ResolveReturnToReceptionDialog()
    {
        if (returnToReceptionDialog != null)
            return;

        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == "ConfirmReturnDialog")
            {
                returnToReceptionDialog = child.gameObject;
                return;
            }
        }
    }

    private void SubscribeToMenuPanel()
    {
        if (menuPanelController != null)
            return;

        menuPanelController = GetComponent<IngameMenuPanelController>();

        if (menuPanelController == null)
            menuPanelController = GetComponentInParent<IngameMenuPanelController>();

        if (menuPanelController == null)
            menuPanelController = GetComponentInChildren<IngameMenuPanelController>(true);

        if (menuPanelController == null)
            return;

        menuPanelController.MenuVisibilityChanged += HandleMenuVisibilityChanged;
    }

    private void SetReturnToReceptionDialogVisible(bool isVisible)
    {
        if (returnToReceptionDialog != null)
            returnToReceptionDialog.SetActive(isVisible);
    }

    private void ConfigureReturnDialogDragHandle()
    {
        if (returnDialogDragHandle == null)
            returnDialogDragHandle = FindOrAddDragHandleInDialog("selectArea");

        if (returnDialogDragHandle == null)
            return;

        RectTransform dialogRect = returnToReceptionDialog.GetComponent<RectTransform>();
        if (dialogRect != null)
            returnDialogDragHandle.Configure(dialogRect);

        returnDialogDragHandle.transform.SetAsLastSibling();
    }

    private void DisableTextRaycastsInDialog()
    {
        TextMeshProUGUI[] texts = returnToReceptionDialog.GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI text in texts)
            text.raycastTarget = false;
    }

    private Button FindOrAddButtonInDialog(string name)
    {
        Transform[] children = returnToReceptionDialog.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name != name)
                continue;

            Button button = child.GetComponent<Button>();
            if (button == null)
                button = child.gameObject.AddComponent<Button>();

            Image image = child.GetComponent<Image>();
            if (image != null)
                button.targetGraphic = image;

            return button;
        }

        return null;
    }

    private UIDragHandle FindOrAddDragHandleInDialog(string name)
    {
        Transform[] children = returnToReceptionDialog.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (!string.Equals(child.name, name, System.StringComparison.OrdinalIgnoreCase))
                continue;

            UIDragHandle dragHandle = child.GetComponent<UIDragHandle>();
            if (dragHandle == null)
                dragHandle = child.gameObject.AddComponent<UIDragHandle>();

            return dragHandle;
        }

        return null;
    }
}
