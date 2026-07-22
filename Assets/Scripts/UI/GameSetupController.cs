using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSetupController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup setupPanel;
    [SerializeField] private bool hidePanelOnAwake = true;

    [Header("Quota Values")]
    [SerializeField] private int shortQuota = 100;
    [SerializeField] private int mediumQuota = 175;
    [SerializeField] private int longQuota = 250;

    [Header("Current Selection")]
    [SerializeField] private DailyQuotaPreset selectedQuotaPreset = DailyQuotaPreset.Medium;
    [SerializeField] private GameDifficulty selectedDifficulty = GameDifficulty.Normal;
    [SerializeField] private bool applyWhenSelectionChanges = true;

    [Header("Optional Labels")]
    [SerializeField] private TMP_Text quotaLabel;
    [SerializeField] private TMP_Text difficultyLabel;

    [Header("Optional Button State Images")]
    [SerializeField] private Sprite buttonOnSprite;
    [SerializeField] private Sprite buttonOffSprite;
    [SerializeField] private Image shortQuotaButtonImage;
    [SerializeField] private Image mediumQuotaButtonImage;
    [SerializeField] private Image longQuotaButtonImage;
    [SerializeField] private Image normalDifficultyButtonImage;
    [SerializeField] private Image hardDifficultyButtonImage;

    [Header("Reset Confirmation")]
    [SerializeField] private CanvasGroup resetConfirmationPanel;
    [SerializeField] private Button confirmResetButton;
    [SerializeField] private Button cancelResetButton;

    private DailyQuotaPreset pendingQuotaPreset;
    private GameDifficulty pendingDifficulty;
    private bool hasPendingSelection;

    private void Awake()
    {
        if (setupPanel == null)
            setupPanel = GetComponent<CanvasGroup>();

        WireResetConfirmationButtons();

        if (RunConfigurationStore.HasConfiguration)
            LoadFromConfiguration(RunConfigurationStore.Current);
        else
            ApplySelection();

        UpdateLabels();

        if (hidePanelOnAwake)
            ClosePanel();

        CloseResetConfirmation();
    }

    private void OnValidate()
    {
        UpdateButtonStateImages();
    }

    public void OpenPanel()
    {
        SetPanelVisible(true);
    }

    public void ClosePanel()
    {
        SetPanelVisible(false);
        CancelPendingConfigurationChange();
    }

    public void TogglePanel()
    {
        SetPanelVisible(!IsPanelVisible());
    }

    public void SelectShortQuota()
    {
        SelectQuota(DailyQuotaPreset.Short);
    }

    public void SelectMediumQuota()
    {
        SelectQuota(DailyQuotaPreset.Medium);
    }

    public void SelectLongQuota()
    {
        SelectQuota(DailyQuotaPreset.Long);
    }

    public void SelectNormalDifficulty()
    {
        SelectDifficulty(GameDifficulty.Normal);
    }

    public void SelectHardDifficulty()
    {
        SelectDifficulty(GameDifficulty.Hard);
    }

    public void ToggleDifficulty()
    {
        SelectDifficulty(selectedDifficulty == GameDifficulty.Hard
            ? GameDifficulty.Normal
            : GameDifficulty.Hard);
    }

    public void ApplyAndClose()
    {
        ApplySelection();
        ClosePanel();
    }

    public void ConfirmPendingConfigurationChange()
    {
        if (!hasPendingSelection)
        {
            CloseResetConfirmation();
            return;
        }

        selectedQuotaPreset = pendingQuotaPreset;
        selectedDifficulty = pendingDifficulty;
        hasPendingSelection = false;

        ApplySelection();

        if (GameSession.I != null)
            GameSession.I.ResetRun();

        CloseResetConfirmation();
    }

    public void CancelPendingConfigurationChange()
    {
        hasPendingSelection = false;
        CloseResetConfirmation();
        UpdateLabels();
    }

    public void ApplySelection()
    {
        var configuration = new RunConfiguration(
            selectedQuotaPreset,
            GetQuotaValue(selectedQuotaPreset),
            selectedDifficulty
        );

        RunConfigurationStore.Set(configuration);

        if (GameSession.I != null)
            GameSession.I.SetRunConfiguration(configuration);

        UpdateLabels();
    }

    private void SelectQuota(DailyQuotaPreset quotaPreset)
    {
        if (quotaPreset == selectedQuotaPreset)
            return;

        if (ShouldConfirmRunReset())
        {
            QueuePendingSelection(quotaPreset, selectedDifficulty);
            return;
        }

        selectedQuotaPreset = quotaPreset;
        OnSelectionChanged();
    }

    private void SelectDifficulty(GameDifficulty difficulty)
    {
        if (difficulty == selectedDifficulty)
            return;

        if (ShouldConfirmRunReset())
        {
            QueuePendingSelection(selectedQuotaPreset, difficulty);
            return;
        }

        selectedDifficulty = difficulty;
        OnSelectionChanged();
    }

    private void OnSelectionChanged()
    {
        if (applyWhenSelectionChanges)
            ApplySelection();
        else
            UpdateLabels();
    }

    private void LoadFromConfiguration(RunConfiguration configuration)
    {
        selectedQuotaPreset = configuration.QuotaPreset;
        selectedDifficulty = configuration.Difficulty;
    }

    private bool ShouldConfirmRunReset()
    {
        return GameSession.I != null && GameSession.I.HasRunProgress;
    }

    private void QueuePendingSelection(DailyQuotaPreset quotaPreset, GameDifficulty difficulty)
    {
        pendingQuotaPreset = quotaPreset;
        pendingDifficulty = difficulty;
        hasPendingSelection = true;
        OpenResetConfirmation();
    }

    private int GetQuotaValue(DailyQuotaPreset quotaPreset)
    {
        return quotaPreset switch
        {
            DailyQuotaPreset.Short => shortQuota,
            DailyQuotaPreset.Long => longQuota,
            _ => mediumQuota
        };
    }

    private void UpdateLabels()
    {
        if (quotaLabel != null)
            quotaLabel.text = $"Cuota: {QuotaName(selectedQuotaPreset)} (${GetQuotaValue(selectedQuotaPreset)})";

        if (difficultyLabel != null)
            difficultyLabel.text = selectedDifficulty == GameDifficulty.Hard
                ? "Dificultad: Difícil"
                : "Dificultad: Normal";

        UpdateButtonStateImages();
    }

    private void UpdateButtonStateImages()
    {
        SetButtonState(shortQuotaButtonImage, selectedQuotaPreset == DailyQuotaPreset.Short);
        SetButtonState(mediumQuotaButtonImage, selectedQuotaPreset == DailyQuotaPreset.Medium);
        SetButtonState(longQuotaButtonImage, selectedQuotaPreset == DailyQuotaPreset.Long);
        SetButtonState(normalDifficultyButtonImage, selectedDifficulty == GameDifficulty.Normal);
        SetButtonState(hardDifficultyButtonImage, selectedDifficulty == GameDifficulty.Hard);
    }

    private void SetButtonState(Image targetImage, bool isSelected)
    {
        if (targetImage == null)
            return;

        Sprite targetSprite = isSelected ? buttonOnSprite : buttonOffSprite;
        if (targetSprite != null)
            targetImage.sprite = targetSprite;
    }

    private void WireResetConfirmationButtons()
    {
        if (confirmResetButton != null)
        {
            confirmResetButton.onClick.RemoveListener(ConfirmPendingConfigurationChange);
            confirmResetButton.onClick.AddListener(ConfirmPendingConfigurationChange);
        }

        if (cancelResetButton != null)
        {
            cancelResetButton.onClick.RemoveListener(CancelPendingConfigurationChange);
            cancelResetButton.onClick.AddListener(CancelPendingConfigurationChange);
        }
    }

    private void OpenResetConfirmation()
    {
        SetCanvasGroupVisible(resetConfirmationPanel, true);
    }

    private void CloseResetConfirmation()
    {
        SetCanvasGroupVisible(resetConfirmationPanel, false);
    }

    private string QuotaName(DailyQuotaPreset quotaPreset)
    {
        return quotaPreset switch
        {
            DailyQuotaPreset.Short => "Corta",
            DailyQuotaPreset.Long => "Larga",
            _ => "Media"
        };
    }

    private bool IsPanelVisible()
    {
        return setupPanel != null && setupPanel.alpha > 0.5f;
    }

    private void SetPanelVisible(bool isVisible)
    {
        if (setupPanel == null)
            return;

        setupPanel.alpha = isVisible ? 1f : 0f;
        setupPanel.interactable = isVisible;
        setupPanel.blocksRaycasts = isVisible;
    }

    private void SetCanvasGroupVisible(CanvasGroup targetCanvasGroup, bool isVisible)
    {
        if (targetCanvasGroup == null)
            return;

        targetCanvasGroup.alpha = isVisible ? 1f : 0f;
        targetCanvasGroup.interactable = isVisible;
        targetCanvasGroup.blocksRaycasts = isVisible;
    }
}
