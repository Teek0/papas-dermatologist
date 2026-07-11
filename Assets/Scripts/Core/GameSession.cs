using UnityEngine;

public static class SceneNames
{
    public const string MainMenu = "mainMenu_UI";
    public const string Reception = "ReceptionScene";
    public const string Camilla = "CamillaScene";
}

public class GameSession : MonoBehaviour
{
    [SerializeField] private GameConstantsSO constants;

    public static GameSession I { get; private set; }
    public Customer CurrentCustomer { get; private set; }
    public int Money { get; private set; }
    public int DailyQuota => constants != null ? constants.DailyQuota : 2500;
    public bool HasReachedDailyQuota => Money >= DailyQuota;
    public float DailyQuotaProgress => DailyQuota <= 0 ? 1f : Mathf.Clamp01((float)Money / DailyQuota);

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

    public bool AddMoney(int delta)
    {
        bool hadReachedQuota = HasReachedDailyQuota;
        Money += delta;
        return !hadReachedQuota && HasReachedDailyQuota;
    }

    public void ClearCustomer()
    {
        CurrentCustomer = null;
    }
}
