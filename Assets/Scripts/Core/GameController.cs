using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    private enum GameState
    {
        Running,
        Results
    }

    [Header("Time Settings")]
    [SerializeField] private float roundDuration = 60f;
    private float remainingTime;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsText;

    private GameState currentState;

    private void Start()
    {
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

        resultsPanel.SetActive(false);
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
        timerText.text = $"Time: {Mathf.CeilToInt(remainingTime)}";
    }

    public void EndRound()
    {
        currentState = GameState.Results;

        resultsPanel.SetActive(true);
        resultsText.text = "Score: 0"; // placeholder
    }
}
