using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    private const string HasSeenTutorialKey = "hasSeenTutorial";

    [Header("References")]
    [SerializeField] private CanvasGroup tutorialPanel;
    [SerializeField] private GameObject slide1;
    [SerializeField] private GameObject slide2;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private UIController uiController;

    private int currentSlide;
    private bool shouldStartGameWhenFinished;

    private void Awake()
    {
        ShowSlide(0);
        SetPanelVisible(false);
    }

    public bool ShouldShowBeforeStart()
    {
        return PlayerPrefs.GetInt(HasSeenTutorialKey, 0) == 0;
    }

    public void OpenForStart()
    {
        shouldStartGameWhenFinished = true;
        ShowSlide(0);
        SetPanelVisible(true);
    }

    public void OpenManual()
    {
        shouldStartGameWhenFinished = false;
        ShowSlide(0);
        SetPanelVisible(true);
    }

    public void NextSlide()
    {
        ShowSlide(currentSlide + 1);
    }

    public void PreviousSlide()
    {
        ShowSlide(currentSlide - 1);
    }

    public void FinishTutorial()
    {
        PlayerPrefs.SetInt(HasSeenTutorialKey, 1);
        PlayerPrefs.Save();

        SetPanelVisible(false);

        if (uiController != null)
            uiController.StartGameDirect();
    }

    public void CloseTutorial()
    {
        shouldStartGameWhenFinished = false;
        SetPanelVisible(false);
    }

    [ContextMenu("Reset Tutorial Progress")]
    public void ResetTutorialProgress()
    {
        PlayerPrefs.DeleteKey(HasSeenTutorialKey);
        PlayerPrefs.Save();
    }

    private void ShowSlide(int slideIndex)
    {
        currentSlide = Mathf.Clamp(slideIndex, 0, 1);

        if (slide1 != null)
            slide1.SetActive(currentSlide == 0);

        if (slide2 != null)
            slide2.SetActive(currentSlide == 1);

        if (previousButton != null)
            previousButton.gameObject.SetActive(currentSlide == 1);

        if (nextButton != null)
            nextButton.gameObject.SetActive(currentSlide == 0);

        if (startButton != null)
            startButton.gameObject.SetActive(currentSlide == 1);

        if (closeButton != null)
            closeButton.gameObject.SetActive(!shouldStartGameWhenFinished);
    }

    private void SetPanelVisible(bool isVisible)
    {
        if (tutorialPanel == null)
            return;

        tutorialPanel.alpha = isVisible ? 1f : 0f;
        tutorialPanel.interactable = isVisible;
        tutorialPanel.blocksRaycasts = isVisible;
    }
}