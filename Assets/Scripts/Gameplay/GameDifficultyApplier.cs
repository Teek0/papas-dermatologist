using UnityEngine;

public class GameDifficultyApplier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TreatmentEvaluator evaluator;
    [SerializeField] private GameObject edgesRoot;

    [Header("Behaviour")]
    [SerializeField] private bool applyOnStart = true;

    private void Awake()
    {
        if (evaluator == null)
            evaluator = FindFirstObjectByType<TreatmentEvaluator>();
    }

    private void Start()
    {
        if (applyOnStart)
            ApplyCurrentDifficulty();
    }

    public void ApplyCurrentDifficulty()
    {
        bool isHardMode = GameSession.I != null
            ? GameSession.I.IsHardMode
            : RunConfigurationStore.Current.IsHardMode;

        if (evaluator != null)
            evaluator.SetPaintOutsidePenaltyMode(isHardMode);

        if (edgesRoot != null)
            edgesRoot.SetActive(!isHardMode);
    }
}
