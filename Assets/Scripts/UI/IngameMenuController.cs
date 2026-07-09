using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class IngameMenuController : MonoBehaviour
{
    [Header("Scene configuration")]
    [SerializeField] private string mainMenuSceneName = SceneNames.MainMenu;
    [SerializeField] private string receptionSceneName = SceneNames.Reception;
    [SerializeField] private float fadeOutDuration = 1.5f;
    [SerializeField] private CanvasGroup sceneCanvasGroup;

    [Header("Audio settings")]
    [SerializeField] private AudioMixer mainMixer;

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
}
