using UnityEngine;

[CreateAssetMenu(fileName = "GameConstantsSO", menuName = "Scriptable Objects/GameConstantsSO")]
public class GameConstantsSO : ScriptableObject
{
    public int basePayment = 10;
    public int baseTimeLimit = 10;
    public const float maxWaitingTime = 5;
    public int StartingMoney = 0;
    public int dailyQuota = 2500;

    public int BasePayment => basePayment;
    public int BaseTimeLimit => baseTimeLimit;
    public float MaxWaitingTime => maxWaitingTime;
    public int DailyQuota => dailyQuota;
}
