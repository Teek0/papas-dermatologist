using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class IngameMenuController : MonoBehaviour
{
    [Header("Scene configuration")]
    [SerializeField] private string mainMenuSceneName = SceneNames.MainMenu;
    [SerializeField] private string receptionSceneName = SceneNames.Reception;
    [SerializeField] private float fadeOutDuration = 1.5f;
    [SerializeField] private CanvasGroup sceneCanvasGroup;

    [Header("Audio settings")]
    [SerializeField] private AudioMixer mainMixer;

    private GameObject returnToReceptionDialog;
    private bool isReturningToReceptionConfirmed;

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

        returnToReceptionDialog.SetActive(true);
        returnToReceptionDialog.transform.SetAsLastSibling();
    }

    private void CancelReturnToReception()
    {
        if (returnToReceptionDialog != null)
            returnToReceptionDialog.SetActive(false);
    }

    private void ConfirmReturnToReception()
    {
        isReturningToReceptionConfirmed = true;
        CancelReturnToReception();
        GoToReception();
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
