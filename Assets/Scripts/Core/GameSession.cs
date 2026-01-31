using UnityEngine;

public class GameSession : MonoBehaviour
{
    [SerializeField] private GameConstantsSO constants;
    public static GameSession I { get; private set; }
    public Customer CurrentCustomer { get; private set; }
    public int Money { get; private set; }

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCustomer(Customer customer)
    {
        CurrentCustomer = customer;
    }

    private void Start()
    {
        Money = constants.StartingMoney;
    }

    public void SetStartingMoney(int amount)
    {
        Money = amount;
    }

    public void AddMoney(int delta)
    {
        Money += delta;
    }

    public void ClearCustomer()
    {
        CurrentCustomer = null;
    }
}
