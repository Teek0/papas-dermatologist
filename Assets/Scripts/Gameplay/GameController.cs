using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class GameController : MonoBehaviour
{
    private enum GameState
    {
        Running,
        Results
    }

    [Header("Input")]
    [SerializeField] private BrushInputWorld brushInput;

    [Header("Time Settings")]
    [SerializeField] private float roundDuration = 60f;
    private float remainingTime;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsText;
    private CanvasGroup resultsPanelCanvasGroup;

    [Header("Evaluation")]
    [SerializeField] private TreatmentEvaluator evaluator;

    [Header("Scene Transition")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private string receptionSceneName = SceneNames.Reception;

    [Header("Round End Audio")]
    [SerializeField] private AudioMixerGroup roundEndOutputGroup;
    [SerializeField] private AudioClip timeUpClip;
    [SerializeField] private AudioClip timerWarningClip;
    [SerializeField] private float timerWarningThreshold = 5f;
    [SerializeField, Range(0f, 2f)] private float timerWarningVolume = 1f;

    private GameState currentState;
    private AudioSource roundEndAudioSource;
    private AudioSource timerWarningAudioSource;
    private bool timerWarningPlayed;

    public bool CanPaint => currentState == GameState.Running && Time.timeScale > 0f;

    private void Awake()
    {
        if (brushInput == null)
            brushInput = FindFirstObjectByType<BrushInputWorld>();

        if (evaluator == null)
            evaluator = FindFirstObjectByType<TreatmentEvaluator>();

        if (timeUpClip == null)
            timeUpClip = Resources.Load<AudioClip>("One Shots/finishround2");

        if (timerWarningClip == null)
            timerWarningClip = Resources.Load<AudioClip>("One Shots/doubletime loop");

        if (resultsPanel != null)
            resultsPanelCanvasGroup = resultsPanel.GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        var customer = GameSession.I != null ? GameSession.I.CurrentCustomer : null;
        if (customer != null)
            roundDuration = customer.Treatment.TimeLimit;

        StartRound();
    }

    private void Update()
    {
        if (currentState != GameState.Running)
            return;

        UpdateTimer();
    }

    private void StartRound()
    {
        remainingTime = roundDuration;
        currentState = GameState.Running;
        timerWarningPlayed = false;

        SetResultsPanelVisible(false);

        if (brushInput != null)
            brushInput.SetPaintingEnabled(true);

        UpdateTimerText();
    }

    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;

        if (!timerWarningPlayed && remainingTime > 0f && remainingTime <= timerWarningThreshold)
        {
            timerWarningPlayed = true;
            PlayTimerWarningSound();
        }

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimerText();
            StopTimerWarningSound();
            PlayTimeUpSound();
            EndRound();
            return;
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(remainingTime)}";
    }

    public void EndRound()
    {
        if (currentState == GameState.Results)
            return;

        currentState = GameState.Results;
        StopTimerWarningSound();

        if (brushInput != null)
            brushInput.SetPaintingEnabled(false);

        var customer = GameSession.I != null ? GameSession.I.CurrentCustomer : null;

        int finalPay = 0;
        float correctPct = 0f;
        float wrongPct = 0f;
        float dirtyPct = 0f;
        float finalScore = 0f;
        bool reachedDailyQuota = false;
        List<TreatmentConditionResult> conditionResults = null;

        if (customer != null && evaluator != null)
        {
            var conditions = customer.Treatment.SkinConditions;
            int basePay = customer.Treatment.Payment;

            var eval = evaluator.Evaluate(conditions, basePay, remainingTime, roundDuration);

            finalPay = eval.finalPayment;
            correctPct = eval.correctCoverage * 100f;
            wrongPct = eval.wrongColorRate * 100f;
            dirtyPct = eval.dirtyRate * 100f;
            finalScore = eval.finalScore;
            conditionResults = eval.conditionResults;
        }
        else if (customer != null)
        {
            finalPay = customer.Treatment.Payment;
            finalScore = finalPay > 0 ? 1f : 0f;
        }

        if (GameSession.I != null)
        {
            reachedDailyQuota = GameSession.I.AddMoney(finalPay);

            if (customer != null)
            {
                GameSession.I.SetLastTreatmentResult(
                    customer,
                    finalPay,
                    finalScore,
                    correctPct / 100f,
                    wrongPct / 100f,
                    dirtyPct / 100f,
                    reachedDailyQuota,
                    conditionResults
                );
            }
        }

        SetResultsPanelVisible(true);

        if (resultsText != null)
        {
            int money = (GameSession.I != null) ? GameSession.I.Money : 0;

            if (customer != null && evaluator != null)
            {
                resultsText.text =
                    $"Correcto: {Mathf.RoundToInt(correctPct)}%\n" +
                    $"Color incorrecto: {Mathf.RoundToInt(wrongPct)}%\n" +
                    $"Pago: {finalPay}\n" +
                    $"Dinero: {money}";
            }
            else
            {
                resultsText.text = $"Pago: {finalPay}\nDinero: {money}";
            }
        }
    }

    private void SetResultsPanelVisible(bool isVisible)
    {
        if (resultsPanel == null)
            return;

        resultsPanel.SetActive(isVisible);

        if (resultsPanelCanvasGroup == null)
            resultsPanelCanvasGroup = resultsPanel.GetComponent<CanvasGroup>();

        if (resultsPanelCanvasGroup == null)
            return;

        resultsPanelCanvasGroup.alpha = isVisible ? 1f : 0f;
        resultsPanelCanvasGroup.interactable = isVisible;
        resultsPanelCanvasGroup.blocksRaycasts = isVisible;
    }

    private void PlayTimeUpSound()
    {
        if (timeUpClip == null)
            return;

        if (roundEndAudioSource == null)
        {
            roundEndAudioSource = gameObject.AddComponent<AudioSource>();
            roundEndAudioSource.playOnAwake = false;
            roundEndAudioSource.spatialBlend = 0f;
        }

        roundEndAudioSource.outputAudioMixerGroup = roundEndOutputGroup;
        roundEndAudioSource.PlayOneShot(timeUpClip);
    }

    private void PlayTimerWarningSound()
    {
        if (timerWarningClip == null)
            return;

        if (timerWarningAudioSource == null)
        {
            timerWarningAudioSource = gameObject.AddComponent<AudioSource>();
            timerWarningAudioSource.playOnAwake = false;
            timerWarningAudioSource.spatialBlend = 0f;
            timerWarningAudioSource.loop = true;
        }

        timerWarningAudioSource.outputAudioMixerGroup = roundEndOutputGroup;
        timerWarningAudioSource.clip = timerWarningClip;
        timerWarningAudioSource.volume = timerWarningVolume;
        timerWarningAudioSource.Play();
    }

    private void StopTimerWarningSound()
    {
        if (timerWarningAudioSource == null || !timerWarningAudioSource.isPlaying)
            return;

        timerWarningAudioSource.Stop();
        timerWarningAudioSource.clip = null;
    }

    // ---------------- Scene transition (Results -> Reception) ----------------

    public void ReturnToReception()
    {
        if (currentState != GameState.Results)
            return;

        Time.timeScale = 1f;
        StartCoroutine(TransitionToReception());
    }

    private IEnumerator TransitionToReception()
    {
        yield return StartCoroutine(SceneTransitionService.FadeOutAndLoadScene(
            receptionSceneName,
            fadeCanvasGroup,
            1f,
            true,
            mainMixer,
            fadeOutDuration));
    }
}
