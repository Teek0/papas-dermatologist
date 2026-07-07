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
        if(constants==null)
        { 
            Debug.LogError("GameConstantsSO no está asignado."); 
            Money=0; 
            return; 
        }
    }

    public void SetCustomer(Customer customer)
    {
        CurrentCustomer = customer;
    }

    private void Start()
    {
        if (constants == null)
        {
            Money = 0;
            return;
        }

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
