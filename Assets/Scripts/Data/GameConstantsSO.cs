using UnityEngine;

[CreateAssetMenu(fileName = "GameConstantsSO", menuName = "Scriptable Objects/GameConstantsSO")]
public class GameConstantsSO : ScriptableObject
{
    public const int basePayment = 50;
    public const int baseTimeLimit = 60;
    public const float maxWaitingTime = 2.5f;
    public int StartingMoney = 100;

    public int BasePayment => basePayment;
    public int BaseTimeLimit => baseTimeLimit;
    public float MaxWaitingTime => maxWaitingTime;
}
