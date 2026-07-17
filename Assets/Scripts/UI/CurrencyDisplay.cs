using TMPro;
using UnityEngine;

public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private TMP_Text dailyQuotaText;

    [Header("Typography")]
    [SerializeField] private TMP_FontAsset currencyFont;
    [SerializeField] private TMP_FontAsset dailyQuotaFont;

    [SerializeField] private bool showDailyQuota = true;
    [SerializeField] private string moneyOnlyFormat = "$ {0}";
    [SerializeField] private string dailyQuotaFormat = "$ {0}";

    private int lastMoney = int.MinValue;
    private int lastDailyQuota = int.MinValue;

    private void Awake()
    {
        if (currencyText == null)
            currencyText = GetComponentInChildren<TMP_Text>(true);

        ApplyTypography();
    }

    private void OnValidate()
    {
        ApplyTypography();
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

        currencyText.text = string.Format(moneyOnlyFormat, money);

        if (dailyQuotaText != null)
            dailyQuotaText.text = showDailyQuota ? string.Format(dailyQuotaFormat, dailyQuota) : "";

        lastMoney = money;
        lastDailyQuota = dailyQuota;
    }

    private void ApplyTypography()
    {
        if (currencyText != null && currencyFont != null)
            currencyText.font = currencyFont;

        if (dailyQuotaText != null)
        {
            TMP_FontAsset font = dailyQuotaFont != null ? dailyQuotaFont : currencyFont;

            if (font != null)
                dailyQuotaText.font = font;
        }
    }
}
