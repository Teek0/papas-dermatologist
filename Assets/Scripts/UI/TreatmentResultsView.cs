using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TreatmentResultsView
{
    private const string CorrectStarsRootName = "StarsCorrectTreatment";
    private const string ProductStarsRootName = "StarsProductControl";
    private const string LabelsTextName = "Labels";
    private const string PayTextName = "Wins (TMP)";
    private const string LossesTextName = "Losses (TMP)";
    private const string FinalPayTextName = "FWins (TMP)";
    private const string TotalTextName = "Total (TMP)";

    [SerializeField] private Transform correctTreatmentStarsRoot;
    [SerializeField] private Transform productControlStarsRoot;
    [SerializeField] private TMP_Text labelsText;
    [SerializeField] private TMP_Text payText;
    [SerializeField] private TMP_Text lossesText;
    [SerializeField] private TMP_Text finalPayText;
    [SerializeField] private TMP_Text totalText;

    private Sprite star0Sprite;
    private Sprite star50Sprite;
    private Sprite star100Sprite;
    private TMP_Text legacyResultsText;

    public struct ResultData
    {
        public float treatmentQuality;
        public float productControl;
        public float correctCoverage;
        public float wrongColorRate;
        public float dirtyRate;
        public int potentialPay;
        public int discount;
        public int finalPay;
        public int totalMoney;
    }

    public void Initialize(GameObject resultsPanel, TMP_Text legacyText, Sprite star0, Sprite star50, Sprite star100)
    {
        legacyResultsText = legacyText;
        star0Sprite = star0;
        star50Sprite = star50;
        star100Sprite = star100;

        if (resultsPanel == null)
            return;

        Transform root = resultsPanel.transform;

        correctTreatmentStarsRoot = ResolveTransform(correctTreatmentStarsRoot, root, CorrectStarsRootName);
        productControlStarsRoot = ResolveTransform(productControlStarsRoot, root, ProductStarsRootName);
        labelsText = ResolveText(labelsText, root, LabelsTextName);
        payText = ResolveText(payText, root, PayTextName);
        lossesText = ResolveText(lossesText, root, LossesTextName);
        finalPayText = ResolveText(finalPayText, root, FinalPayTextName);
        totalText = ResolveText(totalText, root, TotalTextName);
    }

    public bool Show(ResultData data)
    {
        bool hasStructuredUi =
            correctTreatmentStarsRoot != null ||
            productControlStarsRoot != null ||
            payText != null ||
            lossesText != null ||
            finalPayText != null ||
            totalText != null;

        if (!hasStructuredUi)
            return false;

        SetStars(correctTreatmentStarsRoot, data.treatmentQuality);
        SetStars(productControlStarsRoot, data.productControl);

        if (payText != null)
            payText.text = FormatMoney(data.potentialPay);

        if (lossesText != null)
            lossesText.text = "-" + FormatMoney(data.discount);

        if (finalPayText != null)
            finalPayText.text = FormatMoney(data.finalPay);

        if (totalText != null)
            totalText.text = FormatMoney(data.totalMoney);

        return true;
    }

    private void SetStars(Transform starsRoot, float score)
    {
        if (starsRoot == null || star0Sprite == null || star50Sprite == null || star100Sprite == null)
            return;

        List<Image> stars = GetOrderedStarImages(starsRoot);
        int halfUnits = Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(score) * stars.Count * 2f), 0, stars.Count * 2);

        for (int i = 0; i < stars.Count; i++)
        {
            int unitsForStar = Mathf.Clamp(halfUnits - i * 2, 0, 2);

            stars[i].sprite = unitsForStar switch
            {
                2 => star100Sprite,
                1 => star50Sprite,
                _ => star0Sprite
            };
        }
    }

    private List<Image> GetOrderedStarImages(Transform starsRoot)
    {
        Image[] images = starsRoot.GetComponentsInChildren<Image>(true);
        List<Image> result = new(images.Length);

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].transform == starsRoot)
                continue;

            result.Add(images[i]);
        }

        result.Sort((a, b) =>
        {
            RectTransform rectA = a.transform as RectTransform;
            RectTransform rectB = b.transform as RectTransform;
            float xA = rectA != null ? rectA.anchoredPosition.x : a.transform.localPosition.x;
            float xB = rectB != null ? rectB.anchoredPosition.x : b.transform.localPosition.x;
            return xA.CompareTo(xB);
        });

        return result;
    }

    private Transform ResolveTransform(Transform current, Transform root, string objectName)
    {
        if (current != null)
            return current;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == objectName)
                return children[i];
        }

        return null;
    }

    private TMP_Text ResolveText(TMP_Text current, Transform root, string objectName)
    {
        if (current != null)
            return current;

        Transform found = ResolveTransform(null, root, objectName);
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }

    private string FormatMoney(int amount)
    {
        return "$" + amount;
    }
}
