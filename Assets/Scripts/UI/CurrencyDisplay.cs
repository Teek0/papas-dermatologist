using TMPro;
using UnityEngine;

public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private bool showDailyQuota = true;
    [SerializeField] private string moneyOnlyFormat = "$ {0}";
    [SerializeField] private string moneyWithQuotaFormat = "$ {0} / Meta $ {1}";

    private int lastMoney = int.MinValue;
    private int lastDailyQuota = int.MinValue;

    private void Awake()
    {
        if (currencyText == null)
            currencyText = GetComponentInChildren<TMP_Text>(true);
    }

    private void OnEnable()
    {
        Refresh(force: true);
    }

    private void Update()
    {
        Refresh(force: false);
    }

    private void Refresh(bool force)
    {
        if (currencyText == null || GameSession.I == null)
            return;

        int money = GameSession.I.Money;
        int dailyQuota = GameSession.I.DailyQuota;

        if (!force && money == lastMoney && dailyQuota == lastDailyQuota)
            return;

        currencyText.text = showDailyQuota
            ? string.Format(moneyWithQuotaFormat, money, dailyQuota)
            : string.Format(moneyOnlyFormat, money);

        lastMoney = money;
        lastDailyQuota = dailyQuota;
    }
}
