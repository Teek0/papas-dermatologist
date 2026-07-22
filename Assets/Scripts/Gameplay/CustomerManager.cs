using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using Random = UnityEngine.Random;

public class CustomerManager : MonoBehaviour
{
    private const float LegacyCustomerFadeDuration = 3.3f;
    private const float DefaultCustomerFadeDuration = 0.3f;

    public GameConstantsSO Constants;
    public Customer currentCustomer;

    [Header("Character Generation Options")]
    public Sprite[] BodyOptions;
    public Sprite[] HairOptions;
    public Sprite[] EyesOptions;

    [Header("Skin Condition Options")]
    public Sprite[] SkinConditionOptions_Forehead;
    public Sprite[] SkinConditionOptions_Cheeks;
    public Sprite[] SkinConditionOptions_Chin;

    [Header("Character Renderers")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer hairRenderer;
    [SerializeField] private SpriteRenderer eyesRenderer;
    [SerializeField] private SpriteRenderer foreheadRenderer;
    [SerializeField] private SpriteRenderer cheeksRenderer;
    [SerializeField] private SpriteRenderer chinRenderer;

    [Header("Dialogue GameObjects")]
    public GameObject DialogueBox;
    [SerializeField] private TMP_Text npcText;
    [SerializeField] private GameObject acceptButton;
    [SerializeField] private GameObject rejectButton;
    [SerializeField] private GameObject byeButton;
    [SerializeField] private TMP_Text rejectButtonText;
    public TextAsset dialogueOptions;
    private NPCDialogues dialogue;
    public static event Action<string> OnDialogueUpdate;

    [Header("Farewell Visual Feedback")]
    [SerializeField, Range(0f, 1f)] private float healedCoverageThreshold = 0.75f;
    [SerializeField, Range(0f, 1f)] private float improvedCoverageThreshold = 0.35f;
    [SerializeField, Range(0f, 1f)] private float maxWrongColorRateForVisualImprovement = 0.35f;
    [SerializeField, Range(0f, 1f)] private float improvedConditionAlpha = 0.45f;

    [Header("Customer Animation")]
    [SerializeField, Min(0.01f)] private float customerFadeInDuration = DefaultCustomerFadeDuration;
    [SerializeField, Min(0.01f)] private float customerFadeOutDuration = DefaultCustomerFadeDuration;

    [Header("Scene Transition")]
    [SerializeField] private string camillaSceneName = SceneNames.Camilla;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Day End")]
    [SerializeField] private SpriteRenderer receptionBackgroundRenderer;
    [SerializeField] private Sprite nightBackgroundSprite;
    [SerializeField] private GameObject finalThanksPanel;
    [SerializeField] private Image dayEndOverlay;
    [SerializeField] private CanvasGroup[] dayEndFadeGroups;
    [SerializeField] private AudioClip dayEndMusicClip;
    [SerializeField] private AudioSource dayEndMusicSource;
    [SerializeField] private AudioMixerGroup dayEndMusicGroup;
    [SerializeField, Range(0f, 1f)] private float dayEndMusicVolume = 1f;
    [SerializeField] private float dayEndMusicFadeInDuration = 1.5f;
    [SerializeField] private AudioClip dayEndAmbientClip;
    [SerializeField] private AudioSource dayEndAmbientSource;
    [SerializeField] private AudioMixerGroup dayEndAmbientGroup;
    [SerializeField, Range(0f, 1f)] private float dayEndAmbientVolume = 1f;
    [SerializeField] private float dayEndAmbientFadeInDuration = 1.5f;
    [SerializeField] private float dayEndDelay = 1.5f;
    [SerializeField] private float dayEndFadeDuration = 2.5f;
    [SerializeField, Range(0f, 1f)] private float dayEndOverlayAlpha = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool validateSkinConditionIndexContract = true;

    private float customerAlpha;

    private bool attending;
    private bool onHold;
    private bool customerSpawned;
    private bool dialogueVisible;
    private bool showingFarewell;
    private bool isDayEnding;
    private bool dayEndAudioStarted;
    private string defaultRejectButtonText;
    private float foreheadConditionAlpha = 1f;
    private float cheeksConditionAlpha = 1f;
    private float chinConditionAlpha = 1f;

    private float timeToNextPatient;
    private float waitingTime;

    private void OnValidate()
    {
        MigrateLegacyCustomerAnimationValues();
        customerFadeInDuration = Mathf.Max(0.01f, customerFadeInDuration);
        customerFadeOutDuration = Mathf.Max(0.01f, customerFadeOutDuration);
    }

    private void SpawnCustomer()
    {
        Sprite[][] skinConditions = { SkinConditionOptions_Forehead, SkinConditionOptions_Cheeks, SkinConditionOptions_Chin };
        currentCustomer = new Customer(Constants, BodyOptions, HairOptions, EyesOptions, skinConditions);

        if (!ResolveCustomerRenderers())
        {
            Debug.LogError("CustomerManager: faltan renderers del cliente. No se puede mostrar el paciente.");
            currentCustomer = null;
            return;
        }

        ApplyCustomerVisual(currentCustomer, 0f);

        dialogueVisible = false;
        SetDialogueButtonsForRequest();
        if (DialogueBox != null) DialogueBox.SetActive(false);
    }

    private bool ApplyCustomerVisual(Customer customer, float alpha)
    {
        if (customer == null || !ResolveCustomerRenderers())
            return false;

        foreheadRenderer.sprite = null;
        cheeksRenderer.sprite = null;
        chinRenderer.sprite = null;
        ResetConditionVisualAlphas();

        bodyRenderer.sprite = customer.Body;
        hairRenderer.sprite = customer.Hair;
        eyesRenderer.sprite = customer.Eyes;

        if (customer.Treatment?.SkinConditions != null)
        {
            for (int i = 0; i < customer.Treatment.SkinConditions.Count; i++)
            {
                var sc = customer.Treatment.SkinConditions[i];
                if (sc == null) continue;

                if (sc.AfflictedArea == "frente")
                    foreheadRenderer.sprite = sc.Sprite;
                else if (sc.AfflictedArea == "mejillas")
                    cheeksRenderer.sprite = sc.Sprite;
                else if (sc.AfflictedArea == "barbilla")
                    chinRenderer.sprite = sc.Sprite;
            }
        }

        customerAlpha = alpha;
        SetCustomerRenderersColor(new Color(1f, 1f, 1f, customerAlpha));
        return true;
    }

    private bool ResolveCustomerRenderers()
    {
        bodyRenderer = ResolveRenderer(bodyRenderer, "Body");
        hairRenderer = ResolveRenderer(hairRenderer, "Hair");
        eyesRenderer = ResolveRenderer(eyesRenderer, "Eyes");
        foreheadRenderer = ResolveRenderer(foreheadRenderer, "SkinConditions/Forehead");
        cheeksRenderer = ResolveRenderer(cheeksRenderer, "SkinConditions/Cheeks");
        chinRenderer = ResolveRenderer(chinRenderer, "SkinConditions/Chin");

        return bodyRenderer != null &&
               hairRenderer != null &&
               eyesRenderer != null &&
               foreheadRenderer != null &&
               cheeksRenderer != null &&
               chinRenderer != null;
    }

    private SpriteRenderer ResolveRenderer(SpriteRenderer current, string childPath)
    {
        if (current != null)
            return current;

        Transform child = transform.Find(childPath);
        return child != null ? child.GetComponent<SpriteRenderer>() : null;
    }

    private void SetCustomerRenderersColor(Color color)
    {
        if (!ResolveCustomerRenderers())
            return;

        bodyRenderer.color = color;
        hairRenderer.color = color;
        eyesRenderer.color = color;
        foreheadRenderer.color = WithAlphaMultiplier(color, foreheadConditionAlpha);
        cheeksRenderer.color = WithAlphaMultiplier(color, cheeksConditionAlpha);
        chinRenderer.color = WithAlphaMultiplier(color, chinConditionAlpha);
    }

    private Color WithAlphaMultiplier(Color color, float alphaMultiplier)
    {
        color.a *= Mathf.Clamp01(alphaMultiplier);
        return color;
    }

    private void ResetConditionVisualAlphas()
    {
        foreheadConditionAlpha = 1f;
        cheeksConditionAlpha = 1f;
        chinConditionAlpha = 1f;
    }

    private void CustomerFadeIn()
    {
        if (!ResolveCustomerRenderers())
            return;

        if (customerAlpha < 1f)
        {
            customerAlpha = Mathf.MoveTowards(customerAlpha, 1f, Time.deltaTime / customerFadeInDuration);
            SetCustomerRenderersColor(new Color(1f, 1f, 1f, customerAlpha));
        }
        else
        {
            attending = true;
        }
    }

    private void CustomerFadeOut()
    {
        if (!ResolveCustomerRenderers())
            return;

        if (customerAlpha > 0f)
        {
            customerAlpha = Mathf.MoveTowards(customerAlpha, 0f, Time.deltaTime / customerFadeOutDuration);
            SetCustomerRenderersColor(new Color(1f, 1f, 1f, customerAlpha));
        }
        else
        {
            attending = false;
            foreheadRenderer.sprite = null;
            cheeksRenderer.sprite = null;
            chinRenderer.sprite = null;

            dialogueVisible = false;
            if (DialogueBox != null) DialogueBox.SetActive(false);
        }
    }

    public List<SkinCondition> GetSkinConditions()
    {
        if (attending) return currentCustomer.Treatment.SkinConditions;
        throw new Exception("Not attending a customer");
    }

    public Customer CurrentCustomer => currentCustomer;

    private void SetText(string _message)
    {
        if (DialogueBox == null)
        {
            Debug.LogWarning("DialogueBox is null. Cannot set NPC dialogue text.");
            return;
        }

        if (npcText == null)
        {
            var textTf = DialogueBox.transform.Find("Text Box")?.Find("NPC Text");
            if (textTf != null)
                npcText = textTf.GetComponent<TMP_Text>();
        }

        if (npcText == null)
        {
            Debug.LogWarning("NPC Text missing. Assign npcText or keep DialogueBox/Text Box/NPC Text.");
            return;
        }

        npcText.text = _message;
        OnDialogueUpdate?.Invoke(_message);
    }


    private static string ExtractLine(object item)
    {
        if (item == null) return null;

        if (item is string s) return s;

        Type t = item.GetType();

        PropertyInfo p = t.GetProperty("line", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (p != null && p.PropertyType == typeof(string))
            return (string)p.GetValue(item);

        FieldInfo f = t.GetField("line", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (f != null && f.FieldType == typeof(string))
            return (string)f.GetValue(item);

        return null;
    }

    private string PickLineOrFallback(IList list, string fallback)
    {
        if (list == null || list.Count == 0) return fallback;

        int idx = Random.Range(0, list.Count);
        string line = ExtractLine(list[idx]);

        if (string.IsNullOrWhiteSpace(line)) return fallback;
        return line;
    }

    private void SetRandomText()
    {
        string greeting = "Hola. ";
        string preamble = "";
        string request = "¿Me puede ayudar con esto? ";
        string at = " Me molesta en ";
        string closing = ".";

        if (dialogue != null)
        {
            greeting = PickLineOrFallback(dialogue.greetings as IList, greeting);
            preamble = PickLineOrFallback(dialogue.preambles as IList, preamble);
            request = PickLineOrFallback(dialogue.treatmentRequest as IList, request);
            at = PickLineOrFallback(dialogue.pointingAt as IList, at);
            closing = PickLineOrFallback(dialogue.closing as IList, closing);
        }

        string message = greeting + preamble + request;

        List<string> afflictions = new();
        List<string> areas = new();

        if (currentCustomer?.Treatment?.SkinConditions != null)
        {
            for (int i = 0; i < currentCustomer.Treatment.SkinConditions.Count; i++)
            {
                var current = currentCustomer.Treatment.SkinConditions[i];
                if (current == null) continue;

                if (!string.IsNullOrEmpty(current.Type) && !afflictions.Contains(current.Type))
                    afflictions.Add(current.Type);

                if (!string.IsNullOrEmpty(current.AfflictedArea) && !areas.Contains(current.AfflictedArea))
                    areas.Add(current.AfflictedArea);
            }
        }

        List<string> afflictionLabels = new();

        for (int i = 0; i < afflictions.Count; i++)
        {
            string article = (afflictions[i] == "acné") ? "el " : "las ";
            afflictionLabels.Add(article + afflictions[i]);
        }

        message += JoinSpanishList(afflictionLabels);

        bool statingTheObvious = !(Random.Range(0, 3) == 1);

        if (statingTheObvious && areas.Count > 0)
        {
            message += at;

            List<string> areaLabels = new();

            for (int i = 0; i < areas.Count; i++)
            {
                if (areas[i] == "mejillas") areaLabels.Add("mis " + areas[i]);
                else areaLabels.Add("mi " + areas[i]);
            }

            message += JoinSpanishList(areaLabels);
        }

        message += closing;
        SetText(message);
    }

    private string JoinSpanishList(IList<string> items)
    {
        if (items == null || items.Count == 0)
            return "";

        if (items.Count == 1)
            return items[0];

        if (items.Count == 2)
            return items[0] + " y " + items[1];

        string result = "";

        for (int i = 0; i < items.Count; i++)
        {
            if (i == 0)
                result += items[i];
            else if (i == items.Count - 1)
                result += " y " + items[i];
            else
                result += ", " + items[i];
        }

        return result;
    }

    private bool TryShowPendingFarewell()
    {
        if (GameSession.I == null || !GameSession.I.HasPendingFarewell)
            return false;

        currentCustomer = GameSession.I.LastTreatedCustomer;
        if (!ApplyCustomerVisual(currentCustomer, 1f))
            return false;

        ApplyFarewellTreatmentVisuals(GameSession.I.LastTreatmentResult);

        showingFarewell = true;
        attending = true;
        customerSpawned = true;
        dialogueVisible = true;
        onHold = true;

        SetDialogueButtonsForFarewell();

        if (DialogueBox != null)
            DialogueBox.SetActive(true);

        SetText(BuildFarewellText(GameSession.I.LastTreatmentResult));
        return true;
    }

    private void ApplyFarewellTreatmentVisuals(TreatmentResultSummary result)
    {
        if (result == null || result.FinalPayment <= 0 || result.ConditionResults == null)
            return;

        for (int i = 0; i < result.ConditionResults.Count; i++)
        {
            TreatmentConditionResult conditionResult = result.ConditionResults[i];
            if (conditionResult == null)
                continue;

            float alpha = ConditionAlphaForResult(conditionResult);
            SetConditionAlphaForArea(conditionResult.Area, alpha);
        }

        SetCustomerRenderersColor(new Color(1f, 1f, 1f, customerAlpha));
    }

    private float ConditionAlphaForResult(TreatmentConditionResult conditionResult)
    {
        if (conditionResult.WrongColorRate > maxWrongColorRateForVisualImprovement)
            return 1f;

        if (conditionResult.CorrectCoverage >= healedCoverageThreshold)
            return 0f;

        if (conditionResult.CorrectCoverage < improvedCoverageThreshold)
            return 1f;

        float t = Mathf.InverseLerp(
            improvedCoverageThreshold,
            healedCoverageThreshold,
            conditionResult.CorrectCoverage
        );

        return Mathf.Lerp(1f, improvedConditionAlpha, t);
    }

    private void SetConditionAlphaForArea(string area, float alpha)
    {
        if (area == "frente")
            foreheadConditionAlpha = alpha;
        else if (area == "mejillas")
            cheeksConditionAlpha = alpha;
        else if (area == "barbilla")
            chinConditionAlpha = alpha;
    }

    private string BuildFarewellText(TreatmentResultSummary result)
    {
        if (result == null)
            return "Gracias por atenderme.";

        IList options = dialogue?.leaving;
        string fallback = "Gracias por atenderme.";

        switch (result.FeedbackTier)
        {
            case TreatmentFeedbackTier.Good:
                options = dialogue?.leavingGood as IList ?? options;
                fallback = "¡Quedó mucho mejor! Gracias por la atención.";
                break;
            case TreatmentFeedbackTier.Neutral:
                options = dialogue?.leavingNeutral as IList ?? options;
                fallback = "Creo que mejoró un poco. Gracias por atenderme.";
                break;
            case TreatmentFeedbackTier.Bad:
                options = dialogue?.leavingBad as IList ?? options;
                fallback = "No quedó como esperaba, pero gracias por intentarlo.";
                break;
            case TreatmentFeedbackTier.NoPay:
                options = dialogue?.leavingNoPay as IList ?? options;
                fallback = "Mejor lo dejamos hasta aquí. No puedo pagar por este tratamiento.";
                break;
        }

        return PickLineOrFallback(options, fallback);
    }

    private void ResolveDialogueControls()
    {
        if (DialogueBox == null)
            return;

        Transform buttons = DialogueBox.transform.Find("Buttons");

        if (acceptButton == null)
            acceptButton = buttons?.Find("AcceptButton")?.gameObject;

        if (rejectButton == null)
            rejectButton = buttons?.Find("RejectButton")?.gameObject;

        if (byeButton == null)
            byeButton = FindDialogueButton(buttons, "byeButton", "ByeButton", "BAdios", "AdiosButton");

        if (rejectButtonText == null && rejectButton != null)
            rejectButtonText = rejectButton.GetComponentInChildren<TMP_Text>(true);

        if (rejectButtonText != null && string.IsNullOrEmpty(defaultRejectButtonText))
            defaultRejectButtonText = rejectButtonText.text;
    }

    private void SetDialogueButtonsForRequest()
    {
        ResolveDialogueControls();

        if (acceptButton != null)
            acceptButton.SetActive(true);

        if (rejectButton != null)
            rejectButton.SetActive(true);

        if (byeButton != null)
            byeButton.SetActive(false);

        if (rejectButtonText != null && !string.IsNullOrEmpty(defaultRejectButtonText))
            rejectButtonText.text = defaultRejectButtonText;
    }

    private void SetDialogueButtonsForFarewell()
    {
        ResolveDialogueControls();

        if (acceptButton != null)
            acceptButton.SetActive(false);

        bool hasByeButton = byeButton != null;

        if (rejectButton != null)
            rejectButton.SetActive(!hasByeButton);

        if (byeButton != null)
            byeButton.SetActive(true);

        if (!hasByeButton && rejectButtonText != null)
            rejectButtonText.text = "Siguiente";
    }

    private GameObject FindDialogueButton(Transform parent, params string[] names)
    {
        if (parent == null)
            return null;

        for (int i = 0; i < names.Length; i++)
        {
            Transform found = parent.Find(names[i]);
            if (found != null)
                return found.gameObject;
        }

        return null;
    }

    public void rejectCustomer()
    {
        if (showingFarewell)
        {
            bool shouldShowDayEnd = GameSession.I != null && GameSession.I.HasPendingDayEnd;

            showingFarewell = false;
            customerSpawned = false;
            onHold = false;
            dialogueVisible = false;

            if (DialogueBox != null)
                DialogueBox.SetActive(false);

            if (GameSession.I != null)
                GameSession.I.CompletePendingFarewell();

            SetDialogueButtonsForRequest();

            if (shouldShowDayEnd)
                StartCoroutine(DayEndRoutine());

            return;
        }

        customerSpawned = false;
        onHold = false;

        dialogueVisible = false;
        if (DialogueBox != null) DialogueBox.SetActive(false);
    }

    public void acceptCustomer()
    {
        if (showingFarewell)
            return;

        if (GameSession.I != null)
            GameSession.I.SetCustomer(currentCustomer);
        else
            Debug.LogWarning("No existe GameSession en la escena. No se pudo guardar el customer.");

        dialogueVisible = false;
        if (DialogueBox != null) DialogueBox.SetActive(false);

        Time.timeScale = 1f;
        StartCoroutine(TransitionToCamilla());
    }

    private IEnumerator TransitionToCamilla()
    {
        yield return StartCoroutine(SceneTransitionService.FadeOutAndLoadScene(
            camillaSceneName,
            fadeCanvasGroup,
            1f,
            true,
            mainMixer,
            fadeOutDuration));
    }

    void Start()
    {
        MigrateLegacyCustomerAnimationValues();
        ResetDayEndOverlay();

        if (Constants == null)
        {
            Debug.LogError("CustomerManager: Constants no esta asignado.");
            enabled = false;
            return;
        }

        attending = false;
        customerSpawned = false;
        dialogueVisible = false;
        onHold = false;

        customerAlpha = 0f;

        timeToNextPatient = Random.Range(2, Constants.MaxWaitingTime);
        waitingTime = 0;

        if (DialogueBox != null) DialogueBox.SetActive(false);

        if (dialogueOptions != null && !string.IsNullOrEmpty(dialogueOptions.text))
            dialogue = NPCDialogues.CreateFromJSON(dialogueOptions.text);
        else
        {
            Debug.LogWarning("dialogueOptions is null/empty. Dialogue will use fallbacks.");
            dialogue = null;
        }

        ResolveDialogueControls();
        SetDialogueButtonsForRequest();

        bool pendingDayEnd = GameSession.I != null && GameSession.I.HasPendingDayEnd;

        if (pendingDayEnd)
        {
            isDayEnding = true;
            ApplyNightReception();
            StartCoroutine(StartDayEndAudioForNightReception());
        }

        if (TryShowPendingFarewell())
            return;

        if (pendingDayEnd)
            StartCoroutine(DayEndRoutine());

        if (validateSkinConditionIndexContract)
            ValidateSkinConditionIndexContract();
    }

    private void MigrateLegacyCustomerAnimationValues()
    {
        if (Mathf.Approximately(customerFadeInDuration, LegacyCustomerFadeDuration))
            customerFadeInDuration = DefaultCustomerFadeDuration;

        if (Mathf.Approximately(customerFadeOutDuration, LegacyCustomerFadeDuration))
            customerFadeOutDuration = DefaultCustomerFadeDuration;
    }

    private IEnumerator DayEndRoutine()
    {
        isDayEnding = true;
        ResolveDayEndReferences();
        ApplyNightReception();
        PlayDayEndAudio();

        if (finalThanksPanel != null)
            finalThanksPanel.SetActive(false);

        Image overlay = ResolveDayEndOverlay();
        Coroutine customerExitRoutine = StartCoroutine(DayEndCustomerExitRoutine());

        if (overlay != null)
        {
            float elapsed = 0f;

            while (elapsed < dayEndFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / dayEndFadeDuration);
                float alpha = Mathf.Lerp(0f, dayEndOverlayAlpha, progress);
                overlay.color = new Color(0f, 0f, 0f, alpha);
                SetReceptionInterfaceAlpha(1f - progress);
                yield return null;
            }

            overlay.color = new Color(0f, 0f, 0f, dayEndOverlayAlpha);
        }

        SetReceptionInterfaceAlpha(0f);

        if (customerExitRoutine != null)
            yield return customerExitRoutine;

        if (dayEndDelay > 0f)
            yield return new WaitForSeconds(dayEndDelay);

        HideReceptionForDayEnd();

        if (finalThanksPanel != null)
        {
            finalThanksPanel.SetActive(true);
            finalThanksPanel.transform.SetAsLastSibling();
        }

        if (GameSession.I != null)
            GameSession.I.CompletePendingDayEnd();
    }

    private void ResolveDayEndReferences()
    {
        if (finalThanksPanel == null)
            finalThanksPanel = FindSceneObject("FinalThanks");

        if (receptionBackgroundRenderer == null)
        {
            GameObject background = FindSceneObject("Recepcion_01_01 1_0");
            if (background == null)
                background = FindSceneObject("Background");

            if (background != null)
                receptionBackgroundRenderer = background.GetComponent<SpriteRenderer>();
        }
    }

    private void ApplyNightReception()
    {
        ResolveDayEndReferences();

        if (nightBackgroundSprite == null)
            return;

        if (receptionBackgroundRenderer != null)
            receptionBackgroundRenderer.sprite = nightBackgroundSprite;

        GameObject backgroundRoot = FindSceneObject("Background");
        if (backgroundRoot == null)
            return;

        SpriteRenderer[] renderers = backgroundRoot.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].sprite = nightBackgroundSprite;
    }

    private Image ResolveDayEndOverlay()
    {
        if (dayEndOverlay != null)
        {
            dayEndOverlay.color = new Color(0f, 0f, 0f, 0f);
            dayEndOverlay.raycastTarget = true;
            dayEndOverlay.transform.SetAsLastSibling();
            return dayEndOverlay;
        }

        Debug.LogWarning("CustomerManager: dayEndOverlay is null. Assign ReceptionScene > UI Canvas > DayEndFadeOverlay in the Inspector.");
        return null;
    }

    private void ResetDayEndOverlay()
    {
        if (dayEndOverlay == null)
            return;

        dayEndOverlay.color = new Color(0f, 0f, 0f, 0f);
        dayEndOverlay.raycastTarget = false;
    }

    private void HideReceptionForDayEnd()
    {
        if (DialogueBox != null)
            DialogueBox.SetActive(false);

        SetCustomerRenderersColor(new Color(1f, 1f, 1f, 0f));
        SetSceneObjectActive("CurrencyBox", false);
        SetSceneObjectActive("DailyQuotaBox", false);
    }

    private IEnumerator DayEndCustomerExitRoutine()
    {
        while (customerAlpha > 0f)
        {
            customerAlpha = Mathf.MoveTowards(customerAlpha, 0f, Time.unscaledDeltaTime / customerFadeOutDuration);
            SetCustomerRenderersColor(new Color(1f, 1f, 1f, customerAlpha));
            yield return null;
        }

        SetCustomerRenderersColor(new Color(1f, 1f, 1f, 0f));
    }

    private void SetReceptionInterfaceAlpha(float alpha)
    {
        if (dayEndFadeGroups == null || dayEndFadeGroups.Length == 0)
            return;

        for (int i = 0; i < dayEndFadeGroups.Length; i++)
        {
            CanvasGroup canvasGroup = dayEndFadeGroups[i];

            if (canvasGroup == null)
                continue;

            canvasGroup.alpha = alpha;
            canvasGroup.interactable = alpha > 0.01f;
            canvasGroup.blocksRaycasts = alpha > 0.01f;
        }
    }

    private void PlayDayEndAudio()
    {
        StopNormalReceptionAudio();

        if (dayEndAudioStarted)
            return;

        dayEndAudioStarted = true;

        PlayDayEndClip(dayEndMusicSource, dayEndMusicClip, dayEndMusicGroup, dayEndMusicVolume, dayEndMusicFadeInDuration);
        PlayDayEndClip(dayEndAmbientSource, dayEndAmbientClip, dayEndAmbientGroup, dayEndAmbientVolume, dayEndAmbientFadeInDuration);
    }

    private IEnumerator StartDayEndAudioForNightReception()
    {
        PlayDayEndAudio();
        yield return null;
        StopNormalReceptionAudio();
    }

    private void StopNormalReceptionAudio()
    {
        SceneMusicPlayer sceneMusic = FindFirstObjectByType<SceneMusicPlayer>();
        if (sceneMusic != null)
            sceneMusic.Stop();

        AmbienceManager ambience = FindFirstObjectByType<AmbienceManager>();
        if (ambience != null)
            ambience.StopAmbience();
    }

    private void PlayDayEndClip(
        AudioSource source,
        AudioClip clip,
        AudioMixerGroup mixerGroup,
        float targetVolume,
        float fadeInDuration)
    {
        if (source == null || clip == null)
            return;

        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.outputAudioMixerGroup = mixerGroup;
        source.clip = clip;
        source.volume = 0f;
        source.Play();

        StartCoroutine(FadeDayEndSource(source, targetVolume, fadeInDuration));
    }

    private IEnumerator FadeDayEndSource(AudioSource source, float targetVolume, float fadeInDuration)
    {
        if (source == null)
            yield break;

        if (fadeInDuration <= 0f)
        {
            source.volume = targetVolume;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, Mathf.Clamp01(elapsed / fadeInDuration));
            yield return null;
        }

        source.volume = targetVolume;
    }

    private void SetSceneObjectActive(string objectName, bool isActive)
    {
        GameObject sceneObject = FindSceneObject(objectName);

        if (sceneObject != null && sceneObject != finalThanksPanel)
            sceneObject.SetActive(isActive);
    }

    private GameObject FindSceneObject(string objectName)
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].gameObject.scene != gameObject.scene)
                continue;

            if (transforms[i].name == objectName)
                return transforms[i].gameObject;
        }

        return null;
    }

    void Update()
    {
        if (isDayEnding) return;
        if (onHold) return;

        if (!attending)
        {
            if (!customerSpawned)
            {
                if (waitingTime >= timeToNextPatient)
                {
                    SpawnCustomer();
                    timeToNextPatient = Random.Range(3, Constants.MaxWaitingTime);
                    customerSpawned = true;
                    waitingTime = 0;
                }
                else
                {
                    waitingTime += Time.deltaTime;
                }
            }
            else
            {
                CustomerFadeIn();
            }
        }
        else
        {
            if (customerSpawned)
            {
                if (!dialogueVisible)
                {
                    if (DialogueBox != null) DialogueBox.SetActive(true);
                    SetDialogueButtonsForRequest();
                    SetRandomText();

                    dialogueVisible = true;
                    onHold = true;
                }
            }
            else
            {
                if (DialogueBox != null) DialogueBox.SetActive(false);
                dialogueVisible = false;
                CustomerFadeOut();
            }
        }
    }

    private void ValidateSkinConditionIndexContract()
{
    Sprite[][] opts = { SkinConditionOptions_Forehead, SkinConditionOptions_Cheeks, SkinConditionOptions_Chin };
    string[] areaName = { "frente", "mejillas", "mentón" };
    string[] expectedByIndex = { "acné", "arrugas", "cicatrices" };

    for (int a = 0; a < opts.Length; a++)
    {
        var row = opts[a];
        if (row == null)
        {
            Debug.LogError($"[IndexContract] {areaName[a]}: lista null");
            continue;
        }

        if (row.Length < expectedByIndex.Length)
        {
            Debug.LogWarning($"[IndexContract] {areaName[a]}: length={row.Length}. " +
                             $"Contrato incompleto (se esperan 3 índices: 0..2).");
        }

        int max = Mathf.Min(row.Length, expectedByIndex.Length);
        for (int i = 0; i < max; i++)
        {
            var sp = row[i];
            string expected = expectedByIndex[i];

            if (sp == null)
            {
                Debug.LogWarning($"[IndexContract] {areaName[a]} idx={i} (esperado {expected}): sprite null");
                continue;
            }

            string inferred = InferConditionTypeFromSpriteName(sp.name);

            // Si no se puede inferir, no acusamos mismatch, solo avisamos.
            if (inferred == "desconocido")
            {
                Debug.LogWarning($"[IndexContract] {areaName[a]} idx={i} (esperado {expected}): " +
                                 $"no se pudo inferir tipo desde sprite='{sp.name}'.");
                continue;
            }

            if (inferred != expected)
            {
                Debug.LogError($"[IndexContract] MISMATCH en {areaName[a]} idx={i}. " +
                               $"Se esperaba '{expected}' pero sprite='{sp.name}' parece '{inferred}'. " +
                               $"(Probable arrastre/orden incorrecto en Inspector).");
            }
        }
    }
}

private string InferConditionTypeFromSpriteName(string spriteName)
{
    if (string.IsNullOrWhiteSpace(spriteName))
        return "desconocido";

    string n = spriteName.ToLowerInvariant();

    // acné
    if (n.Contains("acne") || n.Contains("espinilla") || n.Contains("espinillas"))
        return "acné";

    // arrugas
    if (n.Contains("arruga") || n.Contains("arrugas") || n.Contains("wrinkle"))
        return "arrugas";

    // cicatrices
    if (n.Contains("cicatriz") || n.Contains("cicatrices") || n.Contains("scar"))
        return "cicatrices";

    return "desconocido";
}

}
