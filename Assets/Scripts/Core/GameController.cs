using UnityEngine;
using TMPro;

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

    private GameState currentState;

    public bool CanPaint => currentState == GameState.Running;

    private void Awake()
    {
        if (brushInput == null)
            brushInput = FindFirstObjectByType<BrushInputWorld>();
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
        if (customer != null && GameSession.I != null)
            GameSession.I.AddMoney(customer.Treatment.Payment);

        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        if (resultsText != null)
            resultsText.text = $"Pago: {(customer != null ? customer.Treatment.Payment : 0)} | Dinero: {(GameSession.I != null ? GameSession.I.Money : 0)}";
    }
}
