using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

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

    [Header("Evaluation")]
    [SerializeField] private TreatmentEvaluator evaluator;

    [Header("Scene Transition")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private string receptionSceneName = "ReceptionScene";

    private GameState currentState;

    public bool CanPaint => currentState == GameState.Running;

    private void Awake()
    {
        if (brushInput == null)
            brushInput = FindFirstObjectByType<BrushInputWorld>();

        if (evaluator == null)
            evaluator = FindFirstObjectByType<TreatmentEvaluator>();
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

        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        if (brushInput != null)
            brushInput.SetPaintingEnabled(true);

        UpdateTimerText();
    }

    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
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
        currentState = GameState.Results;

        if (brushInput != null)
            brushInput.SetPaintingEnabled(false);

        var customer = GameSession.I != null ? GameSession.I.CurrentCustomer : null;

        int finalPay = 0;
        float correctPct = 0f;
        float wrongPct = 0f;
        float dirtyPct = 0f;

        if (customer != null && evaluator != null)
        {
            var conditions = customer.Treatment.SkinConditions;
            int basePay = customer.Treatment.Payment;

            var eval = evaluator.Evaluate(conditions, basePay, remainingTime, roundDuration);

            finalPay = eval.finalPayment;
            correctPct = eval.correctCoverage * 100f;
            wrongPct = eval.wrongColorRate * 100f;
            dirtyPct = eval.dirtyRate * 100f;
        }
        else if (customer != null)
        {
            finalPay = customer.Treatment.Payment;
        }

        if (GameSession.I != null)
            GameSession.I.AddMoney(finalPay);

        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        if (resultsText != null)
        {
            int money = (GameSession.I != null) ? GameSession.I.Money : 0;

            if (customer != null && evaluator != null)
            {
                resultsText.text =
                    $"Correcto: {Mathf.RoundToInt(correctPct)}%\n" +
                    $"Color incorrecto: {Mathf.RoundToInt(wrongPct)}%\n" +
                    $"Ensuciado: {Mathf.RoundToInt(dirtyPct)}%\n\n" +
                    $"Pago: {finalPay}\n" +
                    $"Dinero: {money}";
            }
            else
            {
                resultsText.text = $"Pago: {finalPay}\nDinero: {money}";
            }
        }
    }

    // ---------------- Scene transition (Results -> Reception) ----------------

    /// <summary>
    /// Llamar desde el botón "Volver a la recepción" del panel de resultados.
    /// </summary>
    public void ReturnToReception()
    {
        // Evita doble click y estados raros.
        if (currentState != GameState.Results)
            return;

        Time.timeScale = 1f;
        StartCoroutine(TransitionToReception());
    }

    private IEnumerator TransitionToReception()
    {
        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(fadeCanvasGroup, 1f, true));
        }

        SceneManager.LoadScene(receptionSceneName);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, bool blockRaycasts)
    {
        if (canvasGroup == null)
            yield break;

        canvasGroup.blocksRaycasts = true;

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = blockRaycasts;
    }
}
