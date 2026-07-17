using System.Collections.Generic;
using UnityEngine;

public static class SceneNames
{
    public const string MainMenu = "mainMenu_UI";
    public const string Reception = "ReceptionScene";
    public const string Camilla = "CamillaScene";
}

public enum TreatmentFeedbackTier
{
    NoPay,
    Bad,
    Neutral,
    Good
}

public class TreatmentConditionResult
{
    public TreatmentConditionResult(string area, string type, float correctCoverage, float wrongColorRate)
    {
        Area = area;
        Type = type;
        CorrectCoverage = Mathf.Clamp01(correctCoverage);
        WrongColorRate = Mathf.Clamp01(wrongColorRate);
    }

    public string Area { get; }
    public string Type { get; }
    public float CorrectCoverage { get; }
    public float WrongColorRate { get; }
}

public class TreatmentResultSummary
{
    public TreatmentResultSummary(
        int finalPayment,
        float finalScore,
        float correctCoverage,
        float wrongColorRate,
        float dirtyRate,
        bool reachedDailyQuota,
        List<TreatmentConditionResult> conditionResults = null
    )
    {
        FinalPayment = finalPayment;
        FinalScore = Mathf.Clamp01(finalScore);
        CorrectCoverage = Mathf.Clamp01(correctCoverage);
        WrongColorRate = Mathf.Clamp01(wrongColorRate);
        DirtyRate = Mathf.Clamp01(dirtyRate);
        ReachedDailyQuota = reachedDailyQuota;
        ConditionResults = conditionResults != null
            ? new List<TreatmentConditionResult>(conditionResults)
            : new List<TreatmentConditionResult>();
    }

    public int FinalPayment { get; }
    public float FinalScore { get; }
    public float CorrectCoverage { get; }
    public float WrongColorRate { get; }
    public float DirtyRate { get; }
    public bool ReachedDailyQuota { get; }
    public IReadOnlyList<TreatmentConditionResult> ConditionResults { get; }

    public TreatmentFeedbackTier FeedbackTier
    {
        get
        {
            if (FinalPayment <= 0)
                return TreatmentFeedbackTier.NoPay;

            if (FinalScore >= 0.75f)
                return TreatmentFeedbackTier.Good;

            if (FinalScore >= 0.45f)
                return TreatmentFeedbackTier.Neutral;

            return TreatmentFeedbackTier.Bad;
        }
    }
}

public class GameSession : MonoBehaviour
{
    [SerializeField] private GameConstantsSO constants;

    public static GameSession I { get; private set; }
    public Customer CurrentCustomer { get; private set; }
    public Customer LastTreatedCustomer { get; private set; }
    public TreatmentResultSummary LastTreatmentResult { get; private set; }
    public bool PendingDayEnd { get; private set; }
    public int Money { get; private set; }
    public int DailyQuota => constants != null ? constants.DailyQuota : 2500;
    public bool HasReachedDailyQuota => Money >= DailyQuota;
    public float DailyQuotaProgress => DailyQuota <= 0 ? 1f : Mathf.Clamp01((float)Money / DailyQuota);
    public bool HasPendingFarewell => LastTreatedCustomer != null && LastTreatmentResult != null;
    public bool HasPendingDayEnd => PendingDayEnd;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        if (constants == null)
        {
            Debug.LogError("GameConstantsSO no esta asignado.");
            Money = 0;
            return;
        }

        Money = constants.StartingMoney;
    }

    public void SetCustomer(Customer customer)
    {
        CurrentCustomer = customer;
    }

    public void SetStartingMoney(int amount)
    {
        Money = amount;
    }

    public void ResetRun()
    {
        Money = constants != null ? constants.StartingMoney : 0;
        CurrentCustomer = null;
        LastTreatedCustomer = null;
        LastTreatmentResult = null;
        PendingDayEnd = false;
    }

    public bool AddMoney(int delta)
    {
        bool hadReachedQuota = HasReachedDailyQuota;
        Money += delta;
        return !hadReachedQuota && HasReachedDailyQuota;
    }

    public void SetLastTreatmentResult(
        Customer customer,
        int finalPayment,
        float finalScore,
        float correctCoverage,
        float wrongColorRate,
        float dirtyRate,
        bool reachedDailyQuota,
        List<TreatmentConditionResult> conditionResults = null
    )
    {
        LastTreatedCustomer = customer;
        LastTreatmentResult = new TreatmentResultSummary(
            finalPayment,
            finalScore,
            correctCoverage,
            wrongColorRate,
            dirtyRate,
            reachedDailyQuota,
            conditionResults
        );

        if (reachedDailyQuota)
            PendingDayEnd = true;
    }

    public void CompletePendingFarewell()
    {
        LastTreatedCustomer = null;
        LastTreatmentResult = null;
        ClearCustomer();
    }

    public void CompletePendingDayEnd()
    {
        PendingDayEnd = false;
    }

    public void ClearCustomer()
    {
        CurrentCustomer = null;
    }
}
