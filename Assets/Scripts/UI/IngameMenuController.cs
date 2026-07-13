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
    [SerializeField] private string returnToReceptionMessage = "Volver a recepcion cancelara el tratamiento y no recibiras pago por este paciente.";
    [SerializeField] private string confirmReturnLabel = "Cancelar tratamiento";
    [SerializeField] private string cancelReturnLabel = "Seguir atendiendo";

    private bool isReturnDialogConfigured;
    private bool isReturningToReceptionConfirmed;

    private void Awake()
    {
        ConfigureReturnToReceptionDialog();
        SetReturnToReceptionDialogVisible(false);
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
        return SceneManager.GetActiveScene().name == SceneNames.Camilla
            && GameSession.I != null
            && GameSession.I.CurrentCustomer != null;
    }

    private void ShowReturnToReceptionDialog()
    {
        if (returnToReceptionDialog == null)
        {
            returnToReceptionDialog = CreateReturnToReceptionDialog();

            if (returnToReceptionDialog == null)
            {
                ConfirmReturnToReception();
                return;
            }
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
        SetReturnToReceptionDialogVisible(false);
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

        TextMeshProUGUI messageText = GetOrAddText(returnToReceptionDialog.transform, "MessageText");
        if (messageText != null)
        {
            messageText.text = returnToReceptionMessage;
            messageText.fontSize = 24f;
            messageText.color = Color.black;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.textWrappingMode = TextWrappingModes.Normal;
        }

        Button confirmButton = GetOrAddButton(returnToReceptionDialog.transform, "ConfirmButton");
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(ConfirmReturnToReception);
            confirmButton.onClick.AddListener(ConfirmReturnToReception);
            SetButtonLabel(confirmButton.transform, confirmReturnLabel);
        }

        Button cancelButton = GetOrAddButton(returnToReceptionDialog.transform, "CancelButton");
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(CancelReturnToReception);
            cancelButton.onClick.AddListener(CancelReturnToReception);
            SetButtonLabel(cancelButton.transform, cancelReturnLabel);
        }

        isReturnDialogConfigured = true;
    }

    private void SetReturnToReceptionDialogVisible(bool isVisible)
    {
        if (returnToReceptionDialog != null)
            returnToReceptionDialog.SetActive(isVisible);
    }

    private TextMeshProUGUI GetOrAddText(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child == null)
            return null;

        TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
        if (text == null)
            text = child.gameObject.AddComponent<TextMeshProUGUI>();

        return text;
    }

    private Button GetOrAddButton(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child == null)
            return null;

        Button button = child.GetComponent<Button>();
        if (button == null)
            button = child.gameObject.AddComponent<Button>();

        Image image = child.GetComponent<Image>();
        if (image != null)
            button.targetGraphic = image;

        return button;
    }

    private void SetButtonLabel(Transform buttonTransform, string label)
    {
        Transform labelTransform = buttonTransform.Find("Label");
        TextMeshProUGUI labelText;

        if (labelTransform == null)
        {
            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonTransform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            labelText = labelObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            labelText = labelTransform.GetComponent<TextMeshProUGUI>();
            if (labelText == null)
                labelText = labelTransform.gameObject.AddComponent<TextMeshProUGUI>();
        }

        labelText.text = label;
        labelText.fontSize = 18f;
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.textWrappingMode = TextWrappingModes.Normal;
    }

    private GameObject CreateReturnToReceptionDialog()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas == null)
            parentCanvas = FindFirstObjectByType<Canvas>();

        if (parentCanvas == null)
        {
            Debug.LogWarning("IngameMenuController: no Canvas found for return confirmation dialog.");
            return null;
        }

        return RuntimeConfirmationDialog.Create(
            parentCanvas.transform,
            "ReturnToReceptionConfirmation",
            "Volver a recepcion cancelara el tratamiento y no recibiras pago por este paciente.",
            "Cancelar tratamiento",
            "Seguir atendiendo",
            ConfirmReturnToReception,
            CancelReturnToReception);
    }
}
