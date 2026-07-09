using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class RuntimeConfirmationDialog
{
    public static GameObject Create(
        Transform parent,
        string name,
        string message,
        string confirmLabel,
        string cancelLabel,
        UnityAction confirmAction,
        UnityAction cancelAction)
    {
        GameObject overlay = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        overlay.transform.SetParent(parent, false);

        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlay.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.65f);

        CanvasGroup overlayGroup = overlay.GetComponent<CanvasGroup>();
        overlayGroup.interactable = true;
        overlayGroup.blocksRaycasts = true;

        GameObject panel = new GameObject("DialogPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(overlay.transform, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(620f, 280f);

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.12f, 0.08f, 0.1f, 0.96f);

        CreateText(
            panel.transform,
            "Message",
            message,
            new Vector2(0f, 55f),
            new Vector2(540f, 110f),
            27f);

        CreateButton(
            panel.transform,
            "ConfirmButton",
            confirmLabel,
            new Vector2(-145f, -75f),
            new Color(0.74f, 0.26f, 0.26f, 1f),
            confirmAction);

        CreateButton(
            panel.transform,
            "CancelButton",
            cancelLabel,
            new Vector2(145f, -75f),
            new Color(0.24f, 0.42f, 0.32f, 1f),
            cancelAction);

        overlay.SetActive(false);
        return overlay;
    }

    private static void CreateText(Transform parent, string name, string text, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = position;
        textRect.sizeDelta = size;

        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.textWrappingMode = TextWrappingModes.Normal;
    }

    private static void CreateButton(Transform parent, string name, string label, Vector2 position, Color color, UnityAction action)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(250f, 60f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = color;

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);

        CreateText(buttonObject.transform, "Label", label, Vector2.zero, new Vector2(230f, 48f), 21f);
    }
}
